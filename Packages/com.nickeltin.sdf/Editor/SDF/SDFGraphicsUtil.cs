using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using Object = UnityEngine.Object;

namespace nickeltin.SDF.Editor
{
    /// <summary>
    /// Helper to work with render textures
    /// </summary>
    internal static class SDFGraphicsUtil
    {
        public const int MIN_TEX_SIZE = 4;
        public const int REFERENCE_GRADIENT_IMAGE_SIZE = 512;


        #region Math Helpers

        public static float GetImportedTextureScale(int sourceTexW, int importedTexW)
        {
            return (float)importedTexW / sourceTexW;
        }

        public static float GetImportedTextureScale(TextureImporter importer, Texture importedTexture)
        {
            importer.GetSourceTextureWidthAndHeight(out var sourceW, out _);
            return GetImportedTextureScale(sourceW, importedTexture.width);
        }
        
        public static Vector4 GetBorderOffset(float textureScale, int borderOffset)
        {
            borderOffset = Mathf.Max(0, borderOffset);
            borderOffset = (int)(borderOffset * textureScale);
            return Vector4.one * borderOffset;
        }

        /// <summary>
        /// Border offset needs to be adjusted for imported texture. Border offset is configured for source texture size,
        /// and imported texture size might be smaller. 
        /// </summary>
        public static Vector4 GetAdjustedBorderOffset(TextureImporter importer, Texture tex, int borderOffset)
        {
            return GetBorderOffset(GetImportedTextureScale(importer, tex), borderOffset);
        }

        /// <summary>
        /// To keep sdf effect size on texture consistent between different resolutions
        /// we need to modify <see cref="SDFImportSettings.GradientSize"/> using current resolution and reference resolution. 
        /// </summary>
        public static float GetAdjustedGradientSize(float gradientSize, int width, int height, Vector4 borderOffset)
        {
            var x = width + (int)borderOffset.x + (int)borderOffset.z;
            var y = height + (int)borderOffset.y + (int)borderOffset.w;
            var v = Mathf.Max(x, y);
            return gradientSize * ((float)REFERENCE_GRADIENT_IMAGE_SIZE / v);
        }

        public static float GetGradientSizeAdjustment(int width, int height)
        {
            var v = Mathf.Max(width, height);
            return v / (float)REFERENCE_GRADIENT_IMAGE_SIZE;
        }
        
        public static float GetAdjustedGradientSize(float gradientSize, int width, int height)
        {
            return GetAdjustedGradientSize(gradientSize, width, height, Vector4.zero);
        }

        #endregion


        #region Texture Operations

        
        /// <summary>
        /// Creates render texture from whole original texutre.
        /// Use this to extract sprite from texture and apply some border offset.
        /// Created texture configured to work with SDF.
        /// </summary>
        /// <param name="original">Whole texture</param>
        /// <param name="area">Which area of texture need to be copied, in pixels</param>
        /// <param name="offset">How much offset you want to add to final texture, in pixels</param>
        /// <returns>Render texture, you need to dispose it yourself</returns>
        public static RenderTexture CreateRT(Texture original, RectInt area, Vector4 offset)
        {
            var width = (int)(area.width + offset.x + offset.z);
            var height = (int)(area.height + offset.y + offset.w);
            var rtDesc = new RenderTextureDescriptor(width, height, GraphicsFormat.R8G8B8A8_UNorm, 0, 0);
            var rt = RenderTexture.GetTemporary(rtDesc);
            // For some reason rt might come dirty so bliting black tex
            Graphics.Blit(Texture2D.blackTexture, rt);
            Graphics.CopyTexture(original, 0, 0, area.x, area.y, area.width,
                area.height, rt, 0, 0, (int)offset.x, (int)offset.y);
            return rt;
        }
        

        /// <inheritdoc cref="CreateRT(UnityEngine.Texture,UnityEngine.RectInt,UnityEngine.Vector4)"/>
        public static RenderTexture CreateRT(Texture original)
        {
             return CreateRT(original, new RectInt(0, 0, original.width, original.height), Vector4.zero);
        }

        /// <summary>
        /// Will copy original RT to RT with new resolution, resampling it.
        /// Passed RT will be released by default.
        /// </summary>s
        public static RenderTexture ChangeResolution(RenderTexture rt, int width, int height, bool releaseRT = true)
        {
            var rtDesc = new RenderTextureDescriptor(width, height, GraphicsFormat.R8G8B8A8_UNorm, 0, 0);
            var scaledRT = RenderTexture.GetTemporary(rtDesc);
            Graphics.Blit(rt, scaledRT);
            if (releaseRT) RenderTexture.ReleaseTemporary(rt);
            return scaledRT;
        }

        /// <summary>
        /// Converts RenderTexture to Texture2D, basically copies pixles from GPU to CPU.
        /// Passed RT will be released by default.
        /// Created texture configured to work with SDF.
        /// </summary>
        public static Texture2D CreateTex2D(RenderTexture rt, bool releaseRT = true, TextureFormat format = TextureFormat.Alpha8)
        {
            var tex = new Texture2D(rt.width, rt.height, format, false);
            tex.wrapMode = rt.wrapMode;
            tex.filterMode = rt.filterMode;
            RenderTexture.active = rt;
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            tex.Apply();
            RenderTexture.active = null;
            if (releaseRT) RenderTexture.ReleaseTemporary(rt);
            return tex;
        }


        /// <summary>
        /// Combination of all functions above, will generate singular SDF Tex2D from area in source Tex2D.
        /// All inbetween steps done with Render Texture minimizing CPU involvement.
        /// </summary>
        /// <param name="original">Source texture, imported</param>
        /// <param name="area">Area on source texture, sprite for example</param>
        /// <param name="settings">Import settings to use in backend. Also used in applying 'Border Offset' and 'Resolution Scale'</param>
        /// <param name="backendContext">Backend used in generation</param>
        /// <returns></returns>
        public static Texture2D GenerateSDF(Texture original, RectInt area,
            SDFGenerationBackend.Settings settings, SDFGenerationBackend.Context backendContext)
        {
            var rt = CreateRT(original, area, settings.BorderOffset);
            backendContext.Backend.Generate(rt, settings);
            var scaledW = Mathf.Clamp((int)(rt.width * settings.ResolutionScale), MIN_TEX_SIZE, int.MaxValue);
            var scaledH = Mathf.Clamp((int)(rt.height * settings.ResolutionScale), MIN_TEX_SIZE, int.MaxValue);
            if (scaledW != rt.width || scaledH != rt.height)
            {
                rt = ChangeResolution(rt, scaledW, scaledH);
            }
            var tex = CreateTex2D(rt);
            return tex;
        }
        
        /// <inheritdoc cref="GenerateSDF(UnityEngine.Texture,UnityEngine.RectInt,nickeltin.SDF.Editor.SDFGenerationBackend.Settings,nickeltin.SDF.Editor.SDFGenerationBackend.Context)"/>
        public static Texture2D GenerateSDF(Texture2D original, 
            SDFGenerationBackend.Settings settings, SDFGenerationBackend.Context backend)
        {
            return GenerateSDF(original, 
                new RectInt(0,0, original.width, original.height),
                settings, backend);
        }

        public static Texture2D GenerateSDF(Texture2D[] texes, int maxTextureSize, out Rect[] rects,
            SDFGenerationBackend.Settings settings, SDFGenerationBackend.Context backend)
        {
            var atlas = PackTextures(texes, maxTextureSize, out rects);
            var packedSDF = GenerateSDF(atlas, settings, backend);
            Object.DestroyImmediate(atlas);
            return packedSDF;
        }


        public static Texture2D ExtractTexture(Texture2D original, RectInt area, Vector4 borderOffset)
        {
            var rt = CreateRT(original, area, borderOffset);
            return CreateTex2D(rt, true, TextureFormat.ARGB32);
        }


        public static Texture2D PackTextures(Texture2D[] texs, int maxTextureSize, out Rect[] rects,
            bool destoryTexes = true, TextureFormat format = TextureFormat.Alpha8)
        {
            var atlas = new Texture2D(MIN_TEX_SIZE, MIN_TEX_SIZE, format, false);
            rects = atlas.PackTextures(texs, 0, maxTextureSize);
            for (var i = 0; i < rects.Length; i++)
            {
                if (destoryTexes && texs[i] != null)
                {
                    Object.DestroyImmediate(texs[i]);
                }
            }

            return atlas;
        }
        
        

        /// <summary>
        /// Re-creates texture to be <see cref="TextureFormat.Alpha8"/> 
        /// </summary>
        public static Texture2D ConvertToAlpha8(Texture2D tex, bool destroyInTex = true)
        {
            var alpha8Texture = new Texture2D(tex.width, tex.height, TextureFormat.Alpha8, false)
            {
                filterMode = tex.filterMode,
                name = tex.name,
                wrapMode = tex.wrapMode,
            };
            
            var pixels = tex.GetPixels();
            var alphaValues = new byte[pixels.Length];

            // Extract the alpha channel values from the original pixels and store them in the alphaValues array
            for (var i = 0; i < pixels.Length; i++)
            {
                alphaValues[i] = (byte)(pixels[i].a * 255f); // Convert alpha (0-1) to byte (0-255)
            }
            
            alpha8Texture.LoadRawTextureData(alphaValues);
            alpha8Texture.Apply();

            if (destroyInTex)
            {
                Object.DestroyImmediate(tex);
            }

            return alpha8Texture;
        }

        #endregion

        
        /// <summary>
        /// Sets all pixels to <see cref="Color.clear"/>
        /// </summary>
        public static void Clear(this RenderTexture rt)
        {
            var prevRt = RenderTexture.active;
            RenderTexture.active = rt;
            GL.Clear(true, true, Color.clear);
            RenderTexture.active = prevRt;
        }
        
        /// <summary>
        /// Divides sprite with border for enumeration of 9 elements for each sprite region.
        /// </summary>
        /// <param name="sprite"></param>
        /// <returns></returns>
        public static IEnumerable<RectInt> EnumerateSpriteRects(Sprite sprite)
        {
            var r = sprite.rect;
            var b = sprite.border;

            var y1 = (int)(r.max.y - b.w);
            var h1 = (int)b.w;
            var y2 = (int)(r.min.y + b.y);
            var h2 = (int)(r.height - b.w - b.y);
            var y3 = (int)r.min.y;
            var h3 = (int)b.y;

            var x1 = (int)r.min.x;
            var w1 = (int)b.x;
            var x2 = (int)(r.min.x + b.x);
            var w2 = (int)(r.width - b.x - b.z);
            var x3 = (int)(r.max.x - b.z);
            var w3 = (int)b.z;

            yield return new RectInt(x1, y1, w1, h1);
            yield return new RectInt(x2, y1, w2, h1);
            yield return new RectInt(x3, y1, w3, h1);

            yield return new RectInt(x1, y2, w1, h2);
            yield return new RectInt(x2, y2, w2, h2);
            yield return new RectInt(x3, y2, w3, h2);

            yield return new RectInt(x1, y3, w1, h3);
            yield return new RectInt(x2, y3, w2, h3);
            yield return new RectInt(x3, y3, w3, h3);
        }
    }
}