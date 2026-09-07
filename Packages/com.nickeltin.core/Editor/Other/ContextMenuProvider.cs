using nickeltin.Core.Runtime;
using nickeltin.InternalBridge.Editor;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace nickeltin.Core.Editor
{
    internal static class ContextMenuProvider
    {
        [MenuItem(MenuPathsUtility.UtilsToolbarMenu + "Open DataPath")]
        private static void PersistentDataPath_Context()
        {
            EditorUtility.RevealInFinder(Application.persistentDataPath);
        }

        private const string RECOMPILE = MenuPathsUtility.UtilsToolbarMenu + "Recompile";


        [MenuItem(RECOMPILE)]
        private static void Recompile_Context()
        {
            EditorApplication.UnlockReloadAssemblies();
            CompilationPipeline.RequestScriptCompilation();
        }

        [MenuItem(MenuPathsUtility.UtilsToolbarMenu + "Build And Run (Ignore Exceptions)")]
        private static void BuildAndRun_Context()
        {
            BuildPlayerWindow.ShowBuildPlayerWindow();
            _BuildPlayerWindow.CallBuildMethods(true, BuildOptions.AutoRunPlayer);
        }
    }
}