using System;
using NabaGame.Core.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace NabaGame.Tracking.Editor
{
    [InitializeOnLoad]
    public static class TrackerModuleMenu
    {
      //  [MenuItem("BMH Game/Reload Tracker Module", false, 10)]
        private static void CheckTrackerModules()
        {
            foreach (TrackerType module in Enum.GetValues(typeof(TrackerType)))
            {
//                string trackerName = GetTrackerClassName(module);
//                Type trackerType = Type.GetType($"{trackerName}");
//                bool isHasSdk = trackerType != null;
////                BB_FileUtils.FileExists(GetTrackerClassPath(module)) ||
////                                BB_EditorUtils.NamespaceExists(GetTrackerNamespace(module));
//                if (isHasSdk)
//                {
//                    Debug.Log($"{trackerType.Namespace} : {trackerType.Name}");
//
//                    NB_GlobalDefineUtils.AddDefine(GetTrackerDefineSymbol(module));
//                }
//                else
//                {
//                    Debug.Log($"{trackerName} : not found");
//                    NB_GlobalDefineUtils.RemoveDefine(GetTrackerDefineSymbol(module));
//                }
                NB_GlobalDefineUtils.AddDefine(GetTrackerDefineSymbol(module));

            }
        }

        private static string GetTrackerDefineSymbol(TrackerType module)
        {
            string symbol = string.Empty;
            switch (module)
            {
                case TrackerType.Firebase:
                    symbol = TrackingEditorParameters.SYMBOL_FIREBASE_ANALYTIC;
                    break;
//                case TrackerType.Facebook:
//                    symbol = TrackingEditorParameters.SYMBOL_FACEBOOK_ANALYTIC;
//                    break;
//                case TrackerType.GameAnalytic:
//                    symbol = TrackingEditorParameters.SYMBOL_GAMEANALYTIC;
//                    break;
                case TrackerType.ByteBrew:
                    symbol = TrackingEditorParameters.SYMBOL_BYTEBREW_ANALYTIC;
                    break;
            }

            return symbol;
        }

        private static string GetTrackerClassName(TrackerType module)
        {
            string className = string.Empty;
            switch (module)
            {
                case TrackerType.Firebase:
                    className = TrackingEditorParameters.CLASS_FIREBASE_ANALYTIC;
                    break;
//                case TrackerType.Facebook:
//                    className = TrackingEditorParameters.CLASS_FACEBOOK_ANALYTIC;
//                    break;
//                case TrackerType.GameAnalytic:
//                    className = TrackingEditorParameters.CLASS_GAMEANALYTIC;
//                    break;
                case TrackerType.ByteBrew:
                    className = TrackingEditorParameters.CLASS_BYTEBREW_ANALYTIC;
                    break;
            }

            return className;
        }

        private static string GetTrackerNamespace(TrackerType module)
        {
            string className = string.Empty;
            switch (module)
            {
                case TrackerType.Firebase:
                    className = TrackingEditorParameters.FIREBASE_ANALYTIC_NAMESPACE;
                    break;
//                case TrackerType.Facebook:
//                    className = TrackingEditorParameters.FACEBOOK_ANALYTIC_NAMESPACE;
//                    break;
//                case TrackerType.GameAnalytic:
//                    className = TrackingEditorParameters.GAMEANALYTIC_NAMESPACE;
//                    break;
            }

            return className;
        }
    }
}