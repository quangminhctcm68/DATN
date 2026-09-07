using System.Collections;
using System.Collections.Generic;
using NabaGame.Tracking;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BrickEscape
{
    public class BattlepassManager : MonoBehaviour
    {
        [SerializeField] private List<BattlepassRewardData_Regular> regularBattlepassRewardDatas =
            new List<BattlepassRewardData_Regular>();

        [SerializeField] private List<BattlepassRewardData_Premium> premiumBattlepassRewardDatas =
            new List<BattlepassRewardData_Premium>();

        [SerializeField] private int currentBattlepassLevel = -1;
        [SerializeField] private int currentBattlepassEXP = 0;
        [SerializeField] private int nextLevelEXPRequirement = 0;
        [SerializeField] private int maxLevel = 0;
        [SerializeField] private List<int> battlepassLevelMilestones = new List<int>();
        [SerializeField] private int levelUpCount = 0;
        [SerializeField] private bool expChanged = false;
        private BattlepassProfile battlepassProfile;
        private List<BattlePassMilestoneData> battlepassMilestoneDatas = new List<BattlePassMilestoneData>();

        private List<BattlePassPremiumMilestoneData> battlepassPremiumMilestoneDatas =
            new List<BattlePassPremiumMilestoneData>();

        private int newLevel;
        public int LevelUnlock;

        #region Start, Update, Validate

        public void Init()
        {
            regularBattlepassRewardDatas = GameManager.Instance.battlepassData.GetAllRegularRewards;
            premiumBattlepassRewardDatas = GameManager.Instance.battlepassData.GetAllPremiumRewards;
            battlepassProfile = GameManager.Instance.PlayerProfile.BattlepassProfile;
            battlepassMilestoneDatas = battlepassProfile.GetAllMilestoneData;
            battlepassPremiumMilestoneDatas = battlepassProfile.GetAllPremiumMilestoneData;
            GetData();
            UIMainManager.Instance.homePanel.CheckForBattlepassNotificationVisibility();
        }

        void GetData()
        {
            currentBattlepassLevel = GameManager.Instance.PlayerProfile.BattlepassProfile.CurrentLevel;
            currentBattlepassEXP = GameManager.Instance.PlayerProfile.BattlepassProfile.CurrentEXP;
            foreach (var milestone in regularBattlepassRewardDatas)
            {
                if (milestone.level > maxLevel) maxLevel = milestone.level;
            }

            SetLevelRequirementForNextLevel();
            CheckUnlockOnStartup();
        }

        void CheckUnlockOnStartup()
        {
            foreach (var milestone in regularBattlepassRewardDatas)
            {
                if (milestone.level <= currentBattlepassLevel)
                    battlepassProfile.UpdateMilestoneUnlockData(milestone.level, true);
                // else
                //     battlepassProfile.UpdateMilestoneUnlockData(milestone.level, false);
            }

            foreach (var milestone in premiumBattlepassRewardDatas)
            {
                if (milestone.level <= currentBattlepassLevel)
                    battlepassProfile.UpdatePremiumMilestoneUnlockData(milestone.level, true);
                // else
                //     battlepassProfile.UpdatePremiumMilestoneUnlockData(milestone.level, false);
            }
        }

        #endregion

        #region Add XP

        [Button]
        public void AddXP(int xp)
        {
            currentBattlepassEXP += xp;
            expChanged = true;
            battlepassProfile.SetEXP(currentBattlepassEXP);
            if (!IsMaxLevel(currentBattlepassLevel))
                CheckForLevelUp();
        }

        void CheckForLevelUp()
        {
            //newLevel = GetNearestLevel();
            while (true)
            {
                if (!LeveledUp()) break;
                levelUpCount++;
                battlepassProfile.SetLevel(currentBattlepassLevel);
            }

            // if (newLevel > 0)
            // {
            //     currentBattlepassLevel = newLevel;
            //     battlepassProfile.SetLevel(currentBattlepassLevel);
            // }
        }

        #endregion

        #region Reset Progress Logic

        public void ResetAll()
        {
            currentBattlepassLevel = -1;
            currentBattlepassEXP = 0;
            SetLevelRequirementForNextLevel();
            battlepassProfile.ResetAll();
        }

        #endregion

        #region Getters, Setters

        int GetNearestLevel()
        {
            int nearestLevel = 0;
            int smallestGap = int.MaxValue;
            foreach (var data in regularBattlepassRewardDatas)
            {
                if (data.level > currentBattlepassLevel &&
                    data.expRequirement - currentBattlepassEXP >= 0 &&
                    data.expRequirement - currentBattlepassEXP < smallestGap)
                {
                    nearestLevel = data.level;
                    smallestGap = data.expRequirement - currentBattlepassEXP;
                }
            }

            if (nearestLevel > currentBattlepassLevel)
                return nearestLevel;
            return -1;
        }

        bool LeveledUp()
        {
            if (currentBattlepassEXP >= nextLevelEXPRequirement)
            {
                currentBattlepassEXP -= nextLevelEXPRequirement;
                battlepassProfile.SetEXP(currentBattlepassEXP);
                SetLevelRequirementForNextLevel();
                if (currentBattlepassLevel + 1 <= maxLevel)
                {
                    currentBattlepassLevel++;
                    battlepassProfile.UpdateMilestoneUnlockData(currentBattlepassLevel, true);
                    battlepassProfile.UpdatePremiumMilestoneUnlockData(currentBattlepassLevel, true);
                    TrackingManager.TrackEvent(TrackingEvent.BattlePass, TrackingParamter.Level,
                        currentBattlepassLevel.ToString());
                }
                return true;
            }
            return false;
        }

        bool IsMaxLevel(int level)
        {
            if (level >= maxLevel) return true;
            return false;
        }

        void SetLevelRequirementForNextLevel()
        {
            if (IsMaxLevel(currentBattlepassLevel))
            {
                nextLevelEXPRequirement = 9999;
            }
            else
            {
                nextLevelEXPRequirement = regularBattlepassRewardDatas[currentBattlepassLevel + 1].expRequirement;
            }
        }

        public void ResetChangesStatus()
        {
            levelUpCount = 0;
            expChanged = false;
        }

        public int CurrentBattlepassLevel => currentBattlepassLevel;
        public int CurrentBattlepassEXP => currentBattlepassEXP;
        public int LevelUpCount => levelUpCount;
        public int MaxLevel => maxLevel;
        public bool ExpChanged => expChanged;

        public float GetPercentageToNextLevel()
        {
            if (IsMaxLevel(currentBattlepassLevel)) return 1f;
            if (nextLevelEXPRequirement <= 0) return 0f;
            return (float)currentBattlepassEXP / nextLevelEXPRequirement;
        }

        public bool IsThereUnclaimedReward()
        {
            foreach (var data in battlepassMilestoneDatas)
            {
                if (data.unlockStatus && !data.claimedStatus)
                    return true;
            }

            if (!battlepassProfile.IsBattlePassPurchased) return false;
            foreach (var data in battlepassPremiumMilestoneDatas)
            {
                if (data.unlockStatus && !data.claimedStatus)
                    return true;
            }

            return false;
        }

        #endregion
    }
}