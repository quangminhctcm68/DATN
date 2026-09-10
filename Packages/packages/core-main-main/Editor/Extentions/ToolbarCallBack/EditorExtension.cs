using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityToolbarExtender;


public class OpenSceneButton1 
{
    [MenuItem("BMH Game/Open Scenes", false,2000)]
    public static void OpenScene()
    {
        SceneEditor.OpenWindown();
    }
}

[InitializeOnLoad]
public class SceneSwitchLeftButton
{
    static SceneSwitchLeftButton()
    {
        // EditorApplication.update += Update;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        ToolbarExtender.LeftToolbarGUI.Add(OnToolbarGUI);
    }

    static void OnPlayModeStateChanged(PlayModeStateChange obj)
    {
        if (obj != PlayModeStateChange.EnteredEditMode) return;

        var editorParams = EditorParameters.Instance;
        if (editorParams == null) return;

        if (string.IsNullOrEmpty(editorParams.curEditScene)) return;

        var activeScenePath = SceneManager.GetActiveScene().path;
        if (editorParams.curEditScene != activeScenePath)
        {
            EditorSceneManager.OpenScene(editorParams.curEditScene);

            editorParams.curEditScene = string.Empty;
            EditorUtility.SetDirty(editorParams);
        }
    }

    static void OnToolbarGUI()
    {
        GUILayout.FlexibleSpace();
        if (GUILayout.Button(new GUIContent("open scenes", "Start From LoadScene")))
        {
            SceneEditor.OpenWindown();
        }
    }
}