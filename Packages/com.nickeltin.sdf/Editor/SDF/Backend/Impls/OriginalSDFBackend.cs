using UnityEngine;

namespace nickeltin.SDF.Editor
{
    /// <summary>
    /// Returns original texture copy instead of any custom SDF processing.
    /// </summary>
    internal class OriginalSDFBackend : SDFGenerationBackend
    {
        public override BackendBaseData BaseData { get; } = new BackendBaseData(
            "original", 
            "Returns orginal texture copy");
        

        public override void Generate(RenderTexture rt, Settings settings)
        {
        }
    }
}