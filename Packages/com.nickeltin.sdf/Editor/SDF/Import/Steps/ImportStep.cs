using UnityEditor.AssetImporters;
using UnityEngine;

namespace nickeltin.SDF.Editor
{
    internal abstract class ImportStep
    {
        public readonly struct ProcessResult
        {
            public readonly bool Successful;
            
            // Set true to end import early
            public readonly bool ImportEnded;

            public ProcessResult(bool successful, bool importEnded)
            {
                Successful = successful;
                ImportEnded = importEnded;
            }

            public static ProcessResult End(bool successful)
            {
                return new ProcessResult(successful, true);
            }
            
            public static ProcessResult Continue()
            {
                return new ProcessResult(true, false);
            }
        }
        
        public abstract ProcessResult Process(SDFImportContext sdfCtx, AssetImportContext astCtx);
        
        public static SDFGenerationBackend.Settings GetSettingsForBackend(SDFImportSettings settings, Vector4 borderOffset, 
            int width, int height, float textureScale)
        {
            var gradientSize = SDFGraphicsUtil.GetAdjustedGradientSize(settings.GradientSize, 
                width, height, borderOffset) * textureScale;

            var settings2 = new SDFGenerationBackend.Settings(settings)
            {
                GradientSize = gradientSize,
                BorderOffset = borderOffset,
            };

            return settings2;
        }

        public override string ToString()
        {
            return GetType().Name + "(SDFImportStep)";
        }
    }
}