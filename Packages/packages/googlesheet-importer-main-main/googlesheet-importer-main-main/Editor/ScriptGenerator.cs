using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using NabaGame.Core.Runtime.Extensions;
using UnityEditor;
using UnityEngine;

namespace NabaGame.Googlesheet.Importer.Editor
{
    public static class ScriptGenerator
    {
        private static string scriptTemplate = @"using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

[System.Serializable]
public class {0} 
{
{1}}

public class {0}Data : ScriptableObject 
{
    [TableList]
    public List<{0}> {2} = new List<{0}>();
}";
        private static string fieldTemplate = "\tpublic {0} {1};";

        public static void GenerateClass(string className, List<string> rawFields, string scriptPath)
        {
            Type elementType = Assembly.GetExecutingAssembly().GetType(className);
            if (elementType != null)
            {
                EditorUtility.DisplayDialog("Generate Script", $"{elementType} is already existed", "close");
                return;
            }

            StringBuilder fieldBuilder = new StringBuilder();
            for (int i = 0; i < rawFields.Count; i++)
            {
                string rawField = rawFields[i];
                if (rawField.IsNullOrWhitespace())
                {
                    EditorUtility.DisplayDialog("Generate Script", $"Column {i + 1} is empty field", "close");
                    break;
                }

                if (!rawField.Contains("_"))
                {
                    EditorUtility.DisplayDialog("Generate Script", $"Column {i + 1} : {rawField} is invalid", "close");
                    continue;
                }

                string filedType = rawField.Substring(0, rawField.IndexOf('_'));
                string fieldName = rawField.Substring(rawField.IndexOf('_') + 1);
                bool isEnum = false;
                if (fieldName.Contains(":") && filedType.Trim().ToLower() == "s")
                {
                    var enumParts = fieldName.Split(':');
                    if (enumParts.Length > 1)
                    {
                        string enumType = enumParts[1];
                        fieldName = enumParts[0];
                        if (EnumUtils.GetEnumTypeByName(enumType) == null)
                        {
                            EditorUtility.DisplayDialog("Generate Script", $"Column {i + 1} : enum {enumType} not found", "close");
                        }
                        else
                        {
                            isEnum = true;
                            filedType = enumType;
                        }
                    }
                }

                if (!isEnum)
                {
                    filedType = GetTypeName(filedType.Trim());
                }

                fieldBuilder.AppendLine(string.Format(fieldTemplate, filedType, fieldName));
            }
            string listField = $"{className[0].ToString().ToLower()}{className.Substring(1)}s";
            string classData = scriptTemplate.Replace("{0}", className)
                                            .Replace("{1}", fieldBuilder.ToString())
                                            .Replace("{2}", listField);
            string filePath = scriptPath + $"/{className}Data.cs";
            StreamWriter writer = File.CreateText(filePath);
            writer.WriteLine(classData);
            writer.Close();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Success !! {className} scripts is generated !!");
        }

        private static string GetTypeName(string _data)
        {
            string typeName = "";
            List<string> annotationList = new List<string>(_data.Split('.'));
            if (annotationList.Count == 1)
            {
                typeName = GetPrimitiveTypeName(annotationList[0]);
            }
            else
            {
                typeName = GetComplicatedTypeName(annotationList);
            }

            return typeName;
        }

        private static string GetPrimitiveTypeName(string _data)
        {
            string typeName = string.Empty;
            if (_data == "n")
            {
                typeName = "int";
            }
            else if (_data == "l")
            {
                typeName = "long";
            }
            else if (_data == "b")
            {
                typeName = "bool";
            }
            else if (_data == "s")
            {
                typeName = "string";
            }
            else if (_data == "f")
            {
                typeName = "float";
            }
            else if (_data == "d")
            {
                typeName = "double";
            }
            else if (_data == "sp")
            {
                typeName = "Sprite";
            }
            else if (_data == "spine")
            {
                typeName = "SkeletonAnimation";
            }
            else if (_data == "skeDat")
            {
                typeName = "SkeletonDataAsset";
            }
            else if (_data == "pref")
            {
                typeName = "GameObject";
            }
            else if (_data.StartsWith("p"))
            {
                int first = _data.IndexOf('<');
                int last = _data.IndexOf('>');
                string insideStr = _data.Substring(first + 1, last - first - 1);
                string[] elementList = insideStr.Split(',');
                string keyType = GetPrimitiveTypeName(elementList[0]).Capitalize();
                string valueType = GetPrimitiveTypeName(elementList[1]).Capitalize();
                typeName = "Pair" + keyType + valueType;
            }

            return typeName;
        }

        private static string GetComplicatedTypeName(List<string> annotationList)
        {
            string typeName = string.Empty;
            if (annotationList == null || annotationList.Count == 0)
            {
                return string.Empty;
            }

            if (annotationList.Count == 1)
            {
                return GetPrimitiveTypeName(annotationList[0]);
            }

            List<string> subAnnotationList = new List<string>(annotationList);
            subAnnotationList.RemoveAt(0);

            if (annotationList[0] == "li")
            {
                string elementType = GetComplicatedTypeName(subAnnotationList);
                typeName = "List<" + elementType + ">";
            }
            else if (annotationList[0].StartsWith("p"))
            {
                int first = annotationList[1].IndexOf('<');
                int last = annotationList[1].IndexOf('>');
                string insideStr = annotationList[1].Substring(first + 1, last - first - 1);
                string[] elementList = insideStr.Split(',');
                string keyType = GetPrimitiveTypeName(elementList[0]).Capitalize();
                string valueType = GetPrimitiveTypeName(elementList[1]).Capitalize();
                typeName = "Pair" + keyType + valueType;
            }

            return typeName;
        }
    }
}