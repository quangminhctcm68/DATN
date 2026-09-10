using System;
using System.Collections.Generic;
using NabaGame.Core.Runtime.Extensions;
using UnityEditor;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
using Sirenix.Serialization;
#endif
namespace NabaGame.Googlesheet.Importer.Editor
{
#if ODIN_INSPECTOR
    public class SheetInfo : SerializedScriptableObject
    {
        public string sheetName;
        public string SpreadsheetID;

        [Sirenix.OdinInspector.FolderPath(ParentFolder = "Assets", RequireExistingPath = true)]
        public string ScriptFolder;

        [Sirenix.OdinInspector.FolderPath(ParentFolder = "Assets", RequireExistingPath = true)]
        public string AssetFolder;

        [Sirenix.OdinInspector.FolderPath(ParentFolder = "Assets", RequireExistingPath = true)]
        public string SpriteAssetFolder;

        [Sirenix.OdinInspector.FolderPath(ParentFolder = "Assets", RequireExistingPath = true)]
        public string SkeletonDataFolder;

        [Sirenix.OdinInspector.FolderPath(ParentFolder = "Assets", RequireExistingPath = true)]
        public string PrefabFolder;

        public Sprite defaultSprite;
        [OdinSerialize,HideInInspector]
        public string[,] cells;
        [HideInInspector]
        public Dictionary<string, IList<IList<object>>> sheetData;
        [HideInInspector]
        public List<string> sheetNames;
        [HideInInspector]
        public string selectTab;
        [HideInInspector]
        public List<string> rawFields;

        public void LoadName()
        {
            if (sheetName.IsNullOrWhitespace())
            {
                SpreadSheetLoaderConfig.Instance.sheetIndex++;
                PlayerPrefs.SetInt("BBsheetId", SpreadSheetLoaderConfig.Instance.sheetIndex);
                sheetName = $"New Sheet {SpreadSheetLoaderConfig.Instance.sheetIndex}";
            }
        }
        [Button]
        public void Rename()
        {
            AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(this.GetInstanceID()), $"{sheetName}.asset");
        }

        public void Delete()
        {
        }
    }
#endif
}