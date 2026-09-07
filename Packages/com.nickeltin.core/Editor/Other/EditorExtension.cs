using System;
using System.Linq;
using System.Reflection;
using nickeltin.InternalBridge.Editor;
using UnityEditor;
using UnityEngine;

namespace nickeltin.Core.Editor
{
    public static class EditorExtension
    {
        private delegate void PlaceUIElementRootDelegate(GameObject element, MenuCommand command);

        private static readonly PlaceUIElementRootDelegate rPlaceUIElementRoot;

        static EditorExtension()
        {
            var menuOptions = Type.GetType("UnityEditor.UI.MenuOptions, UnityEditor.UI");
            var methodInfo = menuOptions?.GetMethod("PlaceUIElementRoot", BindingFlags.Static | BindingFlags.NonPublic);
            if (methodInfo != null)
            {
                rPlaceUIElementRoot = (PlaceUIElementRootDelegate)Delegate.CreateDelegate(typeof(PlaceUIElementRootDelegate), methodInfo);
            }
            else
            { 
                throw new Exception($"Unity removed internal method: UnityEditor.UI.MenuOptions.PlaceUIElementRoot(GameObject, MenuCommand)");
            }
        }

        /// <summary>
        /// Places newly created UI element inside of any existent Canvas, or creates new one.
        /// Uses internal unity methods used in UI creation context menus.
        /// </summary>
        public static void PlaceUIElementRoot(GameObject element, MenuCommand command)
        {
            rPlaceUIElementRoot(element, command);
        }

        /// <summary>
        /// Validates is <see cref="EditorGUIUtility.systemCopyBuffer"/> has serialized with JSON object,
        /// and targeted type has any of serialized fields name.
        /// To fill this buffer you can use <see cref="EditorJsonUtility.ToJson(object)"/>
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        [Obsolete]
        public static bool ValidatePasteBuffer(Type type)
        {
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            return fields.Any(field => EditorGUIUtility.systemCopyBuffer.Contains(field.Name));
        }

        [Obsolete]
        public static void SearchInProjectWindow(string searchText)
        {
            _ProjectBrowser.SearchInProjectWindow(searchText);
        }
    }
}