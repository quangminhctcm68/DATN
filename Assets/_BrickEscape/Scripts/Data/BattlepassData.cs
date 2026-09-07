using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BrickEscape
{
    [CreateAssetMenu(fileName = "BattlePass Data Collection", menuName = "GameData/BattlePassDataCollection")]
    public class BattlepassData : SerializedScriptableObject
    {
        [SerializeField] private List<BattlepassRewardData_Regular> regularRewards = new List<BattlepassRewardData_Regular>();
        [SerializeField] private List<BattlepassRewardData_Premium> premiumRewards = new List<BattlepassRewardData_Premium>();

        #if UNITY_EDITOR
        
        [SerializeField] private RawBattlePassRewardsRegularData rawBattlePassRewardsRegularData;
        [SerializeField] private RawBattlePassRewardsPremiumData rawBattlePassRewardsPremiumData;
        
        #region Setup Data

        [Button]
        void GetRegularRewardsFromRawDatas()
        {
            regularRewards.Clear();
            
            foreach (var rawRegularReward in rawBattlePassRewardsRegularData.rawBattlePassRewardsRegulars)
            {
                BattlepassRewardData_Regular regularRewardData = new BattlepassRewardData_Regular();
                regularRewardData.level = rawRegularReward.LevelNumber;
                regularRewardData.expRequirement = rawRegularReward.RequiredExp;
                regularRewardData.rewardType = rawRegularReward.RewardType;
                regularRewardData.coinToAdd = rawRegularReward.CoinAmount;
                regularRewardData.boosterUseCountToAdd = rawRegularReward.BoosterAddAmount;
                regularRewardData.avatarIconIDToUnlock = rawRegularReward.AvatarIconID;
                regularRewardData.avatarFrameIDToUnlock = rawRegularReward.AvatarFrameID;
                regularRewardData.infiniteHeartDurationToAdd = rawRegularReward.InfiniteHeartDuration;
                
                regularRewards.Add(regularRewardData);
            }
        }
        
        [Button]
        void GetPremiumRewardsFromRawDatas()
        {
            premiumRewards.Clear();
            
            foreach (var rawPremiumReward in rawBattlePassRewardsPremiumData.rawBattlePassRewardsPremiums)
            {
                BattlepassRewardData_Premium premiumRewardData = new BattlepassRewardData_Premium();
                premiumRewardData.level = rawPremiumReward.LevelNumber;
                premiumRewardData.rewardType = rawPremiumReward.RewardType;
                premiumRewardData.coinToAdd = rawPremiumReward.CoinAmount;
                premiumRewardData.boosterUseCountToAdd = rawPremiumReward.BoosterAddAmount;
                premiumRewardData.avatarIconIDToUnlock = rawPremiumReward.AvatarIconID;
                premiumRewardData.avatarFrameIDToUnlock = rawPremiumReward.AvatarFrameID;
                premiumRewardData.infiniteHeartDurationToAdd = rawPremiumReward.InfiniteHeartDuration;
                
                premiumRewards.Add(premiumRewardData);
            }
        }
        
        #endregion
        
        #endif
        
        
        #region Get Data

        public List<BattlepassRewardData_Regular> GetAllRegularRewards => regularRewards;
        public List<BattlepassRewardData_Premium> GetAllPremiumRewards => premiumRewards;

        public BattlepassRewardData_Regular GetRegularRewardData(int levelNumber)
        {
            foreach (var regularReward in regularRewards)
            {
                if (regularReward.level == levelNumber)
                    return regularReward;
            }
            return null;
        }
        
        public BattlepassRewardData_Premium GetPremiumRewardData(int levelNumber)
        {
            foreach (var premiumReward in premiumRewards)
            {
                if (premiumReward.level == levelNumber)
                    return premiumReward;
            }
            return null;
        }

        public int GetNumberOfRewards()
        {
            return regularRewards.Count > premiumRewards.Count ? regularRewards.Count : premiumRewards.Count;
        }

        #endregion
    }

    [Serializable]
    public class BattlepassRewardData_Regular
    {
        public int level;
        public int expRequirement;
        public RewardType rewardType;
        public int coinToAdd;
        public int boosterUseCountToAdd;
        public AvatarID avatarIconIDToUnlock;
        public FrameID avatarFrameIDToUnlock;
        public int infiniteHeartDurationToAdd;
    }

    [Serializable]
    public class BattlepassRewardData_Premium
    {
        public int level;
        public RewardType rewardType;
        public int coinToAdd;
        public int boosterUseCountToAdd;
        public AvatarID avatarIconIDToUnlock;
        public FrameID avatarFrameIDToUnlock;
        public int infiniteHeartDurationToAdd;
    }
}
