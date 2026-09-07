#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using nickeltin.SDF.Samples.Runtime;
using UnityEditor;
using UnityEngine;


namespace nickeltin.SDF.Samples.Editor
{
    public abstract class SampleBaseEditor : UnityEditor.Editor
    {
        private static class Defaults
        {
            public static readonly GUIContent Error = new(EditorGUIUtility.IconContent("console.erroricon"));
            public static readonly GUIContent TempContent = new();
        }
        
        private readonly struct ButtonEntry
        {
            public readonly SampleButtonAttribute Attribute;
            public readonly MethodInfo Method;
            public readonly string DisplayName;
            public readonly bool IsValid;
            public readonly string InvalidMessage;

            private ButtonEntry(SampleButtonAttribute attribute, MethodInfo method, string invalidMessage)
            {
                Attribute = attribute;
                Method = method;
                InvalidMessage = invalidMessage;
                IsValid = string.IsNullOrEmpty(invalidMessage);
                DisplayName = string.IsNullOrEmpty(Attribute.Name)
                    ? ObjectNames.NicifyVariableName(method.Name)
                    : attribute.Name;
            }

            public readonly void Invoke(object[] targets)
            {
                foreach (var target in targets)
                {
                    Method.Invoke(target, null);
                }
            }
            
            public static bool TryCreate(MethodInfo info, out ButtonEntry buttonEntry)
            {
                if (!info.IsAbstract)
                {
                    var attr = info.GetCustomAttribute<SampleButtonAttribute>();
                    if (attr != null)
                    {
                        var invalidMessage = new List<string>();
                        if (info.GetParameters().Length > 0)
                        {
                            invalidMessage.Add($"Method can't have parameters");
                        }

                        if (info.ReturnType != typeof(void))
                        {
                            invalidMessage.Add($"Method should have void return");
                        }
                        buttonEntry = new ButtonEntry(attr, info, string.Join("\n", invalidMessage));
                        return true;
                    }
                }

                buttonEntry = new ButtonEntry();
                return false;
            }

            public static int FillAllEntries(Type fromType, List<ButtonEntry> entries)
            {
                var i = 0;
                foreach (var method in fromType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    if (TryCreate(method, out var entry))
                    {
                        entries.Add(entry);
                        i++;
                    }
                }

                return i;
            }
        }
        private List<ButtonEntry> _buttons;
        
        private void OnEnable()
        {
            _buttons = new List<ButtonEntry>();
            ButtonEntry.FillAllEntries(target.GetType(), _buttons);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            foreach (var button in _buttons)
            {
                using (new EditorGUI.DisabledScope(!button.IsValid))
                {
                    var content = Defaults.TempContent;
                    content.image = button.IsValid ? null : Defaults.Error.image;
                    content.text = button.DisplayName;
                    content.tooltip = button.IsValid ? "" : button.InvalidMessage;
                    if (GUILayout.Button(content))
                    {
                        button.Invoke(targets);
                    }
                }
            }
        }
    }
}


#endif