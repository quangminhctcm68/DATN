using System.Collections.Generic;
using System.Linq;
using nickeltin.InternalBridge.Editor;
using nickeltin.SDF.Runtime;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace nickeltin.SDF.Editor
{
    internal class SDFImageBuildPreprocessor : IPreprocessBuildWithReport
    {
        public int callbackOrder => -100;
        
        public void OnPreprocessBuild(BuildReport report)
        {
            var sdfUiDisplayShader = Shader.Find(SDFUtil.SDFDisplayUIShaderName);
            var pureSdfUiDisplayShader = Shader.Find(SDFUtil.SDFDisplayPureUIShaderName);
            ModifyIncludeShaders(new [] { sdfUiDisplayShader, pureSdfUiDisplayShader }, null);
        }

        private static IEnumerable<Object> EnumerateArrayObjects(SerializedProperty property)
        {
            for (var i = 0; i < property.arraySize; i++)
            {
                yield return property.GetArrayElementAtIndex(i).objectReferenceValue;
            }
        }
        
        private static void ModifyIncludeShaders(Shader[] add, Shader[] remove)
        {
            if (add == null && remove == null) return;

            var graphicsSettingsObj = AssetDatabase.LoadAssetAtPath<GraphicsSettings>("ProjectSettings/GraphicsSettings.asset");
            var serializedObject = new SerializedObject(graphicsSettingsObj);
            var arrayProp = serializedObject.FindProperty("m_AlwaysIncludedShaders");
            // ReSharper disable once CollectionNeverQueried.Local
            var shaders = EnumerateArrayObjects(arrayProp).Where(s => s != null).ToHashSet();
            var hasChange = false;
            
            if (add != null)
            {
                foreach (var shader in add)
                {
                    if (shaders.Add(shader)) hasChange = true;
                }
            }

            if (remove != null)
            {
                foreach (var shader in remove)
                {
                    if (shaders.Remove(shader)) hasChange = true;
                }
            }


            if (hasChange)
            {
                arrayProp.ClearArray();
                foreach (var shader in shaders) arrayProp.AppendFoldoutPPtrValue(shader);
                serializedObject.ApplyModifiedProperties();
                AssetDatabase.SaveAssets();
                serializedObject.Dispose();
            }
        }
    }
}