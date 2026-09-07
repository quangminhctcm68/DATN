using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Security.Claims;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace BrickEscape
{
    public class GiftItem : MonoBehaviour
    {
        public RewardType rewardType;
        public Image Icon;
        public Image Gift;
        public GameObject GiftParent;
        public int day;
        public int Qty;
        public TextMeshProUGUI Day_txt;
        public TextMeshProUGUI Qty_txt;
        public GameObject Tick;
        private PlayerProfile profile;
        public AnimGift anim;
        public void SetInfor(RawDailyReward rawDailyReward) 
        {
            rewardType = rawDailyReward.RewardType;
            profile = GameManager.Instance.PlayerProfile;
            bool isUnlock = profile.rewardDailyIndex >=day;
            day = rawDailyReward.ID;
            Qty = rawDailyReward.Qty;
            Day_txt.text = $"Day {day}";

            Icon.gameObject.SetActive(isUnlock);
            GiftParent.gameObject.SetActive(!isUnlock);
            anim.Active = (profile.rewardDailyIndex+1) == day;
            if (!isUnlock)
            { 
                Qty_txt.text = string.Empty;
       
            }
            else
            {
                SetUp();
            }
            Tick.SetActive(isUnlock);
        }
        
        
        public void GiveReward() 
        {
            switch (rewardType)
            {
                case RewardType.Coin:
                    Icon.sprite = GameManager.Instance.spriteCollection.Icon[IconType.Coin];
                    profile.ChangeCoin(Qty);
                    Qty_txt.text = $"+{Qty}";
                    break;

                case RewardType.Hint_Booster:
                    Icon.sprite = GameManager.Instance.spriteCollection.BoostetDic[BoosterType.Hint];
                    profile.ChangeHintBoosterUseCount(Qty);
                    Qty_txt.text = $"+{Qty}";
                    break;

                case RewardType.MagicWand_Booster:
                    Icon.sprite = GameManager.Instance.spriteCollection.BoostetDic[BoosterType.Magic];
                    profile.ChangeMagicWandBoosterUseCount(Qty);
                    Qty_txt.text = $"+{Qty}";
                    break;
                case RewardType.Hammer_Booster:
                    Icon.sprite = GameManager.Instance.spriteCollection.BoostetDic[BoosterType.Hammer];
                    profile.ChangeHammerBoosterUseCount(Qty);
                    Qty_txt.text = $"+{Qty}";
                    break;
                case RewardType.Infinite_Heart:
                    Icon.sprite = GameManager.Instance.spriteCollection.Icon[IconType.Infinite_Heart];
                    GameController.Instance.heartManager.AddUnlimitedHeartTime(Qty*60);    
                    Qty_txt.text = $"{Qty}m";
                    break;

                case RewardType.Avatar_Icon:
                    Icon.sprite = GameManager.Instance.spriteCollection.avatarDic[(AvatarID)Qty];
                    profile.avatarProfile.UnlockAvatar((AvatarID)Qty);
                    Qty_txt.text = "x1";
                    break;

                case RewardType.Avatar_Frame:
                    Icon.sprite = GameManager.Instance.spriteCollection.frameDic[(FrameID)Qty];
                    profile.avatarProfile.UnlockFrame((FrameID)Qty);
                    Qty_txt.text = "x1";
                    break;
            }
            Icon.gameObject.SetActive(true);
            GiftParent.gameObject.SetActive(false);
            Tick.SetActive(true);
            UIMainManager.Instance.chestOpenPanel.SetGiftInfor(rewardType, Qty, () => 
            {
                DOVirtual.DelayedCall(0.3f, () => { UIMainManager.Instance.dailyRewardPanel.Show(); });
            });
        }
        
        
        
        
        public void SetUp()
        {
            switch (rewardType)
            {
                case RewardType.Coin:
                    Icon.sprite = GameManager.Instance.spriteCollection.Icon[IconType.Coin];
                    Qty_txt.text = $"+{Qty}";
                    break;

                case RewardType.Hint_Booster:
                    Icon.sprite = GameManager.Instance.spriteCollection.BoostetDic[BoosterType.Hint];
                    Qty_txt.text = $"+{Qty}";
                    break;

                case RewardType.MagicWand_Booster:
                    Icon.sprite = GameManager.Instance.spriteCollection.BoostetDic[BoosterType.Magic];
                    Qty_txt.text = $"+{Qty}";
                    break;
                case RewardType.Hammer_Booster:
                    Icon.sprite = GameManager.Instance.spriteCollection.BoostetDic[BoosterType.Hammer];
                    Qty_txt.text = $"+{Qty}";
                    break;
                case RewardType.Infinite_Heart:
                    Icon.sprite = GameManager.Instance.spriteCollection.Icon[IconType.Infinite_Heart];
                    Qty_txt.text = $"{Qty}m";
                    break;

                case RewardType.Avatar_Icon:
                    Icon.sprite = GameManager.Instance.spriteCollection.avatarDic[(AvatarID)Qty];
                    Qty_txt.text = "x1";
                    break;

                case RewardType.Avatar_Frame:
                    Icon.sprite = GameManager.Instance.spriteCollection.frameDic[(FrameID)Qty];
                    Qty_txt.text = "x1";
                    break;
            }
           
        }
        

    }
}
