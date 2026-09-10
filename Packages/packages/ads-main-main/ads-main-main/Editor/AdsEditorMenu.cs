using UnityEditor;
using UnityEngine;

namespace NabaGame.Ads.Editor
{
    public class AdsEditorMenu
    {
        [MenuItem("GameObject/Naba Game/AdManager", false, 0)]
        public static void AddAdManager()
        {
            Object adManagerPrefab = AssetDatabase.LoadAssetAtPath<Object>("Packages/com.nabagame.ads/Runtime/Prefabs/AdManager.prefab");
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