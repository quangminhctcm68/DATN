#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BMH.IAP.Editor.Utils
{
    public class MMPEditorMenu
    {
        [MenuItem("GameObject/BMH Game/IAP Manager", false, 0)]
        public static void AddAdManager()
        {
            Object adManagerPrefab =
                AssetDatabase.LoadAssetAtPath<Object>("Packages/com.bmh.mmp/Runtime/Prefabs/IAPManager.prefab");
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
