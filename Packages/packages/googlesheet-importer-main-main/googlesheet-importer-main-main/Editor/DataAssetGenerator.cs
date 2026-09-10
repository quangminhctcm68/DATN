using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
// using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;
using NabaGame.Core.Runtime;
using NabaGame.Core.Runtime.Extensions;

namespace NabaGame.Googlesheet.Importer.Editor
{
    public static class DataAssetGenerator
    {
        public static void GenerateClass(string sheetName, string[,] cells, List<string> rawFields,
            string assetFolderPath)
        {
            sheetName = sheetName.Replace(" ", "");
            string typeName = cells[0, 0];
            string fileName = "Raw" + sheetName;
            string dataClassName =  typeName + "Data";
            Type assetType = GetTypeByName(dataClassName);
            string path = $"Assets/{assetFolderPath}";

            if (assetType == null)
            {
                EditorUtility.DisplayDialog("Error",
                    "Cannot find the script " + dataClassName + ", please generate the script first.", "close");
                return;
            }

            var dataHolder = AssetDatabase.LoadAssetAtPath(path + $"/{fileName}.asset", assetType);
            if (dataHolder == null)
            {
                dataHolder = ScriptableObject.CreateInstance(assetType);
                AssetDatabase.CreateAsset(dataHolder, path + $"/{fileName}.asset");
            }
            string listFieldName = $"{typeName[0].ToString().ToLower()}{typeName.Substring(1)}s";
            FieldInfo dataListField = assetType.GetField(listFieldName);
            var dataList = dataListField.GetValue(dataHolder);
            dataList.GetType().GetMethod("Clear").Invoke(dataList, null);

            Type dataType = GetTypeByName(typeName);
            FieldInfo[] fields = dataType.GetFields();
            if (fields == null || fields.Length <= 0)
            {
                EditorUtility.DisplayDialog("Generate Asset", $"Failed ! {typeName} have no field", "close");
                return;
            }

            if (fields.Length > rawFields.Count)
            {
                EditorUtility.DisplayDialog("Generate Asset",
                    $"Failed ! scripts {typeName} fields count does not match with data", "close");
                return;
            }

            int totalRow = cells.GetLength(1);

            if (totalRow < 3)
            {
                EditorUtility.DisplayDialog("Generate Asset", $"Failed ! data not found", "close");
                return;
            }

            string[] defaultVals = new string[fields.Length];
            for (int row = 2; row < cells.GetLength(1); row++)
            {
                var data = Activator.CreateInstance(dataType);
                for (int col = 0; col < fields.Length; col++)
                {
                    if (col > fields.Length)
                    {
                        continue;
                    }

                    string cell = cells[col, row];
                    if (cell.IsNullOrWhitespace())
                    {
                        if (defaultVals[col].IsNullOrWhitespace())
                        {
                            EditorUtility.DisplayDialog("Generate Asset", $"Failed ! cell[{row},{col}] cannot be null",
                                "close");
                            return;
                        }
                        else
                        {
                            cell = defaultVals[col];
                        }
                    }
                    else
                    {
                        defaultVals[col] = cell;
                    }

                    FieldInfo fieldInfo = fields[col];
                    object value = GetFieldValue(fieldInfo, cell);
                    fieldInfo.SetValue(data, value);
                }

                dataList.GetType().GetMethod("Add").Invoke(dataList, new object[] { data });
            }

            EditorUtility.SetDirty(dataHolder);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Success !! {typeName} data is generated !!");
        }

        private static object GetFieldValue(FieldInfo info, string _value)
        {
            object value = null;
            if (info.FieldType == typeof(int))
            {
                if (int.TryParse(_value, out int rs))
                {
                    value = rs;
                }
            }
            else if (info.FieldType == typeof(long))
            {
                if (long.TryParse(_value, out long rs))
                {
                    value = rs;
                }
            }
            else if (info.FieldType == typeof(double))
            {
                if (double.TryParse(_value, out double rs))
                {
                    value = rs;
                }
            }
            else if (info.FieldType == typeof(float))
            {
                if (float.TryParse(_value, NumberStyles.Float, CultureInfo.InvariantCulture,out float rs))
                {
                    value = rs;
                }
            }
            else if (info.FieldType == typeof(bool))
            {
                if (Boolean.TryParse(_value.ToUpper(), out bool rs))
                {
                    value = rs;
                }
            }
            else if (info.FieldType == typeof(string))
            {
                value = _value;
            }
            else if (info.FieldType == typeof(List<int>))
            {
                //value = _value;
                string[] elementList = _value.Split(',');
                value = elementList.Select(s => int.Parse(s)).ToList();
            }
            else if (info.FieldType == typeof(Sprite))
            {
                // if (name == "Costume"||name == "Costume2")
                // {
                //     value = (Sprite) AssetDatabase.LoadAssetAtPath(
                //         "Assets/_RedBlueImposter/Sprites/UI/Skin/" + _value + ".png", typeof(Sprite));
                // }
                // else if (name == "Weapon"||name == "Weapon2")
                // {
                //     value = (Sprite) AssetDatabase.LoadAssetAtPath(
                //         "Assets/_RedBlueImposter/Sprites/UI/Weapon/" + _value + ".png", typeof(Sprite));
                // }
                // else if (name == "RoomBase1")
                // {
                //     value = (Sprite) AssetDatabase.LoadAssetAtPath(
                //         "Assets/_RedBlueImposter/Sprites/UI/Base/Icon_2D/" + _value + ".png", typeof(Sprite));
                // }
            }
            else if (info.FieldType.IsEnum)
            {
                try
                {
                    value = Enum.Parse(info.FieldType, _value);
                }
                catch (Exception e)
                {
                    value = null;
                    Debug.LogError($"Convert Fail: {_value} to Enum Type {info.FieldType}");
                }
            }

            return value;
        }

        private static bool IsEnum(Type checkType)
        {
            return checkType.BaseType.Name.Equals("Enum");
        }

        public static Type GetTypeByName(string name)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.Name == name)
                        return type;
                }
            }

            return null;
        }
    }
}