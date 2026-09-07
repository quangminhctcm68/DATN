using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BrickEscape
{
    public class TrackingEvent
    {
        public static readonly string Purchased_IAP ="purchased_IAP";
        public static readonly string Booster_Event = "booster_event";
        public static readonly string Failed_Level = "lailed_level";
        public static readonly string Watched_Video = "watched_video";
        public static readonly string BattlePass = "battle_pass";
        public static readonly string StartLevel = "start_level";
        public static readonly string RestartLevel = "restart_level";
        public static readonly string FinishLevel = "finish_level";
        public static readonly string Claim_Daily_Quest = "claim_daily_quest";
        public static readonly string Daily_Reward = "daily_reward";
        public static readonly string Claim_Daily_Reward = "claim_daily_reward";
        public static readonly string Claim_Monthly_Quest = "claim_monthly_quest";
        public static readonly string WeeklyQuest = "weekly_quest";
        public static readonly string Button_Click = "button_click";
    }

    public class TrackingParamter
    {
        public static readonly string Level = "level";
        public static readonly string Type = "type";
        public static readonly string Count = "count";
    }
}