using System.Collections.Generic;
using System.IO;
using NabaGame.Core.Runtime.Extensions;
using UnityEditor;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
#endif
namespace NabaGame.Googlesheet.Importer.Editor
{
    public delegate void UpdateSheetListAction();
    
#if ODIN_INSPECTOR
    public class SpreadSheetLoaderConfig : ScriptableObject
    {
        private static string defaultAssetFolder = "BBPackages/BBGoogleSheet/Editor";
        private static string defaultSheetFolder = "BBPackages/BBGoogleSheet/Editor/SheetInfos";
        [FoldoutGroup("Sheet List", true,0),PropertyOrder(1),InlineEditor,OnCollectionChanged("Before", "After")]
        public List<SheetInfo> sheetList;

        [HideInInspector] public int sheetIndex;
        public UpdateSheetListAction sheetUpdateCallback;

        public static SpreadSheetLoaderConfig Instance
        {
            get => GetInstance();
        }

        private static SpreadSheetLoaderConfig GetInstance()
        {
            SpreadSheetLoaderConfig value =
                AssetDatabase.LoadAssetAtPath<SpreadSheetLoaderConfig>(
                    $"Assets/{defaultAssetFolder}/SpreadSheetLoaderConfig.asset");
            if (value == null)
            {
                value = CreateInstance<SpreadSheetLoaderConfig>();
                if (!Directory.Exists($"{Application.dataPath}/{defaultAssetFolder}"))
                {
                    Directory.CreateDirectory($"{Application.dataPath}/{defaultAssetFolder}");
                }

                AssetDatabase.CreateAsset(value, $"Assets/{defaultAssetFolder}/SpreadSheetLoaderConfig.asset");
            }

            return value;
        }

        public void Before(CollectionChangeInfo info, object value)
        {
        }

        public void After(CollectionChangeInfo info, object value)
        {
            sheetUpdateCallback?.Invoke();
        }

        [FoldoutGroup("Sheet List", true, 0),Button(ButtonSizes.Large),PropertyOrder(0),GUIColor(0, 1, 0)]
        public void AddNewSheet()
        {
            SheetInfo newSheet = CreateInstance<SheetInfo>();
            newSheet.LoadName();

            if (!Directory.Exists($"{Application.dataPath}/{defaultSheetFolder}"))
            {
                Directory.CreateDirectory($"{Application.dataPath}/{defaultSheetFolder}");
            }

            AssetDatabase.CreateAsset(newSheet, $"Assets/{defaultSheetFolder}/{newSheet.sheetName}.asset");
            sheetList.Add(newSheet);
        }

        [BoxGroup("Delete Sheet",order:2),ShowInInspector, ValueDropdown("sheetList")] private SheetInfo wantToDeleteSheet;
        [BoxGroup("Delete Sheet", order:2), Button(ButtonSizes.Large), EnableIf("@wantToDeleteSheet != null"),GUIColor(1, 0.6f, 0.4f)]
        private void DeleteSheet()
        {
            if (EditorUtility.DisplayDialog("Delete Confirmation",
                    $"Do you want to delete sheet {wantToDeleteSheet.name}?", "Yes", "No"))
            {
                sheetList.Remove(wantToDeleteSheet);
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(wantToDeleteSheet.GetInstanceID()));
            }
        }
    }
#endif
}