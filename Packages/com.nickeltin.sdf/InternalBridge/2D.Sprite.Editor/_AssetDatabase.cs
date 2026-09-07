using UnityEditor;

namespace nickeltin.SDF.InternalBridge.Editor
{
    internal static class _AssetDatabase
    {
        public static int GetMainAssetInstanceID(string assetPath)
        {
            return AssetDatabase.GetMainAssetInstanceID(assetPath);
        }
    }
}