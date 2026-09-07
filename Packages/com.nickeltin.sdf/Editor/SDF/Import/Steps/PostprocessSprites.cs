using System;
using System.Linq;
using System.Text;
using nickeltin.InternalBridge.Editor;
using nickeltin.SDF.InternalBridge.Editor;
using nickeltin.SDF.Runtime;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEditor.U2D;
using UnityEngine;
using Object = UnityEngine.Object;

namespace nickeltin.SDF.Editor
{
    /// <summary>
    /// Third step for import.
    /// We generate packed texture and meta assets for sprites here
    /// </summary>
    internal class PostprocessSprites : ImportStep
    {
        public const string SDF_TEXTURE_POSTFIX = "(SDFTexture)";
        public const string SDF_SPRITE_POSTFIX = "(SDFSprite)";
        public const string SDF_SPRITE_META_ASSET_POSTFIX = "(SDFSpriteMetadataAsset)";

        protected HideFlags SDFSpriteMetadataHideFlags = HideFlags.HideInHierarchy;
        protected bool IsSDFSpriteMetadataDecoupled = false;
        /// <summary>
        /// If set to true will use sprite GUID (Legacy) way to bind to sprites.
        /// If false will use sprite InternalID which is more consistent.
        /// </summary>
        protected bool UseSpriteGUIDToBindAssets = true;
        
        /// <summary>
        /// Result of SDF generation for sprites
        /// </summary>
        public struct GenerationResult
        {
            /// <summary>
            /// Packed texture for sdf's, all sprites are present here
            /// </summary>
            public readonly Texture2D PackedTexture;

            public readonly Sprite[] PackedSprites;
            
            /// <summary>
            /// How much sprites is scaled from their original size to fit into texture atlas
            /// </summary>
            public readonly float[] SpritesScale;

            public SDFSpriteMetadataAsset[] MetadataAssets;

            public GenerationResult(Texture2D packedTexture, Sprite[] packedSprites, float[] spritesScale)
            {
                PackedTexture = packedTexture;
                PackedSprites = packedSprites;
                SpritesScale = spritesScale;
                MetadataAssets = Array.Empty<SDFSpriteMetadataAsset>();
            }
        }

        protected virtual int GetPackedTextureMaxSize(SDFImportContext sdfCtx, AssetImportContext astCtx)
        {
            return sdfCtx.TextureImporter.maxTextureSize;
        }
        
        public sealed override ProcessResult Process(SDFImportContext sdfCtx, AssetImportContext astCtx)
        {
            // Performing some additional validation for provided sprite data
            var spriteNames = sdfCtx.Sprites.Select(sprite => sprite.name).ToList();
            var spriteRectNames = sdfCtx.TextureImporter.GetSpriteRects().Select(rect => rect.name).ToList();
            
            // Simple case where provided sprite count is different from import settings
            if (spriteNames.Count != spriteRectNames.Count)
            {
                Debug.LogError(
                    $"Can't import SDF, source sprites count ({spriteNames.Count}) not matching import settings sprite count ({spriteRectNames.Count}).",
                    sdfCtx.MainAssetImporter);
                return ProcessResult.End(false);
            }
            
            // Case where sprite order is not the same as their rects in import settings
            // This case was a pain to find in decoupled pipeline
            if (!spriteNames.SequenceEqual(spriteRectNames))
            {
                Debug.LogError($"Can't import SDF, source sprite sequence not matching import settings sprite rects.\n" +
                               $"Source sprites: {string.Join(", ", spriteNames)}\n" + 
                               $"Source sprite rects: {string.Join(", ", spriteRectNames)}", 
                    sdfCtx.MainAssetImporter);
                return ProcessResult.End(false);
            }
            
            Generate_Internal(ref sdfCtx, astCtx);
            return ProcessResult.End(true);
        }

        protected virtual void Generate_Internal(ref SDFImportContext sdfCtx, AssetImportContext astCtx)
        {
            Generate(ref sdfCtx, astCtx);
        }

        protected GenerationResult Generate(ref SDFImportContext sdfCtx, AssetImportContext astCtx)
        {
            var spriteRects = sdfCtx.TextureImporter.GetSpriteRects();
            var maxSize = GetPackedTextureMaxSize(sdfCtx, astCtx);
            var result = GeneratePackedTextures(sdfCtx.TextureCopy, sdfCtx.AdjustedBorderOffset, 
                sdfCtx.ImportedTextureScale, maxSize, sdfCtx.Sprites, spriteRects, 
                sdfCtx.Backend, sdfCtx.ImportSettings);
            
            
            // Previously name of the texture was Texture.name + (SDF) but addressable for some reason referencing
            // sub-assets by parent asset GUID and sub-asset name, so unique names is required
            result.PackedTexture.name = sdfCtx.Texture.name + SDF_TEXTURE_POSTFIX;
            
            sdfCtx.AddSubAsset("PackedSDFTex", result.PackedTexture);

            var sourceTextureGUID = AssetDatabase.AssetPathToGUID(sdfCtx.TextureImporter.assetPath);
            var importResult = new SDFImportResult(new long[sdfCtx.Sprites.Length], 
                new long[sdfCtx.Sprites.Length], sourceTextureGUID);
            
            var metadataAssets = new SDFSpriteMetadataAsset[sdfCtx.Sprites.Length];
            for (var i = 0; i < sdfCtx.Sprites.Length; i++)
            {
                var sdfSprite = result.PackedSprites[i];
                var sprite = sdfCtx.Sprites[i];
                var spriteRect = spriteRects[i];
                
                // Sprite ID is basically project-wide unique ID, just in case we adding SDF postfix and using it as local identifier
                var sdfSpriteStrId = GetUniqueLocalID(spriteRect, "SDFSprite", UseSpriteGUIDToBindAssets);
                sdfSprite.SetSpriteID(new GUID(sdfSpriteStrId));
                
                // Previously name of the sprite was Sprite.name + (SDF) but addressable for some reason referencing
                // sub-assets by parent asset GUID and sub-asset name, so unique names is required
                sdfSprite.name += SDF_SPRITE_POSTFIX;
                sdfCtx.AddSubAsset(sdfSpriteStrId, sdfSprite);
                
                // Creating meta asset
                var metadataAsset = CreateSpriteMetadataAsset(sprite, sdfSprite, sdfCtx.AdjustedBorderOffset);
                metadataAssets[i] = metadataAsset;
                var metaAssetStrId = GetUniqueLocalID(spriteRect, "SDFSpriteMetadata", UseSpriteGUIDToBindAssets);
                
                // Generating preview of meta asset
                var spriteSize = sprite.rect.size;
                var thumbnail = _SpriteUtility.RenderStaticPreview(sprite, Color.white, (int)spriteSize.x, (int)spriteSize.y);
                sdfCtx.AddSubAsset(metaAssetStrId, metadataAsset, thumbnail);
                
                // Calculating internalID for meta asset, custom type is MonoBehaviour 114 - any script defined object.
                importResult.MetaAssetsLocalIDs[i] = sdfCtx.MainAssetImporter.MakeInternalIDForCustomType(metaAssetStrId);
                // Sprite internalID is 21300000 for single mode, and saved id in sprite rect (or loaded from table in importer) for multiple.
                importResult.SourceSpritesLocalIDs[i] = GetSourceSpriteLocalID(sdfCtx, spriteRect);
            }
            
            sdfCtx.ResultArtifact = importResult;
            result.MetadataAssets = metadataAssets;
            return result;
        }
        
        protected virtual long GetSourceSpriteLocalID(SDFImportContext sdfCtx, SpriteRect spriteRect)
        {
            if (sdfCtx.TextureImporter.spriteImportMode == SpriteImportMode.Multiple)
            {
                // For multiple sprites hash code of their guid is used
                return spriteRect.GetInternalID();
            }

            // 213 stands for sprite type, see more at: https://docs.unity3d.com/Manual/ClassIDReference.html
            // If single sprite mode sprite file id will always be 21300000 (don't know why zeros are added)
            return 21300000;
        }
        
        /// <summary>
        /// Will use base sprite id/guid and then append some offset/postfix to it.
        /// <paramref name="useSpriteGUID"/> determines is using old way with <see cref="SpriteEditorExtension.GetSpriteID"/>
        /// use it for assets imported from older version.
        /// 
        /// Sprite guid is bad since its unique project-wide and is reassigned upon sprite rect creation,
        /// meaning if you delete sprite rect and the create identical one guid will be different.
        /// However <see cref="AssetImporter"/> writes to its meta file 'internalIDToNameTable',
        /// to make 'internalID' consistent over different sprite rects.
        /// Use internalID instead of spriteGUID, this is preferable to keep references intact.
        /// </summary>
        private static string GetUniqueLocalID(SpriteRect sprite, string addition, bool useSpriteGUID = true)
        {
            var hash = new Hash128();
            if (useSpriteGUID)
            {
                // ReSharper disable once SuggestVarOrType_SimpleTypes
                // Using defined type to use exact Append overload
                GUID guid = sprite.spriteID;
                hash.Append(ref guid);
            }
            else
            {
                // ReSharper disable once SuggestVarOrType_BuiltInTypes
                // Using defined type to use exact Append overload
                long internalID = sprite.GetInternalID();
                hash.Append(ref internalID);
            }
            hash.Append(addition);
            return hash.ToString();
        }
        
        private SDFSpriteMetadataAsset CreateSpriteMetadataAsset(Sprite sourceSprite, Sprite sdfSprite, Vector4 borderOffset)
        {
            var asset = ScriptableObject.CreateInstance<SDFSpriteMetadataAsset>();
            asset._isImportedDecoupled = IsSDFSpriteMetadataDecoupled;
            // Settings hide flags to prevent visible scriptable object from becoming the main asset.
            // This is some of the stupid unit behaviour...
            // AssetImporter chooses himself what main asset is, you can't control this in AssetPostprocessor
            asset.hideFlags = SDFSpriteMetadataHideFlags;
            asset._metadata = new SDFSpriteMetadata(sourceSprite, sdfSprite, borderOffset);
            
            // Previously name of the meta asset was == to sourceSprite.name but addressable for some reason referencing
            // sub-assets by parent asset GUID and sub-asset name, so unique names is required.
            // This is done for testing since addressables probably don't even support hidden assets
            asset.name = sourceSprite.name + SDF_SPRITE_META_ASSET_POSTFIX;
            
            AssignNewPipelineSpriteMetadata(sourceSprite, sdfSprite, asset);
            return asset;
        }

        protected virtual void AssignNewPipelineSpriteMetadata(Sprite sourceSprite, Sprite sdfSprite, SDFSpriteMetadataAsset asset)
        {
#if SDF_NEW_SPRITE_METADATA
            sourceSprite.AddScriptableObject(asset);
            sdfSprite.AddScriptableObject(asset);
#endif
        }

        /// <summary>
        /// Generates sdf for each sprite and packs all sprites from texture to one atlas texture.
        /// </summary>
        private static GenerationResult GeneratePackedTextures(Texture texture, Vector4 borderOffset,
            float textureScale, int maxTextureSize,
            Sprite[] sprites, SpriteRect[] spriteRects,
            SDFGenerationBackend.Context backend, SDFImportSettings settings)
        {
            var spriteTextures = new Texture2D[sprites.Length];
            var packedSprites = new Sprite[sprites.Length];
            var spritesScales = new float[sprites.Length];
            
            for (var i = 0; i < sprites.Length; i++)
            {
                var sprite = sprites[i];
                var meta = spriteRects[i];

                var adjustedTexScale = textureScale * SDFGraphicsUtil.GetGradientSizeAdjustment((int)meta.rect.width, (int)meta.rect.height);
                
                var backendSettings = GetSettingsForBackend(settings, borderOffset, 
                    (int)sprite.rect.width, (int)sprite.rect.height, adjustedTexScale);

                var sdfTex = SDFGraphicsUtil.GenerateSDF(texture, sprite.rect.ToRectInt(), backendSettings, backend);

                spriteTextures[i] = sdfTex;
            }

            Texture2D CreateTexForPacking()
            {
                return new Texture2D(SDFGraphicsUtil.MIN_TEX_SIZE, SDFGraphicsUtil.MIN_TEX_SIZE, TextureFormat.Alpha8, false) 
                {
                    filterMode = texture.filterMode,
                    wrapMode = texture.wrapMode,
                };
            }

            var packedSDF = CreateTexForPacking();
            var rects = packedSDF.PackTextures(spriteTextures, 0, maxTextureSize);
            packedSDF = SDFGraphicsUtil.ConvertToAlpha8(packedSDF);

            // If min size is to small packing might fail
            if (rects == null)
            {
                Debug.LogError($"Can't pack to min size of {maxTextureSize}");
            }
            
            // Create SDF sprites after packing all their texture to atlas
            for (var i = 0; i < sprites.Length; i++)
            {
                var sprite = sprites[i];
                var pixelRect = UVToTextureRect(packedSDF, rects != null ? rects[i] : new Rect(0,0,packedSDF.width, packedSDF.height));
                
                // Pixel per unity and borders should be multiplied by resolution scale.
                // Also by its scaling factor from packed texture, it might became smaller
                var spriteScale = pixelRect.width / spriteTextures[i].width; // How much smaller sprite has became after packing to atlas
                var scaledBorderOffset = borderOffset * settings.ResolutionScale * spriteScale;
                
                var pixelsPerUnit = sprite.pixelsPerUnit * settings.ResolutionScale * spriteScale;
                var border = sprite.border * settings.ResolutionScale * spriteScale + scaledBorderOffset;
                
                // Sprite might have custom pivot that is not center, then we need to offset its sprite with consideration of border offset
                var pivot = spriteRects[i].pivot;
                RemapPivot(ref pivot, pixelRect, scaledBorderOffset);
                
                var packedSprite = Sprite.Create(packedSDF, pixelRect, pivot, 
                    pixelsPerUnit, 0, SpriteMeshType.FullRect, 
                    border, false);
                
                packedSprite.name = sprite.name;
                packedSprites[i] = packedSprite;
                spritesScales[i] = spriteScale;
            }
            
            //Destroy temp textures that is now packed in two atlas textures
            foreach (var tex in spriteTextures) Object.DestroyImmediate(tex);
            
            return new GenerationResult(packedSDF, packedSprites, spritesScales);
        }


        /// <summary>
        /// Remaps pivot of sprite to remain in same spot, even when border offset is added
        /// </summary>
        /// <param name="pivot">Original pivot</param>
        /// <param name="spriteRect">Sprite rect with applied border offset</param>
        /// <param name="borderOffset">Applied border offset</param>
        private static void RemapPivot(ref Vector2 pivot, Rect spriteRect, Vector4 borderOffset)
        {
            var spriteSizeWithoutBorderOffset =
                new Vector2(spriteRect.width - borderOffset.x - borderOffset.z,
                    spriteRect.height - borderOffset.y - borderOffset.w);
            
            var positionInSpriteWithoutBorder = 
                new Vector2(Mathf.Lerp(0, spriteSizeWithoutBorderOffset.x, pivot.x),
                    Mathf.Lerp(0, spriteSizeWithoutBorderOffset.y, pivot.y));

            var spriteBoundsWithBorderOffsetInPlace = new Rect(-borderOffset.x, -borderOffset.y,
                spriteRect.width, spriteRect.height);

            pivot = SDFMath.InverseLerp(spriteBoundsWithBorderOffsetInPlace.min,
                spriteBoundsWithBorderOffsetInPlace.max, positionInSpriteWithoutBorder);
        }
        
        public static Rect UVToTextureRect(Texture tex, Rect uvRect)
        {
            var minX = (int)Mathf.Lerp(0, tex.width, uvRect.xMin);
            var minY = (int)Mathf.Lerp(0, tex.height, uvRect.yMin);
            var maxX = (int)Mathf.Lerp(0, tex.width, uvRect.xMax);
            var maxY = (int)Mathf.Lerp(0, tex.height, uvRect.yMax);
            var width = Mathf.Abs(minX - maxX);
            var height = Mathf.Abs(minY - maxY);
            return new Rect(minX, minY, width, height);
        }
    }
}