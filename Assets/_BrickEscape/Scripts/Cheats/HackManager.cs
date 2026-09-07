using System.Collections;
using System.Collections.Generic;
using BrickEscape;
using NabaGame.Core.Runtime.Singleton;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Solitaire
{
    public class HackManager : Singleton<HackManager>
    {
        public override void Init()
        {
      
        }
        
        [Button]
        public void AddMoney(int value)
        {
            GameManager.Instance.PlayerProfile.ChangeCoin(value);
        }
        
        // [Button]
        // public void ChangeInterCooldownStatus(bool status)
        // {
        //     if (status) UIMainManager.Instance.interAdsPanel.RestartInterCooldown();
        //     else UIMainManager.Instance.interAdsPanel.StopInterCooldown();
        // }
    }
}
