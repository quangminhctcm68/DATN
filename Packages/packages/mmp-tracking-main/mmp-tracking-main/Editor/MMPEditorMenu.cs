#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BMH.MMP.Editor.Utils
{
    public class MMPEditorMenu
    {
        [MenuItem("GameObject/BMH Game/MMP Manager", false, 0)]
        public static void AddAdManager()
        {
            Object adManagerPrefab =
                AssetDatabase.LoadAssetAtPath<Object>("Packages/com.bmh.mmp/Runtime/Prefabs/MMPTrackingManager.prefab");
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
#endif
