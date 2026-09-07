#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class KeyStoreTool
{
    static KeyStoreTool()
    {
        PlayerSettings.keystorePass = "12345679";
        PlayerSettings.keyaliasPass = "12345679";
    }
}
#endif
