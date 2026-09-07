  using ArrowMaze;
using BMH.Ads;
using NabaGame.Core.Runtime.EventManager;
using NabaGame.Core.Runtime.Singleton;
using Singular;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace BrickEscape
{
    public class GameManager : SerializedSingleton<GameManager>
    {
        public LevelData levelData;
        public RawShopData iapData;
        public DataCollection dataCollection;
        public Pooling pooling;
        public SpriteCollection spriteCollection;
        public ProfileData profileData;
        public PlayerProfile PlayerProfile;
        public RawFrameData rawFrameData;
        public RawAvatarData rawAvatarData;
        public BattlepassData battlepassData;
        public CheatData cheatData;
        public CookedDailyReward cookedDailyReward;

        public RawDailyQuestData rawDailyQuestData;
        public RawDailyQuestRewardData rawDailyQuestRewardData;

        public RawWeeklyQuestData rawWeeklyQuestData;
        public RawWeeklyRewardData rawWeeklyRewardData;

        public RawMonthlyQuestData rawMonthlyQuestData; 
        public RawMonthlyRewardData rawMonthlyRewardData;
        public override void Init()
        {
            Application.targetFrameRate = 60;
            PlayerProfile = new PlayerProfile();
        }
        private void Start()
        {
            LoadIAP();
            cheatData.SetFreeIAP(false);
            DOTween.SetTweensCapacity(800, 150);
        }
        public void SetNoAds() 
        {
            PlayerProfile.isNoAds = 1;
            PlayerProfile.SaveData();
            UIMainManager.Instance.homePanel.CheckNoAds();
            UIMainManager.Instance.gamePlayPanel.CheckNoAds();
        }
        public int amount;
        [Button]
        public void AddDailyBadge()
        {
            PlayerProfile.QuestProfile.AddDailyBadgeCount(amount);
        }
        [Button]
        public void AddWeeklyBadge()
        {
            PlayerProfile.QuestProfile.AddWeeklyBadgeCount(amount);
        }
        [Button]
        public void AddMonthlyBadge()
        {
            PlayerProfile.QuestProfile.AddMonthlyBadgeCount(amount);
        }


        [Button]
        public void AddClearBlock()
        {
            PlayerProfile.QuestProfile.AddDailyClearBlockCount(500);
        }
        [Button]
        public void AddUseBooster()
        {
            PlayerProfile.QuestProfile.AddDailyUseBoosterCount(3);
        }
        [Button]
        public void AddCollectCoin()
        {
            PlayerProfile.QuestProfile.AddDailyCollectCointCount(100);
        }

        [Button]
        public void AddInifityHeart()
        {
            GameController.Instance.heartManager.AddUnlimitedHeartTime(300);
        }

        //[Button]
        //public void ResetHeart()
        //{
        //    GameController.Instance.heartManager.SetUnl(300);
        //}

        public int GetToday()
        {
#if UNITY_EDITOR
            // Test: 30s = 1 day
            return (int)(Time.realtimeSinceStartup / 20f);
#else
    // Production
    return (int)(DateTime.UtcNow - DateTime.UnixEpoch).TotalDays;
#endif
        }

        #region Application Pause/ Quit

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                PlayerProfile.SaveData();
            }
            else
            {
                if (GameController.Instance != null)
                {
                    GameController.Instance.heartManager.CheckTime();
                    //GameController.Instance.heartManager.SaveUnlimitedHeartTime();
                }
            }
        }

        private void OnApplicationQuit()
        {
            if (GameController.Instance != null)
            {
                GameController.Instance.heartManager.SaveUnlimitedHeartTime();
            }
            PlayerProfile.SaveData();
           
        }

        #endregion

        public void LoadIAP()
        {
            List<IAPInfo> iAPInfos = new List<IAPInfo>();
            foreach (var rawShop in iapData.rawShops)
            {
                iAPInfos.Add(new IAPInfo()
                {
                    productId = rawShop.PackageID,
                    amout = rawShop.Amout
                });
                IAPManager.Instance.InitializePurchasing(iAPInfos);
            }

        }

    }
}
