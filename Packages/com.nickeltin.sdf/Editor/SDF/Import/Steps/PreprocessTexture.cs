using UnityEditor.AssetImporters;

namespace nickeltin.SDF.Editor
{
    /// <summary>
    /// First step of import.
    /// We define is texture has valid type and is its <see cref="SDFImportSettings"/> allows to import sdf.
    /// </summary>
    internal class PreprocessTexture : ImportStep
    {
        public override ProcessResult Process(SDFImportContext sdfCtx, AssetImportContext astCtx)
        {
            var shouldImportSDF = SDFEditorUtil.ShouldImportSDF(sdfCtx.TextureImporter, out var importSettings);
            sdfCtx.ImportSettings = importSettings;

            return shouldImportSDF 
                ? ProcessResult.Continue() 
                : ProcessResult.End(false);
        }
    }
}