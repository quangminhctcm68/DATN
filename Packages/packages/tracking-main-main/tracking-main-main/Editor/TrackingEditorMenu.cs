using UnityEditor;
using UnityEngine;

namespace NabaGame.Tracking.Editor
{
    public class TrackingEditorMenu
    {
        [MenuItem("GameObject/BMH Game/TrackingManager", false, 0)]
        public static void AddAdManager()
        {
            Object prefab = AssetDatabase.LoadAssetAtPath<Object>("Packages/com.nabagame.tracking/Runtime/Prefabs/TrackingManager.prefab");
            if (prefab != null)
            {
                PrefabUtility.InstantiatePrefab(prefab);
            }
            else
            {
                Debug.LogError("Cannot find TrackingManager prefab");
            }
        }
    }
}