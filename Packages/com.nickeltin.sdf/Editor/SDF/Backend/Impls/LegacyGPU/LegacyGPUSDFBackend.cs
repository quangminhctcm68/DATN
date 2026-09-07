using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace nickeltin.SDF.Editor
{
    public partial class SDFImportSettings
    {
        public const float MIN_GRAD_SIZE = 0;
        public const float MAX_GRAD_SIZE = 1;
        
        [Tooltip("How big is SDF effect generated on texture. Consistent over texture sizes.")]
        [SerializeField, Range(MIN_GRAD_SIZE, MAX_GRAD_SIZE)] 
        public float GradientSize = 0.2f;
    }

    /// <summary>
    /// Main sdf generation backend.
    /// For provided Render Texture will generate sdf textures for each channel (rgba) with one material
    /// and then combine them all channels to single texture with second material.
    /// In order for backend to work <see cref="StaticCache.GenerationShader"/> shader should be in project
    /// with corresponding GUIDs defined in <see cref="GetArtifactDependencies"/>
    /// </summary>
    internal class LegacyGPUSDFBackend : SDFGenerationBackend
    {
        private static class StaticCache
        {
            private static Shader _generationShader;
            private static Shader _combineShader;
        
            public static Shader GenerationShader
            {
                get
                {
                    if (_generationShader == null)
                    {
                        // For some reason shader with Shader.Find might return null, so manually loading shader as asset.
                        // Note: not sure that this will always work due to shader might not be compiled at that point...
                        _generationShader = AssetDatabase.LoadAssetAtPath<Shader>(
                                AssetDatabase.GUIDToAssetPath(GENERATION_SHADER_GUID));
                    }
                    

                    return _generationShader;
                }
            }


            public static Material CreateMaterial(Shader shader)
            {
                return new Material(shader);
            }
        }


        public const string GENERATION_SHADER_GUID = "662d200f77a9a0b40af3f32a96f5bc3e";
        
        public override BackendBaseData BaseData { get; } = new BackendBaseData(
            "legacy",
            "Uses Jump Flood Algorithm to generate SDF on GPU",
            nameof(SDFImportSettings.GradientSize)
        );

        public override string GetDisplayName()
        {
            return "Legacy (GPU)";
        }
        

        private static readonly int CHANNEL_ID = Shader.PropertyToID("_Channel");
        private static readonly int FEATHER_ID = Shader.PropertyToID("_Feather");
        private static readonly int SPREAD_ID = Shader.PropertyToID("_Spread");
        private static readonly int SOURCE_TEX_ID = Shader.PropertyToID("_SourceTex");

        public override IEnumerable<string> GetArtifactDependencies()
        {
            // This is SDFGenerator shader GUID, it needed to be added as dependency because texture can be imported before shader.
            // If shader is an dependency it will trigger reimport on texture when shader is imported. 
            yield return GENERATION_SHADER_GUID;
        }
        
        public override void Generate(RenderTexture rt, Settings settings)
        {
            GenerateSingleChannel(rt, settings.GradientSize);   
        }
        
        /// <inheritdoc cref="Generate(UnityEngine.RenderTexture,nickeltin.SDF.Editor.SDFGenerationBackend.Settings)"/>
        /// <remarks>
        ///     Uses only alpha channel
        /// </remarks>
        public static void GenerateSingleChannel(RenderTexture rt, float gradientSize)
        {
            var genMat = StaticCache.CreateMaterial(StaticCache.GenerationShader);
            genMat.SetFloat(CHANNEL_ID, 3);
            GenerateChannel_New(rt, genMat, gradientSize, true);
            Object.DestroyImmediate(genMat);
        }
        
        public static void GenerateChannel_New(RenderTexture texture, Material material, float gradientSize, bool sRGB)
        {
            material.SetFloat(FEATHER_ID, gradientSize);
            // Allocate some temporary buffers
            var stepFormat = new RenderTextureDescriptor(texture.width, texture.height, 
                GraphicsFormat.R16G16B16A16_UNorm, 0, 0);
            stepFormat.sRGB = false;

            var target1 = RenderTexture.GetTemporary(stepFormat);
            var target2 = RenderTexture.GetTemporary(stepFormat);
            
            
            target1.filterMode = FilterMode.Point;
            target2.filterMode = FilterMode.Point;
            target1.wrapMode = TextureWrapMode.Clamp;
            target2.wrapMode = TextureWrapMode.Clamp;

            const int firstPass = 0;
            var finalPass = material.FindPass("FinalPass");

            // Detect edges of image
            material.EnableKeyword("FIRSTPASS");
            material.SetFloat(SPREAD_ID, 1);
            Graphics.Blit(texture, target1, material, firstPass);
            material.DisableKeyword("FIRSTPASS");
            
            Swap(ref target1, ref target2);
            
            // Gather nearest edges with varying spread values
            for (var i = 11; i >= 0; i--)
            {
                material.SetFloat(SPREAD_ID, Mathf.Pow(2, i));
                Graphics.Blit(target2, target1, material, firstPass);
                Swap(ref target1, ref target2);
            }

            var resultFormat = new RenderTextureDescriptor(texture.width, texture.height, 
                GraphicsFormat.R8G8B8A8_UNorm, 0, 0);
            resultFormat.sRGB = sRGB;


            var resultTarget = RenderTexture.GetTemporary(resultFormat);
            resultTarget.wrapMode = TextureWrapMode.Clamp;

            // Compute the final distance from nearest edge value
            material.SetTexture(SOURCE_TEX_ID, texture);
            Graphics.Blit(target2, resultTarget, material, finalPass);
            
            Graphics.CopyTexture(resultTarget, texture);
            
            // Clean up
            RenderTexture.ReleaseTemporary(resultTarget);
            RenderTexture.ReleaseTemporary(target2);
            RenderTexture.ReleaseTemporary(target1);
        }
        
        /// <summary>
        /// Generates SDF for one channel.
        /// Channel is specified in material.
        /// </summary>
        /// <param name="texture">Will take input from it and will output to it</param>
        /// <param name="material">Specify channel here</param>
        /// <param name="gradientSize">How big SDF should be</param>
        /// <param name="sRGB"></param>
        public static void GenerateChannel(RenderTexture texture, Material material, float gradientSize, bool sRGB)
        {
            material.SetFloat(FEATHER_ID, gradientSize);
            // Allocate some temporary buffers
            var stepFormat = new RenderTextureDescriptor(texture.width, texture.height, 
                GraphicsFormat.R16G16B16A16_UNorm, 0, 0);
            stepFormat.sRGB = false;

            var target1 = RenderTexture.GetTemporary(stepFormat);
            var target2 = RenderTexture.GetTemporary(stepFormat);
            target1.filterMode = FilterMode.Point;
            target2.filterMode = FilterMode.Point;
            target1.wrapMode = TextureWrapMode.Clamp;
            target2.wrapMode = TextureWrapMode.Clamp;

            const int firstPass = 0;
            var finalPass = material.FindPass("FinalPass");

            // Detect edges of image
            material.EnableKeyword("FIRSTPASS");
            material.SetFloat(SPREAD_ID, 1);
            Graphics.Blit(texture, target1, material, firstPass);
            material.DisableKeyword("FIRSTPASS");
            Swap(ref target1, ref target2);

            // Gather nearest edges with varying spread values
            for (var i = 11; i >= 0; i--)
            {
                material.SetFloat(SPREAD_ID, Mathf.Pow(2, i));
                Graphics.Blit(target2, target1, material, firstPass);
                Swap(ref target1, ref target2);
            }

            var resultFormat = new RenderTextureDescriptor(texture.width, texture.height, 
                GraphicsFormat.R8G8B8A8_UNorm, 0, 0);
            resultFormat.sRGB = sRGB;


            var resultTarget = RenderTexture.GetTemporary(resultFormat);
            resultTarget.wrapMode = TextureWrapMode.Clamp;

            // Compute the final distance from nearest edge value
            material.SetTexture(SOURCE_TEX_ID, texture);
            Graphics.Blit(target2, resultTarget, material, finalPass);

            Graphics.CopyTexture(resultTarget, texture);
            
            // Clean up
            RenderTexture.ReleaseTemporary(resultTarget);
            RenderTexture.ReleaseTemporary(target2);
            RenderTexture.ReleaseTemporary(target1);
        }

        private static void Swap<T>(ref T v1, ref T v2)
        {
            (v1, v2) = (v2, v1);
        }
        
        /// <summary>
        /// Call this method to generate SDF texture for input tex, it will be filled with new pixels.
        /// This call should be executed after assets from <see cref="GetArtifactDependecies"/> is imported,
        /// see more in <see cref="SDFImportSDFImporterUnityInterfacependencies"/>
        /// </summary>
        /// <param name="rt">In/Out texture</param>
        /// <param name="gradientSize">How big SDF is</param>
        // public static void Generate(RenderTexture rt, float gradientSize)
        // {
        //     var genMat = StaticCache.CreateMaterial(StaticCache.GenerationShader);
        //     var combineMat = StaticCache.CreateMaterial(StaticCache.CombineShader);
        //
        //
        //     var resultRtDesc = new RenderTextureDescriptor(rt.width, rt.height, GraphicsFormat.R8G8B8A8_UNorm, 0, 0);
        //     var allChannelsBuffer1 = RenderTexture.GetTemporary(resultRtDesc);
        //     var allChannelsBuffer2 = RenderTexture.GetTemporary(resultRtDesc);
        //     
        //     
        //     void RenderChannel(int channelIndex)
        //     {
        //         genMat.SetFloat(CHANNEL_ID, channelIndex);
        //         var channelRT = RenderTexture.GetTemporary(resultRtDesc);
        //         Graphics.CopyTexture(rt, channelRT);
        //         GenerateChannel(channelRT, genMat, gradientSize, true);
        //         
        //         combineMat.SetFloat(CHANNEL_ID, channelIndex);
        //         
        //         Graphics.CopyTexture(allChannelsBuffer1, allChannelsBuffer2);
        //         
        //         combineMat.SetTexture(SOURCE_TEX_ID, allChannelsBuffer2);
        //         
        //         Graphics.Blit(channelRT, allChannelsBuffer1, combineMat);
        //         
        //         RenderTexture.ReleaseTemporary(channelRT);
        //     }
        //     
        //     var mode = TextureModes.RGBA;
        //     for (var c = 3; c >= 0; c--)
        //     {
        //         if (((int)mode & (1 << c)) == 0) continue;
        //         RenderChannel(c);
        //     }
        //     
        //     
        //     Graphics.CopyTexture(allChannelsBuffer1, rt);
        //     
        //     RenderTexture.ReleaseTemporary(allChannelsBuffer1);
        //     RenderTexture.ReleaseTemporary(allChannelsBuffer2);
        //     
        //     Object.DestroyImmediate(genMat);
        //     Object.DestroyImmediate(combineMat);
        // }
    }
}