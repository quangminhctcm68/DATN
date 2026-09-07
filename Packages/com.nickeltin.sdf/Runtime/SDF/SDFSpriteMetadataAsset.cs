using nickeltin.SDF.Runtime.DecoupledPipeline;
using UnityEngine;
using UnityEngine.Internal;

namespace nickeltin.SDF.Runtime
{
    /// <summary>
    /// Generated for each sprite at import time, used for referencing sprite, sdf sprite, and import metadata.
    /// This assets is hidden in editor to prevent texture breaking (becoming main asset of texture)
    /// </summary>
    [ExcludeFromPreset, ExcludeFromDocs]
    public class SDFSpriteMetadataAsset : ScriptableObject
    {
        [SerializeField] internal SDFSpriteMetadata _metadata = default;
        [SerializeField] internal bool _isImportedDecoupled = false;

        public SDFSpriteMetadata Metadata => _metadata;

        /// <summary>
        /// True is this meta asset is imported from decoupled pipeline as part of <see cref="SDFAsset"/>
        /// </summary>
        public bool IsImportedDecoupled => _isImportedDecoupled;

        public static implicit operator SDFSpriteMetadata(SDFSpriteMetadataAsset asset)
        {
            return asset.Metadata;
        }
    }
}