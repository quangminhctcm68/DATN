using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MaxSdkBase;
using static UnityEngine.Rendering.DebugUI;

namespace BrickEscape
{
    [Serializable]
    public class QuestProfile 
    {
        public int CoinCollectDaily = 0;
        public int WatchAdsCountDaily = 0;
        public int ClearBlockCountDaily = 0;
        public int UseBoosterCountDaily = 0;
        public int CollectDailyBadge = 0; // kiem tra tien do Quest cua Weekly
        public int CollectWeeklyBadge = 0;// kiem tra tien do Quest cua Monthly
        public int CollectMonthlyBadge = 0; //kiem tra reward Monthly

        public int CollectDailyBadgeReward = 0; // kiem tra reward daily
        public int CollectWeeklyBadgeReward = 0;// kiem tra reward weekly



        public Dictionary<int,bool> DailyQuest = new Dictionary<int, bool>();
        public Dictionary<int,bool> WeeklyQuest = new Dictionary<int,bool>();
        public Dictionary<int,bool> MonthlyQuest = new Dictionary<int,bool>();


        public Dictionary<int, bool> DailyQuestReward = new Dictionary<int, bool>();
        public Dictionary<int, bool> WeeklyQuestReward = new Dictionary<int, bool>();
        public Dictionary<int, bool> MonthlyQuestReward = new Dictionary<int, bool>();

        const int DEFAULT_DAILY = 5;
        const int DEFAULT_WEEKLY = 5;
        const int DEFAULT_MONTHLY = 3;

        const string DAILY_KEY = "DQ_";
        const string WEEKLY_KEY = "WQ_";
        const string MONTHLY_KEY = "MQ_";
       
        const string DAILY_R_KEY = "DQ_R";
        const string WEEKLY_R_KEY = "WQ_R";
        const string MONTHLY_R_KEY = "MQ_R";
        public QuestProfile() { }   

        public void Load()
        {
            CoinCollectDaily = PlayerPrefs.GetInt(StringConsts.COIN_COLLECT_DAILY, 0);
            WatchAdsCountDaily = PlayerPrefs.GetInt(StringConsts.WATCH_ADS_COUNT_DAILY, 0);
            ClearBlockCountDaily = PlayerPrefs.GetInt(StringConsts.CLEAR_BLOCK_COUNT_DAILY, 0);
            UseBoosterCountDaily = PlayerPrefs.GetInt(StringConsts.USE_BOOSTER_COUNT_DAILY, 0);
            CollectDailyBadge = PlayerPrefs.GetInt(StringConsts.COLLECT_DAILY_BADGE, 0);
            CollectWeeklyBadge = PlayerPrefs.GetInt(StringConsts.COLLECT_WEEKLY_BADGE, 0);
            CollectMonthlyBadge = PlayerPrefs.GetInt(StringConsts.COLLECT_MONTHLY_BADGE, 0);

            CollectDailyBadgeReward = PlayerPrefs.GetInt(StringConsts.COLLECT_DAILY_BADGE_REWARD, 0);
            CollectWeeklyBadgeReward = PlayerPrefs.GetInt(StringConsts.COLLECT_WEEKLY_BADGE_REWARD, 0);


            DailyQuest = LoadDictionary(DAILY_KEY, DEFAULT_DAILY);
            WeeklyQuest = LoadDictionary(WEEKLY_KEY, DEFAULT_WEEKLY);
            MonthlyQuest = LoadDictionary(MONTHLY_KEY, DEFAULT_MONTHLY);


            DailyQuestReward = LoadDictionary(DAILY_R_KEY, DEFAULT_DAILY);
            WeeklyQuestReward = LoadDictionary(WEEKLY_R_KEY, DEFAULT_WEEKLY);
            MonthlyQuestReward = LoadDictionary(MONTHLY_R_KEY, DEFAULT_MONTHLY);
        }

        public void Save()
        {
            PlayerPrefs.SetInt(StringConsts.COIN_COLLECT_DAILY, CoinCollectDaily);
            PlayerPrefs.SetInt(StringConsts.WATCH_ADS_COUNT_DAILY, WatchAdsCountDaily);
            PlayerPrefs.SetInt(StringConsts.CLEAR_BLOCK_COUNT_DAILY, ClearBlockCountDaily);
            PlayerPrefs.SetInt(StringConsts.USE_BOOSTER_COUNT_DAILY, UseBoosterCountDaily);
            PlayerPrefs.SetInt(StringConsts.COLLECT_DAILY_BADGE, CollectDailyBadge);
            PlayerPrefs.SetInt(StringConsts.COLLECT_WEEKLY_BADGE, CollectWeeklyBadge);
            PlayerPrefs.SetInt(StringConsts.COLLECT_MONTHLY_BADGE, CollectMonthlyBadge);

            PlayerPrefs.SetInt(StringConsts.COLLECT_DAILY_BADGE_REWARD, CollectDailyBadgeReward);
            PlayerPrefs.SetInt(StringConsts.COLLECT_WEEKLY_BADGE_REWARD, CollectWeeklyBadgeReward);

            SaveDictionary(DAILY_KEY, DailyQuest);
            SaveDictionary(WEEKLY_KEY, WeeklyQuest);
            SaveDictionary(MONTHLY_KEY, MonthlyQuest);

            SaveDictionary(DAILY_R_KEY, DailyQuestReward);
            SaveDictionary(WEEKLY_R_KEY, WeeklyQuestReward);
            SaveDictionary(MONTHLY_R_KEY, MonthlyQuestReward);

            PlayerPrefs.Save();
        }

        public void AddDailyWatchAdsCount(int amount) 
        {
            WatchAdsCountDaily += amount;
        }
        public void AddDailyCollectCointCount(int amount)
        {
           CoinCollectDaily += amount;
        }

        public void AddDailyUseBoosterCount(int amount)
        {
            UseBoosterCountDaily  += amount;
        }
        public void AddDailyClearBlockCount(int amount)
        {
           ClearBlockCountDaily += amount;
        }
        public void AddDailyBadgeCount(int amount)
        {
            CollectDailyBadge += amount;
            CollectDailyBadgeReward += amount;
        }
        public void AddWeeklyBadgeCount(int amount)
        {
            CollectWeeklyBadge += amount;
            CollectWeeklyBadgeReward += amount;
        }
        public void AddMonthlyBadgeCount(int amount)
        {
            CollectMonthlyBadge += amount;
        }
        public void ResetDaily()
        {
            CoinCollectDaily = 0;
            WatchAdsCountDaily = 0;
            ClearBlockCountDaily = 0;
            UseBoosterCountDaily = 0;

            CollectDailyBadgeReward = 0;

            ResetDictionary(DailyQuest);
            ResetDictionary(DailyQuestReward);
        }
        public void ResetWeekly() 
        {
            CollectDailyBadge = 0;
            CollectWeeklyBadgeReward= 0;

            ResetDictionary(WeeklyQuest);
            ResetDictionary(WeeklyQuestReward);
        }

        public void ResetMonthly()
        {

            CollectWeeklyBadge = 0;
            CollectMonthlyBadge = 0;

            ResetDictionary(MonthlyQuest);
            ResetDictionary(MonthlyQuestReward);
        }

        public bool GetDailyQuest(int Index)
        {
            if (!DailyQuest.ContainsKey(Index)) 
            {
                DailyQuest.Add(Index, false);
            }

            return DailyQuest[Index];
        }
        public bool GetWeeklyQuest(int Index)
        {
            if (!WeeklyQuest.ContainsKey(Index))
            {
                WeeklyQuest.Add(Index, false);
            }
        
            return WeeklyQuest[Index];
        }
        public bool GetMonthlyQuest(int Index)
        {
            if (!MonthlyQuest.ContainsKey(Index))
            {
                MonthlyQuest.Add(Index, false);
            }
          
            return MonthlyQuest[Index];
        }
        void ResetDictionary(Dictionary<int, bool> dict)
        {
            var keys = new List<int>(dict.Keys);

            for (int i = 0; i < keys.Count; i++)
            {
                dict[keys[i]] = false;
            }
        }
        Dictionary<int, bool> LoadDictionary(string prefix, int defaultCount)
        {
            Dictionary<int, bool> dict = new Dictionary<int, bool>();

            int index = 1;

            while (PlayerPrefs.HasKey(prefix + index))
            {
                bool value = PlayerPrefs.GetInt(prefix + index) == 1;
                dict[index] = value;
                index++;
            }

            // nếu chưa có dữ liệu -> tạo default
            if (dict.Count == 0)
            {
                for (int i = 1; i <= defaultCount; i++)
                {
                    dict[i] = false;
                }
            }

            return dict;
        }
        void SaveDictionary(string prefix, Dictionary<int, bool> dict)
        {
            foreach (var kv in dict)
            {
                PlayerPrefs.SetInt(prefix + kv.Key, kv.Value ? 1 : 0);
            }
        }
    }


}
