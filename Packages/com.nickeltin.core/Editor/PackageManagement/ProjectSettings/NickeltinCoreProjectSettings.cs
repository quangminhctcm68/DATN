using System;
using System.Collections.Generic;
using System.Linq;
using nickeltin.SOCreateWindow.Editor;
using nickeltin.SOCreateWindow.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace nickeltin.Core.Editor
{
    [FilePath("ProjectSettings/NickeltinCoreSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    internal class NickeltinCoreProjectSettings : ScriptableSingleton<NickeltinCoreProjectSettings>
    {
        [SerializeField, Tooltip("If package is installed from registry or git repo it can check for updates automatically")]
        internal bool _checkForUpdates = true;
        [SerializeField, Tooltip("List of version that will be ignored when checking for updates")] 
        internal string[] _ignoredVersions = Array.Empty<string>();
        [SerializeField, Tooltip("Context click in project view can open up Window where scriptable objects or other assets can be defined. " +
                                 "Use " + nameof(CreateAssetWindowAttribute) + " or " + nameof(CustomCreateAssetWindowAttribute))]
        internal bool _showCreateWindow = false;

        public static bool CheckForUpdates
        {
            get => instance._checkForUpdates;
            set => SetProperty(ref instance._checkForUpdates, value);
        }

        public static string[] IgnoredVersions
        {
            get => instance._ignoredVersions;
            set => SetProperty(ref instance._ignoredVersions, value);
        }
        
        public static bool ShowCreateWindow
        {
            get => instance._showCreateWindow;
            set => SetProperty(ref instance._showCreateWindow, value);
        }

        public static HashSet<Version> IgnoredVersionsSet
        {
            get
            {
                return instance._ignoredVersions.Select(str =>
                    {
                        Version.TryParse(str, out var ver);
                        return ver;
                    })
                    .Where(ver => ver != default)
                    .ToHashSet();
            }
        }


        private void OnEnable()
        {
            // Removing flag to make object editable in editor
            this.hideFlags &= ~HideFlags.NotEditable;
        }

        private void OnDisable() => Save();

        private static bool SetProperty<T>(ref T prop, T newValue)
        {
            if (EqualityComparer<T>.Default.Equals(prop, newValue)) return false;

            prop = newValue;
            instance.Save();
            return true;
        }
        
        public void Save()
        {
            Save(true);
        }

        protected override void Save(bool saveAsText)
        {
            ScriptableObjectCreator.VerifyCreateMenu();
            base.Save(saveAsText);
        }

        internal SerializedObject GetSerializedObject() => new(this);
    }

    internal class NickeltinCoreSettingsProvider : SettingsProvider
    {
        private readonly SerializedObject _serializedObject;
        private SerializedProperty _checkForUpdates;
        private SerializedProperty _ignoredVersions;
        private SerializedProperty _showCreateWindow;


        public NickeltinCoreSettingsProvider(string path, SettingsScope scopes, SerializedObject serializedObject)
            : base(path, scopes, GetSearchKeywordsFromSerializedObject(serializedObject))
        {
            _serializedObject = serializedObject;
        }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            NickeltinCoreProjectSettings.instance.Save();
            
            _checkForUpdates = _serializedObject.FindProperty(nameof(NickeltinCoreProjectSettings._checkForUpdates));
            _ignoredVersions = _serializedObject.FindProperty(nameof(NickeltinCoreProjectSettings._ignoredVersions));
            _showCreateWindow = _serializedObject.FindProperty(nameof(NickeltinCoreProjectSettings._showCreateWindow));
        }

        public override void OnGUI(string searchContext)
        {
            _serializedObject.Update();
            
            EditorGUILayout.PropertyField(_checkForUpdates);
            EditorGUILayout.PropertyField(_ignoredVersions);
            EditorGUILayout.PropertyField(_showCreateWindow);
            
            if (_serializedObject.ApplyModifiedProperties())
            {
                NickeltinCoreProjectSettings.instance.Save();
            }
        }

      
        
        [SettingsProvider]
        public static SettingsProvider CreateTimelineProjectSettingProvider()
        {
            var provider = new NickeltinCoreSettingsProvider("Project/Nickeltin/Core", SettingsScope.Project, 
                NickeltinCoreProjectSettings.instance.GetSerializedObject());
            return provider;
        }
    }

}