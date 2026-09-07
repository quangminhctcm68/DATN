using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace nickeltin.SDF.Editor
{
    /// <summary>
    /// Second step of import.
    /// We get generation backend from import settings here, and ensure its dependencies.
    /// If dependencies is not yet loaded import stops.
    /// Otherwise if texture is not sprite the regular SDFTex is generated, for plain texture.
    /// TODO: in future maybe return non-sprite textures support
    /// </summary>
    internal class PostprocessTexture : ImportStep
    {
        public override ProcessResult Process(SDFImportContext sdfCtx, AssetImportContext astCtx)
        {
            if (sdfCtx.Texture == null)
            {
                return ProcessResult.End(false);
            }

            sdfCtx.Backend = SDFGenerationBackend.Provider.GetBackend(sdfCtx.ImportSettings.SDFGenerationBackend);
            
            // Adding dependencies to texture.
            // If any dependency is not yet imported aborting import.
            // Texture will be re-imported after dependencies is imported.
            if (!EnsureDependencies(sdfCtx.Backend, astCtx))
            {
                return ProcessResult.End(false);
            }

            // Creating texture and blitting original to work with unified format texture.
            sdfCtx.TextureCopy = CopyTex(sdfCtx.Texture);
            
            
            // Calculating imported texture scale. Texture might be clamped by max size, we need to know for how much.
            sdfCtx.ImportedTextureScale = SDFGraphicsUtil.GetImportedTextureScale(sdfCtx.TextureImporter, sdfCtx.Texture);
            
            
            // Border offset needs to be adjusted because texture size might be different
            var borderOffset =
                SDFGraphicsUtil.GetAdjustedBorderOffset(sdfCtx.TextureImporter, sdfCtx.Texture, sdfCtx.ImportSettings.BorderOffset);
            sdfCtx.AdjustedBorderOffset = borderOffset;

            // var texContainer = CreateTextureContainer(sdfCtx.Texture, borderOffset);
            // sdfCtx.TextureContainer = texContainer;
            // astCtx.AddObjectToAsset("SDFTextureContainer", texContainer);
            
            
            // TODO: currently not supporting plain texture, only sprites
            // If plain texture then import process ends here
            // if (sdfCtx.Importer.textureType != TextureImporterType.Sprite)
            // {
            //     var sdfTex = GenerateTexture(sdfCtx.Importer, sdfCtx.TextureCopy, sdfCtx.AdjustedBorderOffset, 
            //         sdfCtx.ImportedTextureScale, sdfCtx.Backend, sdfCtx.ImportSettings);
            //     sdfTex.name = sdfCtx.Texture.name;
            //     // Settings generated texture, if texture in sprite mode it will be setted in PostprocessSprites step.
            //     // texContainer._generatedTexture = sdfTex;
            //     astCtx.AddObjectToAsset("SDFTex", sdfTex);
            //     return ProcessResult.End(true);
            // }
            
            return ProcessResult.Continue();
        }

        protected virtual Texture CopyTex(Texture2D texture)
        {
            return CopyTex(texture, true);
        }
        
        // private static SDFTextureContainer CreateTextureContainer(Texture sourceTex, Vector4 borderOffset)
        // {
        //     var texContainer = ScriptableObject.CreateInstance<SDFTextureContainer>();
        //     texContainer.hideFlags = HideFlags.HideInHierarchy;
        //     texContainer._sourceTexture = sourceTex;
        //     texContainer._borderOffset = borderOffset;
        //     texContainer.name = sourceTex.name;
        //     return texContainer;
        // }
        
        /// <summary>
        /// Creates render texture copy of main texture
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="copyByInstantiation">For asset postprocess direct read from texture for some reason throws error</param>
        protected static Texture CopyTex(Texture2D texture, bool copyByInstantiation)
        {
            var rtDesc = new RenderTextureDescriptor(texture.width, texture.height, GraphicsFormat.R8G8B8A8_UNorm, 0, 0);
            var rt = RenderTexture.GetTemporary(rtDesc);
            rt.filterMode = texture.filterMode;
            rt.wrapMode = texture.wrapMode;

            Graphics.Blit(Texture2D.blackTexture, rt);
            if (copyByInstantiation)
            { 
                var texCpy = Object.Instantiate(texture);
                Graphics.Blit(texCpy, rt);
                Object.DestroyImmediate(texCpy);
            }
            else
            {
                Graphics.Blit(texture, rt);
            }
            return rt;
        }
        
        /// <summary>
        /// Generates SDFTex for plain texture, not a sprite texture.
        /// </summary>
        private static Texture2D GenerateTexture(TextureImporter importer, Texture tex, Vector4 borderOffset, float textureScale, 
            SDFGenerationBackend.Context backend, SDFImportSettings settings)
        {
            importer.GetSourceTextureWidthAndHeight(out var sourceW, out var sourceH);
            textureScale *= SDFGraphicsUtil.GetGradientSizeAdjustment(sourceW, sourceH);
            var backendSettings = GetSettingsForBackend(settings, borderOffset, tex.width, tex.height, textureScale);

            
            var sdfTex = SDFGraphicsUtil.GenerateSDF(tex,
                new RectInt(0, 0, tex.width, tex.height), backendSettings, backend);
            
            sdfTex.filterMode = tex.filterMode;
            sdfTex.wrapMode = tex.wrapMode;
            
            return sdfTex;
        }
        
        /// <summary>
        /// Ensures all SDF backend dependencies by adding them to <paramref name="importContext"/> with <see cref="AssetImportContext.DependsOnArtifact(GUID)"/>.
        /// </summary>
        /// <param name="backend"></param>
        /// <param name="importContext"></param>
        /// <returns>Returns true is all dependencies is already imported</returns>
        private static bool EnsureDependencies(SDFGenerationBackend.Context backend, AssetImportContext importContext)
        {
            var allDependenciesImported = true;
            foreach (var guid in backend.ArtifactDependencies)
            {
                importContext.DependsOnArtifact(guid);
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (allDependenciesImported)
                {
                    var dependency = AssetDatabase.LoadAssetAtPath(path, typeof(Object));
                    if (dependency == null)
                    {
                        allDependenciesImported = false;
                    }
                }
            }

            return allDependenciesImported;
        }
    }
}