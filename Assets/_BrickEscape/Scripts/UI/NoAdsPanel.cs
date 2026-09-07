using NabaGame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BrickEscape
{
    public class NoAdsPanel : BaseUI
    {
        public IAPItem[] iAPItems;
        public List<AnimButton> AnimNoAds;
        public void SetInfor() 
        {
            foreach (var item in iAPItems) 
            {
                item.SetInfor();
            }
        }
        public void CheckNoAds()
        {
            if (GameManager.Instance.PlayerProfile.isNoAds == 1)
            {
                foreach (var item in AnimNoAds)
                {
                    item.autoPlayOnAwake = false;
                    item.SetColor(ButtonColor.Gray_Up, ButtonColor.Gray_Down, FontColor.Gray);
                    item.ResetAnim();

                }
            }

        }
    }
}
