using UnityEditor;
using UnityEngine;

namespace BMH.Ads.Editor
{
    public class AdsEditorMenu
    {
        [MenuItem("GameObject/BMH Game/AdManager", false, 0)]
        public static void AddAdManager()
        {
            Object adManagerPrefab = AssetDatabase.LoadAssetAtPath<Object>("Packages/com.bmh.ads/Runtime/Prefabs/AdManager.prefab");
            if (adManagerPrefab != null)
            {
                PrefabUtility.InstantiatePrefab(adManagerPrefab);
            }
            else
            {
                Debug.LogError("Cannot find AdManager prefab");
            }
        }
    }
}