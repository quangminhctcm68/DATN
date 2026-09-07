using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace BrickEscape
{
    public class BattlepassProfile
    {
        [SerializeField] private int currentLevel = -1;
        [SerializeField] private int currentEXP = 0;
        [SerializeField] private List<BattlePassMilestoneData> battlePassMilestoneDatas = new List<BattlePassMilestoneData>();
        [SerializeField] private List<BattlePassPremiumMilestoneData> battlePassPremiumMilestoneDatas = new List<BattlePassPremiumMilestoneData>();
        [SerializeField] private bool isBattlePassPurchased = false;

        [SerializeField] private string startDate;
        
        private string milestoneDatasJson;
        private string premiumMilestoneDatasJson;
        
        #region Edit Data
        
        public void AddEXP(int amount)
        {
            currentEXP += amount;
        }
        
        public void SetEXP(int amount)
        {
            currentEXP = amount;
        }
        
        public void AddLevel(int level)
        {
            if (currentLevel + level > currentLevel)
            {
                UpdateMilestoneUnlockData(currentLevel, level, true);
                UpdatePremiumMilestoneUnlockData(currentLevel, level, true);
            }
            else
            {
                UpdateMilestoneUnlockData(currentLevel, level, false);
                UpdatePremiumMilestoneUnlockData(currentLevel, level, false);
            }
            currentLevel += level;
        }
        
        public void SetLevel(int level)
        {
            currentLevel = level;
        }
        
        public void SetLevel(int level, bool updateUnlockStatus)
        {
            if (currentLevel >= level)
            {
                UpdateMilestoneUnlockData(currentLevel, level, true);
                UpdatePremiumMilestoneUnlockData(currentLevel, level, true);
            }
            else
            {
                UpdateMilestoneUnlockData(currentLevel, level, false);
                UpdatePremiumMilestoneUnlockData(currentLevel, level, false);
            }
            currentLevel = level;
        }

        public void ResetStartDate()
        {
            startDate = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        }
        
        public void SetBattlePassPurchased(bool isPurchased)
        {
            isBattlePassPurchased = isPurchased;
        }
        
        #endregion
        
        #region Edit Claim/Unlock Data

        public void UpdateMilestoneUnlockData(int levelToCheck, bool newUnlockStatus)
        {
            GetMilestoneData(levelToCheck).unlockStatus = newUnlockStatus;

        }
        
        public void UpdatePremiumMilestoneUnlockData(int levelToCheck, bool newUnlockStatus)
        {
            GetPremiumMilestoneData(levelToCheck).unlockStatus = newUnlockStatus;

        }
        
        public void UpdateMilestoneUnlockData(int levelToCheck_start, int levelToCheck_end, bool newUnlockStatus)
        {
            for (int i = levelToCheck_start; i <= levelToCheck_end; i++)
            {
                GetMilestoneData(i).unlockStatus = newUnlockStatus;
            }
        }
        
        public void UpdatePremiumMilestoneUnlockData(int levelToCheck_start, int levelToCheck_end, bool newUnlockStatus)
        {
            for (int i = levelToCheck_start; i <= levelToCheck_end; i++)
            {
                GetPremiumMilestoneData(i).unlockStatus = newUnlockStatus;
            }
        }

        public void UpdateClaimedMilestone(int levelToCheck, bool newClaimStatus)
        {
            GetMilestoneData(levelToCheck).claimedStatus = newClaimStatus;
        }
        
        public void UpdateClaimedPremiumMilestone(int levelToCheck, bool newClaimStatus)
        {
            GetPremiumMilestoneData(levelToCheck).claimedStatus = newClaimStatus;
        }
        
        #endregion
        
        #region Set Time
        
        public long StartTime => long.Parse(startDate);
        
        #endregion
        
        #region Get Data
        
        public BattlePassMilestoneData GetMilestoneData(int milestoneID)
        {
            foreach (var data in battlePassMilestoneDatas)
            {
                if (data.levelRequirement == milestoneID)
                    return data;
            }
            
            BattlePassMilestoneData tempData = new BattlePassMilestoneData();
            tempData.levelRequirement = milestoneID;
            battlePassMilestoneDatas.Add(tempData);
            return tempData;
        }
        
        public BattlePassPremiumMilestoneData GetPremiumMilestoneData(int milestoneID)
        {
            foreach (var data in battlePassPremiumMilestoneDatas)
            {
                if (data.levelRequirement == milestoneID)
                    return data;
            }
            
            BattlePassPremiumMilestoneData tempData = new BattlePassPremiumMilestoneData();
            tempData.levelRequirement = milestoneID;
            battlePassPremiumMilestoneDatas.Add(tempData);
            return tempData;
        }
        
        public int CurrentLevel => currentLevel;
        public int CurrentEXP => currentEXP;
        public bool IsBattlePassPurchased => isBattlePassPurchased;
        public List<BattlePassMilestoneData> GetAllMilestoneData => battlePassMilestoneDatas;
        public List<BattlePassPremiumMilestoneData> GetAllPremiumMilestoneData => battlePassPremiumMilestoneDatas;
        
        #endregion
        
        #region Save, Load

        public void Save()
        {
            PlayerPrefs.SetString(StringConsts.BATTLE_PASS_START_DATE, startDate);
            PlayerPrefs.SetInt(StringConsts.BATTLE_PASS_CURRENT_LEVEL, currentLevel);
            PlayerPrefs.SetInt(StringConsts.BATTLE_PASS_PREMIUM_STATUS, isBattlePassPurchased ? 1 : 0);
            PlayerPrefs.SetInt(StringConsts.BATTLE_PASS_CURRENT_EXP, currentEXP);
            milestoneDatasJson = JsonConvert.SerializeObject(battlePassMilestoneDatas);
            PlayerPrefs.SetString(StringConsts.BATTLE_PASS_REWARD_LIST, milestoneDatasJson);
            premiumMilestoneDatasJson = JsonConvert.SerializeObject(battlePassPremiumMilestoneDatas);
            PlayerPrefs.SetString(StringConsts.BATTLE_PASS_PREMIUM_REWARD_LIST, premiumMilestoneDatasJson);
            PlayerPrefs.Save();
        }

        public void Load()
        {
            currentLevel = PlayerPrefs.GetInt(StringConsts.BATTLE_PASS_CURRENT_LEVEL, -1);
            currentEXP = PlayerPrefs.GetInt(StringConsts.BATTLE_PASS_CURRENT_EXP, 0);
            isBattlePassPurchased = PlayerPrefs.GetInt(StringConsts.BATTLE_PASS_PREMIUM_STATUS, 0) == 1;
            if (PlayerPrefs.HasKey(StringConsts.BATTLE_PASS_REWARD_LIST))
            {
                battlePassMilestoneDatas =
                    JsonConvert.DeserializeObject<List<BattlePassMilestoneData>>
                        (PlayerPrefs.GetString(StringConsts.BATTLE_PASS_REWARD_LIST));
            }

            if (PlayerPrefs.HasKey(StringConsts.BATTLE_PASS_PREMIUM_REWARD_LIST))
            {
                battlePassPremiumMilestoneDatas =
                    JsonConvert.DeserializeObject<List<BattlePassPremiumMilestoneData>>
                        (PlayerPrefs.GetString(StringConsts.BATTLE_PASS_PREMIUM_REWARD_LIST));
            }

            if (PlayerPrefs.HasKey(StringConsts.BATTLE_PASS_START_DATE))
            {
                startDate = PlayerPrefs.GetString(StringConsts.BATTLE_PASS_START_DATE);
                if (string.IsNullOrEmpty(startDate))
                {
                    startDate = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
                }
            }
            else
            {
                startDate = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            }
            
            
        }
        
        #endregion
        
        #region Cheat
        
        public void ResetAll()
        {
            currentLevel = -1;
            currentEXP = 0;
            foreach (var data in battlePassMilestoneDatas)
            {
                data.unlockStatus = false;
                data.claimedStatus = false;
            }
            
            foreach (var data in battlePassPremiumMilestoneDatas)
            {
                data.unlockStatus = false;
                data.claimedStatus = false;
            }
            
            isBattlePassPurchased = false;
            startDate = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        }
        
        public void ResetClaimStatus()
        {
            foreach (var data in battlePassMilestoneDatas)
            {
                data.claimedStatus = false;
            }
            
            foreach (var data in battlePassPremiumMilestoneDatas)
            {
                data.claimedStatus = false;
            }
        }

        public void ResetPremiumPurchaseStatus()
        {
            isBattlePassPurchased = false;
        }

        public void ResetProgress()
        {
            currentLevel = -1;
            currentEXP = 0;
            foreach (var data in battlePassMilestoneDatas)
            {
                data.unlockStatus = false;
                data.claimedStatus = false;
            }
            
            foreach (var data in battlePassPremiumMilestoneDatas)
            {
                data.unlockStatus = false;
                data.claimedStatus = false;
            }
        }
        
        #endregion
    }

    [Serializable]
    public class BattlePassMilestoneData
    {
        [JsonProperty("lvR")] public int levelRequirement = 0;
        [JsonProperty("uS")] public bool unlockStatus = false;
        [JsonProperty("cS")] public bool claimedStatus = false;
    }

    [Serializable]
    public class BattlePassPremiumMilestoneData
    {
        [JsonProperty("lvRP")] public int levelRequirement = 0;
        [JsonProperty("uSP")] public bool unlockStatus = false;
        [JsonProperty("cSP")] public bool claimedStatus = false;
    }
}
