using UnityEditor;
using UnityEngine;

namespace NabaGame.Ads.Editor
{
    public class ExtentMenu
    {
        [MenuItem("GameObject/BMH Game/NotificationManager", false, 0)]
        public static void AddNotification()
        {
            Object notifi = AssetDatabase.LoadAssetAtPath<Object>("Packages/com.naba.extend/Runtime/Prefabs/NotificationManager.prefab");
            if (notifi != null)
            {
                PrefabUtility.InstantiatePrefab(notifi);
            }
            else
            {
                Debug.LogError("Cannot find AdManager prefab");
            }
        }
        [MenuItem("GameObject/BMH Game/AppViewManager", false, 0)]
        public static void AddAppView()
        {
            Object appview = AssetDatabase.LoadAssetAtPath<Object>("Packages/com.naba.extend/Runtime/Prefabs/AppReviewManager.prefab");
            if (appview != null)
            {
                PrefabUtility.InstantiatePrefab(appview);
            }
            else
            {
                Debug.LogError("Cannot find AdManager prefab");
            }
        }
    }
}