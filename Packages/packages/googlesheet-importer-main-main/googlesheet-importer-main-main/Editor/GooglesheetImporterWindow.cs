using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
#endif
namespace NabaGame.Googlesheet.Importer.Editor
{
#if ODIN_INSPECTOR
    public class GooglesheetImporterWindow : OdinMenuEditorWindow
    {
        private static SpreadSheetLoaderConfig config;

        [MenuItem("BMH Game/Googlesheet Importer #&g")]
        private static void OpenWindow()
        {
            var window = GetWindow<GooglesheetImporterWindow>();
            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(1000, 600);
        }

        protected override OdinMenuTree BuildMenuTree()
        {
           var  tree = new OdinMenuTree(true);
            var customMenuStyle = new OdinMenuStyle
            {
                BorderPadding = 0f,
                AlignTriangleLeft = true,
                TriangleSize = 16f,
                TrianglePadding = 0f,
                Offset = 20f,
                Height = 23,
                IconPadding = 0f,
                BorderAlpha = 0.323f
            };
            tree.DefaultMenuStyle = customMenuStyle;
            tree.Config.DrawSearchToolbar = true;
            config = SpreadSheetLoaderConfig.Instance;
            tree.AddObjectAtPath("Data Config", new DataConfigView(this, config));
            if (!config.sheetList.IsNullOrEmpty())
            {

                for (int i = 0; i < config.sheetList.Count; i++)
                {
                    tree.AddMenuItemAtPath("Sheets",
                        new SheetInfoMenuItem(tree, new SheetInfoView(config.sheetList[i])));
                }
            }

            return tree;
        }
    }

    
    
    public class SheetInfoMenuItem : OdinMenuItem
    {
        public SheetInfoView instance;

        public SheetInfoMenuItem(OdinMenuTree tree, SheetInfoView instance) : base(tree, instance.sheetName, instance)
        {
            this.instance = instance;
        }

        protected override void OnDrawMenuItem(Rect rect, Rect labelRect)
        {
            labelRect.x -= 16;
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Space)
            {
                var selection = this.MenuTree.Selection
                    .Select(x => x.Value)
                    .OfType<SheetInfo>();

                if (selection.Any())
                {
                    Event.current.Use();
                }
            }
        }

        public override string SmartName
        {
            get { return instance.sheetName; }
        }
    }

    [Serializable]
    public class DataConfigView
    {
        private OdinMenuEditorWindow window;

        [InlineEditor(InlineEditorModes.FullEditor)]
        public SpreadSheetLoaderConfig config;

        public DataConfigView(OdinMenuEditorWindow _window, SpreadSheetLoaderConfig _config)
        {
            window = _window;
            config = _config;
            config.sheetUpdateCallback = OnUpdateSheetList;
        }

        private void OnUpdateSheetList()
        {
            window.ForceMenuTreeRebuild();
        }
    }
#else
public class nabagameEditorToolWindow {
    [MenuItem("BMH Game/Googlesheet Importer #&g")]
    static void OpenWarningPanel()
    {
        EditorUtility.DisplayDialog("Install Odin", "Please Install Odin Package to use Google sheet Importer", "OK");
    }
}
#endif
}

