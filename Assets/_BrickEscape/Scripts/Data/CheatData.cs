using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BrickEscape
{
    
    [CreateAssetMenu(fileName = "Cheat Data", menuName = "GameData/CheatData")]
    public class CheatData : ScriptableObject
    {
        [SerializeField] private bool freeIAP = false;
        
        #region Edit Data
        
        public void SetFreeIAP(bool value)
        {
            freeIAP = value;
        }
        
        #endregion
        
        #region Get Data
        
        public bool AllowFreeIAP => freeIAP;
        
        #endregion
    }
}
