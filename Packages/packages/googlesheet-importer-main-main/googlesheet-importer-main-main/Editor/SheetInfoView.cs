using System;
using System.Collections.Generic;
using NabaGame.Core.Runtime.Extensions;

using UnityEditor;
using UnityEngine;
using Object = System.Object;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
using Sirenix.Serialization;
#endif
namespace NabaGame.Googlesheet.Importer.Editor
{
#if ODIN_INSPECTOR
    [Serializable, ShowOdinSerializedPropertiesInInspector]
    public class SheetInfoView
    {
        [FoldoutGroup("Sheet Info", true), OnValueChanged("OnNameChanged")]
        public string sheetName;

        [FoldoutGroup("Sheet Info", true), OnValueChanged("OnIdChanged")]
        public string SpreadsheetID;

        [FoldoutGroup("Sheet Info", true),
         Sirenix.OdinInspector.FolderPath(ParentFolder = "Assets", RequireExistingPath = true),
         OnValueChanged("OnScriptFolderChanged")]
        public string ScriptFolder;

        [FoldoutGroup("Sheet Info", true),
         Sirenix.OdinInspector.FolderPath(ParentFolder = "Assets", RequireExistingPath = true),
         OnValueChanged("OnAssetFolderChanged")]
        public string AssetFolder;

        [FoldoutGroup("Sheet Info", true),
         Sirenix.OdinInspector.FolderPath(ParentFolder = "Assets", RequireExistingPath = true),
         OnValueChanged("OnSpriteAssetFolderChanged")]
        public string SpriteAssetFolder;

        [FoldoutGroup("Sheet Info", true),
         Sirenix.OdinInspector.FolderPath(ParentFolder = "Assets", RequireExistingPath = true),
         OnValueChanged("OnSkeletonDataFolderChanged")]
        public string SkeletonDataFolder;

        [FoldoutGroup("Sheet Info", true),
         Sirenix.OdinInspector.FolderPath(ParentFolder = "Assets", RequireExistingPath = true),
         OnValueChanged("OnPrefabFolderChanged")]
        public string PrefabFolder;

        [FoldoutGroup("Sheet Info", true), OnValueChanged("OndefaultSpriteChanged")]
        public Sprite defaultSprite;

        private Dictionary<string, IList<IList<Object>>> sheetData
        {
            get => info.sheetData;
            set
            {
                info.sheetData = value;
                EditorUtility.SetDirty(info);
            }
        }

        private List<string> sheetNames
        {
            get => info.sheetNames;
            set
            {
                info.sheetNames = value;
                EditorUtility.SetDirty(info);
            }
        }

        [FoldoutGroup("Sheet Data", true), ValueDropdown("sheetNames"), OnValueChanged("OnSheetSelected"), OdinSerialize]
        public string selectTab
        {
            get => info.selectTab;
            set
            {
                info.selectTab = value;
                EditorUtility.SetDirty(info);
            }
        }

        [FoldoutGroup("Sheet Data", true), TableMatrix, ShowInInspector]
        public string[,] cells
        {
            get => info.cells;
            set
            {
                info.cells = value;

                EditorUtility.SetDirty(info);
                AssetDatabase.SaveAssets();
            }
        }

        public List<string> rawFields
        {
            get => info.rawFields;
            set
            {
                info.rawFields = value;
                EditorUtility.SetDirty(info);
            }
        }

        private GoogleSheetController googleSheetController;
        private string infoBoxMessage;
        private SheetInfo info;

        public SheetInfoView(SheetInfo sheetInfo)
        {
            // sheetInfo.LoadName();
            info = sheetInfo;
            sheetName = sheetInfo.sheetName;
            SpreadsheetID = sheetInfo.SpreadsheetID;
            ScriptFolder = sheetInfo.ScriptFolder;
            AssetFolder = sheetInfo.AssetFolder;
            SpriteAssetFolder = sheetInfo.SpriteAssetFolder;
            SkeletonDataFolder = sheetInfo.SkeletonDataFolder;
            PrefabFolder = sheetInfo.PrefabFolder;
            defaultSprite = sheetInfo.defaultSprite;
        }

        public SheetInfoView(SheetInfoView sheetInfo)
        {
            info = sheetInfo.info;
            sheetName = sheetInfo.sheetName;
            SpreadsheetID = sheetInfo.SpreadsheetID;
            ScriptFolder = sheetInfo.ScriptFolder;
            AssetFolder = sheetInfo.AssetFolder;
            SpriteAssetFolder = sheetInfo.SpriteAssetFolder;
            SkeletonDataFolder = sheetInfo.SkeletonDataFolder;
            PrefabFolder = sheetInfo.PrefabFolder;
            defaultSprite = sheetInfo.defaultSprite;
            sheetData = sheetInfo.sheetData;
            sheetNames = sheetInfo.sheetNames;
            selectTab = sheetInfo.selectTab;
        }

        private void OnNameChanged()
        {
            info.sheetName = sheetName;
            info.name = sheetName;
            EditorUtility.SetDirty(info);
            // AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(info.GetInstanceID()), $"{sheetName}.asset");
        }

        private void OnIdChanged()
        {
            info.SpreadsheetID = SpreadsheetID;
            EditorUtility.SetDirty(info);
        }

        private void OnScriptFolderChanged()
        {
            info.ScriptFolder = ScriptFolder;
            EditorUtility.SetDirty(info);
        }

        private void OnAssetFolderChanged()
        {
            info.AssetFolder = AssetFolder;
            EditorUtility.SetDirty(info);
        }

        private void OnSpriteAssetFolderChanged()
        {
            info.SpriteAssetFolder = SpriteAssetFolder;
            EditorUtility.SetDirty(info);
        }

        private void OnSkeletonDataFolderChanged()
        {
            info.SkeletonDataFolder = SkeletonDataFolder;
            EditorUtility.SetDirty(info);
        }

        private void OndefaultSpriteChanged()
        {
            info.defaultSprite = defaultSprite;
            EditorUtility.SetDirty(info);
        }

        private void OnPrefabFolderChanged()
        {
            info.PrefabFolder = PrefabFolder;
            EditorUtility.SetDirty(info);
        }

        [ButtonGroup("Sheet Info/Script", 1), Button(ButtonSizes.Large)]
        public void GenerateScript()
        {
            if (ScriptFolder.IsNullOrWhitespace())
            {
                infoBoxMessage = "Script Folder path is Null";
                return;
            }

            if (selectTab.IsNullOrWhitespace())
            {
                infoBoxMessage = "No sheet is selected";
                return;
            }

            string folderPath = $"{Application.dataPath}/{ScriptFolder}";
            ScriptGenerator.GenerateClass(cells[0, 0], rawFields, folderPath);
        }

        [ButtonGroup("Sheet Info/Script", 1), Button(ButtonSizes.Large)]
        public void GenerateAssets()
        {
            if (AssetFolder.IsNullOrWhitespace())
            {
                infoBoxMessage = "Asset Folder path is Null";
            }

            DataAssetGenerator.GenerateClass(selectTab, cells, rawFields,AssetFolder);
        }

        [FoldoutGroup("Sheet Info", true, 0), Button(ButtonSizes.Large), GUIColor(0.91f, 0.98f, 0.50f), EnableIf("@SpreadsheetID != string.Empty"),
         InfoBox("$infoBoxMessage", InfoMessageType.Warning, "@!string.IsNullOrEmpty(infoBoxMessage)")]
        public void LoadSheet()
        {
            string credentialFilePath = System.IO.Path.GetFullPath("Packages/com.nabagame.googlesheet.importer/Editor/bb-googlesheet-data-collector.json");// $"{Application.dataPath}/{SpreadSheetLoaderConfig.Instance.CredentialFilePath}";
            if (credentialFilePath.IsNullOrWhitespace())
            {
                EditorUtility.DisplayDialog("Load Sheet Data", $"credential file path is invalid !! check the data config", "close");
                return;
            }

            try
            {
                if (googleSheetController == null)
                {
                    googleSheetController = new GoogleSheetController(SpreadsheetID, credentialFilePath);
                }
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("Load Sheet Data", $"{e.Message}", "close");
                return;
            }

            sheetNames = googleSheetController.GetAllSheetName();
            sheetData = googleSheetController.GetAllSheetValueRange(sheetNames);
            if (!selectTab.IsNullOrWhitespace() && sheetNames.Contains(selectTab))
            {
                OnSheetSelected();
            }
            else
            {
                selectTab = String.Empty;
                cells = null;
            }
        }


        private void OnSheetSelected()
        {
            if (sheetData != null && sheetData.Count > 0 &&
                sheetData.TryGetValue(selectTab, out IList<IList<Object>> data))
            {
                infoBoxMessage = string.Empty;
                info.selectTab = selectTab;
                if (!data.IsNullOrEmpty() && data.Count >= 2)
                {
                    int rowCount = data.Count;
                    int colCount = data[1].Count;
                    rawFields = new List<string>(colCount);
                    string[,] newCells = new string[colCount, rowCount];
                    for (int i = 0; i < rowCount; i++)
                    {
                        if (!data[i].IsNullOrEmpty())
                        {
                            for (int j = 0; j < data[i].Count; j++)
                            {
                                if (j < colCount)
                                {
                                    string value = (string)data[i][j];
                                    if (i == 1)
                                    {
                                        rawFields.Add(value);
                                    }

                                    newCells[j, i] = value;
                                }
                            }
                        }
                    }

                    cells = newCells;
                }
                EditorUtility.SetDirty(info);
            }
            else
            {
                Debug.LogError($"data {sheetName} is null or does not contain sheet {selectTab}");
            }
        }
    }
#endif
}