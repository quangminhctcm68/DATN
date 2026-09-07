using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BrickEscape
{
    public class TimeManager : MonoBehaviour
    {
        [FoldoutGroup("Free Coin Data")] [SerializeField] private bool allowReceivingCoins = false;
        [FoldoutGroup("Free Coin Data")] [SerializeField] private int freeCoinWaitTimeInSeconds = 1800;
        [FoldoutGroup("Free Coin Data")] [SerializeField] private int freeCoinCurrentWaitTime = 0;
        
        [FoldoutGroup("Battlepass Timer Data (Seconds)")] [SerializeField] private long battlepassMaximumTime = 2592000;
        [FoldoutGroup("Battlepass Timer Data (Seconds)")] [SerializeField] private long battlepassCurrentTime = 0; 
        [FoldoutGroup("Battlepass Timer Data (Seconds)")] [SerializeField] private string battlepassCurrentTime_InText;
        [FoldoutGroup("Battlepass Timer Data (Seconds)")] [SerializeField] private float battlepassCurrentTime_FillAmount = 1;
        [FoldoutGroup("Battlepass Timer Data (Seconds)")] [SerializeField] private float battlepassCurrentTime_FillAmountPerSec;
        
        [FoldoutGroup("Epic Bundle Timer Data (Seconds)")] [SerializeField] private long epicBundleMaximumTime = 86400;
        [FoldoutGroup("Epic Bundle Timer Data (Seconds)")] [SerializeField] private long epicBundleCurrentTime = 0;
        [FoldoutGroup("Epic Bundle Timer Data (Seconds)")] [SerializeField] private string epicBundleCurrentTime_InText;
        
        private TimeSpan battlepassTimeSpan;
        private BattlepassManager battlepassManager;
        private BattlepassPanel battlepassPanel;
        private BattlepassConfirmPurchasePanel battlepassConfirmPurchasePanel;
        private HomePanel homePanel;

        private TimeSpan epicBundleTimeSpan;
        
        WaitForSeconds waitTime = new WaitForSeconds(1f);
        Coroutine timeCoroutine;
        
        #region Start, Update, Validate

        public void Init()
        {
            GetPanelReferences();
            SetupBattlepassData();
            SetupEpicBundleData();
            ChangeReceiveCoinsPermission(true);
            StartClock();
        }

        void GetPanelReferences()
        {
            if (battlepassManager == null) battlepassManager = GameController.Instance.battlepassManager;
            if (battlepassPanel == null) battlepassPanel = UIMainManager.Instance.battlepassPanel;
            if (battlepassConfirmPurchasePanel == null) battlepassConfirmPurchasePanel = UIMainManager.Instance.battlepassConfirmPurchasePanel;
            if (homePanel == null) homePanel = UIMainManager.Instance.homePanel;
        }
        
        #endregion
        
        #region Time Logic

        void StartClock()
        {
            if (timeCoroutine != null) return;
            timeCoroutine = StartCoroutine(TimeRoutine());
        }

        public void StopClock()
        {
            if (timeCoroutine != null)
            {
                StopCoroutine(timeCoroutine);
                timeCoroutine = null;
            }
        }

        IEnumerator TimeRoutine()
        {
            bool allowClockToContinue = true;
            
            while (allowClockToContinue)
            {
                allowClockToContinue = false;

                if (ReduceFreeCoinTimer())
                {
                    allowClockToContinue = true;
                    UpdateFreeCoinTimer_UI();
                }
                else if (!allowReceivingCoins)
                {
                    ChangeReceiveCoinsPermission(true);
                }

                if (ReduceBattlepassTimer())
                {
                    allowClockToContinue = true;
                    UpdateBattlepassTimer_UI();
                }
                else
                {
                    allowClockToContinue = true;
                    CloseAllBattlepassRelatedPanels();
                    ResetBattlepassProgress();
                    battlepassCurrentTime = battlepassMaximumTime;
                    battlepassCurrentTime_FillAmount = 1;
                    UpdateBattlepassTimer_UI();
                }

                if (ReduceEpicBundleTimer())
                {
                    allowClockToContinue = true;
                    UpdateEpicBundleTimer_UI();
                }
                else
                {
                    allowClockToContinue = true;
                    ResetEpicBundleTimerProgress();
                    UpdateEpicBundleTimer_UI();
                }
                
                yield return waitTime;
            }
            
            timeCoroutine = null;
        }
        
        #endregion
        
        #region Free Coin Countdown

        public void ReceiveCoinAction()
        {
            ChangeReceiveCoinsPermission(false);
            ResetFreeCoinCountdown();
            StartClock();
        }

        void ResetFreeCoinCountdown()
        {
            freeCoinCurrentWaitTime = freeCoinWaitTimeInSeconds + 1;
        }

        bool ReduceFreeCoinTimer()
        {
            if (freeCoinCurrentWaitTime - 1 >= 0)
            {
                freeCoinCurrentWaitTime--;
                return true;
            }
            else
            {
                return false;
            }
        }

        void ChangeReceiveCoinsPermission(bool status)
        {
            allowReceivingCoins = status;
            if (status) UpdateFreeCoinPermission_UI();
        }

        void UpdateFreeCoinTimer_UI()
        {
            //Send data to UI here
            UIMainManager.Instance.homePanel.timerReward.UpdateText(freeCoinCurrentWaitTime);
        }

        void UpdateFreeCoinPermission_UI()
        {
            //Send data to UI here
            UIMainManager.Instance.homePanel.timerReward.CanRecive();
        }

        #endregion
        
        #region Battlepass Logic

        void SetupBattlepassData()
        {
            battlepassCurrentTime = battlepassMaximumTime - (DateTimeOffset.UtcNow.ToUnixTimeSeconds() -
                                    GameManager.Instance.PlayerProfile.BattlepassProfile.StartTime);

            if (battlepassCurrentTime <= 0)
            {
                ResetBattlepassProgress();
                battlepassCurrentTime = battlepassMaximumTime;
            }
            
            battlepassCurrentTime_FillAmountPerSec = 1f / battlepassMaximumTime;
            battlepassCurrentTime_FillAmount = battlepassCurrentTime_FillAmountPerSec * battlepassCurrentTime;
            UpdateBattlepassTimer_UI();
            
            //StartClock();
        }
        
        bool ReduceBattlepassTimer()
        {
            if (battlepassCurrentTime - 1 >= 0)
            {
                battlepassCurrentTime--;
                battlepassCurrentTime_FillAmount -= battlepassCurrentTime_FillAmountPerSec;
                return true;
            }
            else
            {
                return false;
            }
        }
       private bool isLocked = true;
        void UpdateBattlepassTimer_UI()
        {
            //battlepassPanel.UpdateTimer(battlepassCurrentTime);
            //battlepassConfirmPurchasePanel.UpdateTimer(battlepassCurrentTime);
            if(GameManager.Instance.PlayerProfile.LevelProfile.currentLevel < GameController.Instance.battlepassManager.LevelUnlock) 
            {
                if (isLocked) 
                {
                    homePanel.LockBattlePass();
                    isLocked = false;
                }
            }
            else 
            {
                CalculateBattlepassTimerForUI();
                battlepassPanel.UpdateTimer(battlepassCurrentTime_InText);

                battlepassConfirmPurchasePanel.UpdateTimer(battlepassCurrentTime_InText);
                homePanel.UpdateBattlepassTimerText(battlepassCurrentTime_InText);
                homePanel.UpdateFillAmount(battlepassCurrentTime_FillAmount);
            }
             
        }

        void CloseAllBattlepassRelatedPanels()
        {
            battlepassConfirmPurchasePanel.Close(true);
        }

        void ResetBattlepassProgress()
        {
            battlepassManager.ResetAll();
            battlepassPanel.ResetProgress();
        }

        void CalculateBattlepassTimerForUI()
        {
            battlepassTimeSpan = TimeSpan.FromSeconds(battlepassCurrentTime);
            
            if (battlepassTimeSpan.Days >= 1)
            {
                battlepassCurrentTime_InText = $"{battlepassTimeSpan.Days:D2}d {battlepassTimeSpan.Hours:D2}h";
            }
            else if (battlepassTimeSpan.Hours >= 1)
            {
                battlepassCurrentTime_InText = $"{battlepassTimeSpan.Hours:D2}h {battlepassTimeSpan.Minutes:D2}m";
            }
            else if (battlepassTimeSpan.Minutes >= 1)
            {
                battlepassCurrentTime_InText = $"{battlepassTimeSpan.Minutes:D2}m {battlepassTimeSpan.Seconds:D2}s";
            }
            else
            {
                battlepassCurrentTime_InText = $"{battlepassTimeSpan.Seconds:D2}s";
            }
        }
        
        #endregion
        
        #region Epic Bundle

        void SetupEpicBundleData()
        {
            epicBundleCurrentTime = epicBundleMaximumTime - (DateTimeOffset.UtcNow.ToUnixTimeSeconds() -
                                                             GameManager.Instance.PlayerProfile.EpicBundleStartDay);
            if (epicBundleCurrentTime <= 0)
            {
                ResetEpicBundleTimerProgress();
            }
            UpdateEpicBundleTimer_UI();
            //StartClock();
        }

        bool ReduceEpicBundleTimer()
        {
            if (epicBundleCurrentTime - 1 >= 0)
            {
                epicBundleCurrentTime--;
                return true;
            }
            else
            {
                return false;
            }
        }

        void ResetEpicBundleTimerProgress()
        {
            GameManager.Instance.PlayerProfile.UpdateStartDay();
            epicBundleCurrentTime = epicBundleMaximumTime;
        }

        void UpdateEpicBundleTimer_UI()
        {
            CalculateEpicBundleTimerForUI();
            homePanel.UpdateEpicBundleTimerText(epicBundleCurrentTime_InText);
        }
        
        void CalculateEpicBundleTimerForUI()
        {
            epicBundleTimeSpan = TimeSpan.FromSeconds(epicBundleCurrentTime);
            
            if (epicBundleTimeSpan.Days >= 1)
            {
                epicBundleCurrentTime_InText = $"{epicBundleTimeSpan.Days:D2}d {epicBundleTimeSpan.Hours:D2}h";
            }
            else if (epicBundleTimeSpan.Hours >= 1)
            {
                epicBundleCurrentTime_InText = $"{epicBundleTimeSpan.Hours:D2}h {epicBundleTimeSpan.Minutes:D2}m";
            }
            else if (epicBundleTimeSpan.Minutes >= 1)
            {
                epicBundleCurrentTime_InText = $"{epicBundleTimeSpan.Minutes:D2}m {epicBundleTimeSpan.Seconds:D2}s";
            }
            else
            {
                epicBundleCurrentTime_InText = $"{epicBundleTimeSpan.Seconds:D2}s";
            }
        }
        
        #endregion
        
        #region Getters, Setters
        
        public bool AllowReceivingCoins => allowReceivingCoins;
        public int FreeCoinCurrentWaitTime => freeCoinCurrentWaitTime;
        
        #endregion
        
        #region Cheat Section

        public void ChangeBattlepassTimer(long time)
        {
            battlepassCurrentTime = time;
        }
        
        #endregion
    }
}

