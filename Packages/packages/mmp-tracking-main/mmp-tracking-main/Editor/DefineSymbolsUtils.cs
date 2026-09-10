#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using NabaGame.Core.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace BMH.MMP.Editor.Utils
{
    [InitializeOnLoad]
    public class DefineSymbolsUtils : MonoBehaviour
    {
        [MenuItem("BMH Game/Add Define Symbols/Add 'BMH_SINGULAR'", false, 1)]
        private static void BMH_SINGULAR()
        {
            NB_GlobalDefineUtils.AddDefine("BMH_SINGULAR");
        }
        [MenuItem("BMH Game/Add Define Symbols/Add 'BMH_SOLAR'", false, 1)]
        private static void BMH_SOLAR()
        {
            NB_GlobalDefineUtils.AddDefine("BMH_SOLAR");
        }
    }
}
#endif