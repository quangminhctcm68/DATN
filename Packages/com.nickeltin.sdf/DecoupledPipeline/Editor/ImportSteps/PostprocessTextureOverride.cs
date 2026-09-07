using UnityEngine;

namespace nickeltin.SDF.Editor.DecoupledPipeline
{
    internal class PostprocessTextureOverride : PostprocessTexture
    {
        protected override Texture CopyTex(Texture2D texture)
        {
            return CopyTex(texture, false);
        }
    }
}