using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using NabaGame.UI;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;

namespace BrickEscape
{
    public class DailyQuestPanel : BaseUI
    {

        [Header("Timer Text")]
        [SerializeField] private TMP_Text _timerText;

        private DateTime _firstLoginTime;
        public RectTransform ProcessParent;
        public RectTransform TaskParent;
        [SerializeField] private int _currentTab;
        [SerializeField] Tab[] tabs;
        #region INIT
        public List<DailyQuest> dailyQuests ;
        public List<WeeklyQuest> weeklyQuests;
        public List<MonthlyQuest> monthlyQuests;
        public ScrollRect scrollRect ;
        public RewardProcess dailyProcess ;
        public RewardProcess weeklyProcess ;
        public RewardProcess monthlyProcess ;
        public RectTransform dailybadge;
        public RectTransform weeklybadge;
        public RectTransform monthlybadge;


        GameObject notify;

        public override void OnInAnimationStart()
        {
            base.OnInAnimationStart();
            CheckInfor();
            dailyProcess.CheckProcess();
            weeklyProcess.CheckProcess();
            monthlyProcess.CheckProcess();
        }
        public void SetInfor()
        {
            InitTime();
            for (int i = 0; i < tabs.Length; i++)
            {
                tabs[i].SetInfor();
            }
            for (int i = 0; i < dailyQuests.Count; i++)
            {
                dailyQuests[i].SetInfor();
            }
            for (int i = 0; i < weeklyQuests.Count; i++)
            {
                weeklyQuests[i].SetInfor();
            }
            for (int i = 0; i < monthlyQuests.Count; i++)
            {
                monthlyQuests[i].SetInfor();
            }
            dailyProcess.SetInfor();
            weeklyProcess.SetInfor();
            monthlyProcess.SetInfor();
            notify = UIMainManager.Instance.homePanel.DailyQuestNotify;
            CheckNotify();
        }
        public void CloseUI() 
        {
            CheckNotify();
            Hide();
        }


        public void CheckInfor() 
        {
            for (int i = 0; i < dailyQuests.Count; i++)
            {
                dailyQuests[i].CheckProcess();
            }
            for (int i = 0; i < weeklyQuests.Count; i++)
            {
                weeklyQuests[i].CheckProcess();
            }
            for (int i = 0; i < monthlyQuests.Count; i++)
            {
                monthlyQuests[i].CheckProcess();
            }
         
        }
        void InitTime()
        {
            if (!PlayerPrefs.HasKey("FIRST_LOGIN_TIME"))
            {
                PlayerPrefs.SetString("FIRST_LOGIN_TIME", DateTime.Now.Ticks.ToString());
            }

            InitResetTime(); 
            long ticks = Convert.ToInt64(PlayerPrefs.GetString("FIRST_LOGIN_TIME"));

            _firstLoginTime = new DateTime(ticks);
        }
        void InitResetTime()
        {
            DateTime now = DateTime.Now;

            if (!PlayerPrefs.HasKey(StringConsts.DAILY_RESET_TIME))
            {
                DateTime next = now.Date.AddDays(1);
                PlayerPrefs.SetString(StringConsts.DAILY_RESET_TIME, next.Ticks.ToString());
            }

            if (!PlayerPrefs.HasKey(StringConsts.WEEKLY_RESET_TIME))
            {
                DateTime next = now.Date.AddDays(7);
                PlayerPrefs.SetString(StringConsts.WEEKLY_RESET_TIME, next.Ticks.ToString());
            }

            if (!PlayerPrefs.HasKey(StringConsts.MONTHLY_RESET_TIME))
            {
                DateTime next = now.Date.AddDays(30);
                PlayerPrefs.SetString(StringConsts.MONTHLY_RESET_TIME, next.Ticks.ToString());
            }
        }

   

        private DateTime _nextDailyReset;
        private DateTime _nextWeeklyReset;
        private DateTime _nextMonthlyReset;

        // Timer để chỉ check 1 lần/giây thay vì mỗi frame
        private float _resetCheckTimer;
        private const float RESET_CHECK_INTERVAL = 1f;

        void Awake()
        {
            _nextDailyReset = LoadOrInitTime(StringConsts.DAILY_RESET_TIME, GetNextDailyTime);
            _nextWeeklyReset = LoadOrInitTime(StringConsts.WEEKLY_RESET_TIME, GetNextWeekTime);
            _nextMonthlyReset = LoadOrInitTime(StringConsts.MONTHLY_RESET_TIME, GetNextMonthTime);
        }

        // ─── Load một lần, không alloc string lại trong runtime ───
        DateTime LoadOrInitTime(string key, Func<DateTime> fallback)
        {
            string raw = PlayerPrefs.GetString(key, null);
            if (raw != null && long.TryParse(raw, out long ticks))
                return new DateTime(ticks);

            DateTime t = fallback();
            PlayerPrefs.SetString(key, t.Ticks.ToString());
            return t;
        }

        private float _nextUpdateTime;
        // ─── Update: dùng float timer, không tạo GC ───
        void Update()
        {
            _resetCheckTimer -= Time.deltaTime;
            if (_resetCheckTimer <= 0f)
            {
                _resetCheckTimer = RESET_CHECK_INTERVAL;
                CheckReset();
            }

            if (!IsVisible()) return;

            float now = Time.realtimeSinceStartup;
            if (now < _nextUpdateTime) return;
            UpdateTimer();
            _nextUpdateTime = now + (_currentTab == -1 ? 1f : 60f);
        }

        // ─── CheckReset: so sánh field, zero alloc ───
        void CheckReset()
        {
            DateTime now = DateTime.Now; // gọi 1 lần duy nhất

            if (now >= _nextDailyReset)
            {
                ResetDaily();
                _nextDailyReset = _nextDailyReset.AddDays(1);
                SaveTime(StringConsts.DAILY_RESET_TIME, _nextDailyReset);
            }

            if (now >= _nextWeeklyReset)
            {
                ResetWeekly();
                _nextWeeklyReset = _nextWeeklyReset.AddDays(7);
                SaveTime(StringConsts.WEEKLY_RESET_TIME, _nextWeeklyReset);
            }

            if (now >= _nextMonthlyReset)
            {
                ResetMonthly();
                _nextMonthlyReset = _nextMonthlyReset.AddMonths(1);
                SaveTime(StringConsts.MONTHLY_RESET_TIME, _nextMonthlyReset);
            }
        }

        // ─── Helpers giữ nguyên ───
        DateTime GetNextDailyTime() => DateTime.Today.AddDays(1);
        DateTime GetNextWeekTime()
        {
            DateTime now = DateTime.Now;
            int d = ((int)DayOfWeek.Monday - (int)now.DayOfWeek + 7) % 7;
            return now.Date.AddDays(d == 0 ? 7 : d);
        }
        DateTime GetNextMonthTime()
        {
            DateTime now = DateTime.Now;
            return new DateTime(now.Year, now.Month, 1).AddMonths(1);
        }


        #endregion

        void SaveTime(string key, DateTime time)
        {
            PlayerPrefs.SetString(key, time.Ticks.ToString());
        }

        [Button]
        public void ResetDaily()
        {
            GameManager.Instance.PlayerProfile.QuestProfile.ResetDaily();
            for (int i = 0; i < dailyQuests.Count; i++)
            {
                dailyQuests[i].CheckProcess();
            }
            dailyProcess.CheckProcess();
        }
        [Button]
        public void ResetWeekly()
        {
            GameManager.Instance.PlayerProfile.QuestProfile.ResetWeekly();
            for (int i = 0; i < weeklyQuests.Count; i++)
            {
                weeklyQuests[i].CheckProcess();
            }
            weeklyProcess.CheckProcess();
        }
        [Button]
        public void ResetMonthly()
        {
            GameManager.Instance.PlayerProfile.QuestProfile.ResetMonthly();
            for (int i = 0; i < monthlyQuests.Count; i++)
            {
                monthlyQuests[i].CheckProcess();
            }

            monthlyProcess.CheckProcess();
        }

        #region TAB SWITCH
        public void SetTab(int Index)
        {
            _currentTab = Index;
            float ProcessTargetY = -Index * 200;
            float TaskTargetX = -Index * 1000;
            DG.Tweening.DOTweenModuleUI.DOAnchorPosY(ProcessParent,ProcessTargetY,0.25f);
            DG.Tweening.DOTweenModuleUI.DOAnchorPosX(TaskParent, TaskTargetX, 0.25f);
            for (int i = 0; i < tabs.Length; i++) 
            {
                if (tabs[i].index == Index) 
                {
                    tabs[i].OnSelected();
                   
                }
                else 
                {
                    tabs[i].OnUnSelected();
                }
            }
         
            UpdateTimer();
            float now = Time.realtimeSinceStartup;
            if (_currentTab == -1)
                _nextUpdateTime = now + 1f;
            else
                _nextUpdateTime = now + 60f;
        }

        #endregion

        #region TIMER
     
        void UpdateTimer()
        {
     
            DateTime now = DateTime.Now;

            switch (_currentTab)
            {
                case -1:
                    UpdateDaily(now);
                    break;

                case 0:
                    UpdateWeekly(now);
                    break;

                case 1:
                    UpdateMonthly(now);
                    break;
            }
        }
        #endregion

        #region DAILY

        void UpdateDaily(DateTime now)
        {
            DateTime tomorrow = now.Date.AddDays(1);

            TimeSpan remain = tomorrow - now;

            _timerText.SetText(
                "{0:00}:{1:00}:{2:00}",
                remain.Hours,
                remain.Minutes,
                remain.Seconds);
        }

        #endregion

        #region WEEKLY

        void UpdateWeekly(DateTime now)
        {
            int daysPassed = (now - _firstLoginTime).Days;

            int cycle = daysPassed / 7;

            DateTime end = _firstLoginTime.AddDays((cycle + 1) * 7);

            TimeSpan remain = end - now;

            _timerText.SetText(
                "{0:00}d {1:00}h",
                remain.Days,
                remain.Hours);
        }

        #endregion

        #region MONTHLY

        void UpdateMonthly(DateTime now)
        {
            int daysPassed = (now - _firstLoginTime).Days;

            int cycle = daysPassed / 30;

            DateTime end = _firstLoginTime.AddDays((cycle + 1) * 30);

            TimeSpan remain = end - now;

            _timerText.SetText(
                "{0:00}d {1:00}h",
                remain.Days,
                remain.Hours);
        }

        #endregion


        public void CheckNotify()
        {
            if(notify== null) return;
            notify.SetActive(ShouldShowNotify());
        }


        public bool ShouldShowNotify()
        {
            CheckInfor();
            dailyProcess.CheckProcess();
            weeklyProcess.CheckProcess();
            monthlyProcess.CheckProcess();

            // Check Quest
            if (HasReadyQuest(dailyQuests)) return true;
            if (HasReadyQuest(weeklyQuests)) return true;
            if (HasReadyQuest(monthlyQuests)) return true;

            // Check Reward Process
            if (HasReadyReward(dailyProcess)) return true;
            if (HasReadyReward(weeklyProcess)) return true;
            if (HasReadyReward(monthlyProcess)) return true;

            return false;

        }
        bool HasReadyQuest<T>(List<T> quests) where T : BaseQuest
        {
            if (quests == null) return false;

            foreach (var q in quests)
            {
                if (q.isReady && !q.isDone)
                    return true;
            }
            return false;
        }
        bool HasReadyReward(RewardProcess process)
        {
            if (process == null || process.questRewards == null) return false;

            foreach (var r in process.questRewards)
            {
                if (r.isReady && !r.isDone)
                    return true;
            }
            return false;
        }




    }
}

