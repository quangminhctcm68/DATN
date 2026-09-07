using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BrickEscape
{
    public class HeartManager : MonoBehaviour
    {
        [SerializeField] private bool clockIsActive;

        [SerializeField] private int maxHeart;
        [SerializeField] private int currentHeart;

        [SerializeField] private int timeToRecoverOneHeart_InSeconds;
        [SerializeField] private int remainingTime;

        private int currentTime
        {
            get 
            {
                return remainingTime;
            }
            set
            {
                remainingTime = value;
                SaveRemainingTimeData();
            }
        }
        
        [SerializeField] private int remainingUnlimitedHeartTime;

        private Coroutine heartClock;
        private Coroutine unlimitedHeartClock;

        #region Start, Update, Validate

        [Button]
        public void SetInfo()
        {
            GetDataOnStartup();
            CheckTime();
            CheckForUnlimitedHeart();
        }

        #endregion

        #region Startup Logic

        public void GetDataOnStartup()
        {
            maxHeart = GameManager.Instance.PlayerProfile.HeartProfile.MaxHeart;
            currentHeart = GameManager.Instance.PlayerProfile.HeartProfile.CurrentHeart;
            UpdateHeartDataToUI();
            //GameManager.Instance.PlayerProfile.HeartProfile.SetLastLoginTime();
        }

        #endregion

        #region Change Heart Values

        [Button]
        public void ReduceHeart()
        {
            if (IsUnlimitedHeartActive()) return;
            currentHeart--;
            ClampHeartValue();
            UpdateHeartDataToUI();
            SaveHeartData();
            StartHeartClock();

        }

        public void ReduceHeart(int newHeartValue)
        {
            if (IsUnlimitedHeartActive()) return;
            currentHeart = newHeartValue;
            ClampHeartValue();
            UpdateHeartDataToUI();
            SaveHeartData();
            StartHeartClock();
        }

        public void ReduceHeart(int heartValueToReduce, bool reduceMode)
        {
            if (IsUnlimitedHeartActive()) return;
            currentHeart -= heartValueToReduce;
            ClampHeartValue();
            UpdateHeartDataToUI();
            SaveHeartData();
            StartHeartClock();
        }

        [Button]
        public void GainHeart()
        {
            if (IsUnlimitedHeartActive())
            {
                currentHeart++;
                ClampHeartValue();
                SaveHeartData();
                return;
            }
            
            currentHeart++;
            ClampHeartValue();
            UpdateHeartDataToUI();
            SaveHeartData();
            UpdateHeartDataToUI();
        }

        public void GainHeart(int newHeartValue)
        {
            if (IsUnlimitedHeartActive())
            {
                currentHeart = newHeartValue;
                ClampHeartValue();
                SaveHeartData();
                return;
            }
            
            currentHeart = newHeartValue;
            ClampHeartValue();
            UpdateHeartDataToUI();
            SaveHeartData();
            UpdateHeartDataToUI();
        }

        public void GainHeart(int heartValueToGain, bool addMode)
        {
            if (IsUnlimitedHeartActive())
            {
                currentHeart += heartValueToGain;
                ClampHeartValue();
                SaveHeartData();
                return;
            }
            
            currentHeart += heartValueToGain;
            ClampHeartValue();
            UpdateHeartDataToUI();
            SaveHeartData();
            UpdateHeartDataToUI();
        }

        void ClampHeartValue()
        {
            currentHeart = Mathf.Clamp(currentHeart, 0, maxHeart);
        }

        void SaveHeartData()
        {
            GameManager.Instance.PlayerProfile.HeartProfile.ChangeCurrentHeart(currentHeart);
            //GameManager.Instance.PlayerProfile.HeartProfile.ChangeMaxHeart(maxHeart);
        }
        private bool isSetColor = false;
        void UpdateHeartDataToUI()
        {
            if (unlimitedHeartIsActive) return;
            if (unlimitedHeartIsActive) return;

            string color = currentHeart == 0 ? "red" : "white";
            string heartText = $"<color={color}>{currentHeart}</color>/5";

            UIMainManager.Instance.homePanel.Heart.text = heartText;
        }

        #endregion

        #region Heart Regen

        public void StartHeartClock()
        {
            if (clockIsActive) return;
            StopHeartClock();

            clockIsActive = true;
            currentTime = timeToRecoverOneHeart_InSeconds;
            heartClock = StartCoroutine(HeartGenerator());
        }

        public void StartHeartClock(bool timeCheckVer)
        {
            if (clockIsActive) return;
            StopHeartClock();

            clockIsActive = true;
            heartClock = StartCoroutine(HeartGenerator());
        }

        public void StopHeartClock()
        {
            if (heartClock != null)
            {
                StopCoroutine(heartClock);
                heartClock = null;
                clockIsActive = false;
            }
        }

        IEnumerator HeartGenerator()
        {
            while (currentHeart < maxHeart)
            {
                yield return new WaitForSeconds(1);
                currentTime--;
                if (currentTime < 0)
                {
                    GainHeart();
                    if (currentHeart < maxHeart) 
                    {
                        currentTime = timeToRecoverOneHeart_InSeconds;
                        UpdateRemainingTimeToUI();
                    }
                        
                    else
                    {
                        UpdateEmptyRemainingTimeToUI();
                        currentTime = 0;
                    }
                        
                }
                else
                {
                    //Update UI
                    UpdateRemainingTimeToUI();
                }
            }

            heartClock = null;
            clockIsActive = false;
        }

        void SaveRemainingTimeData()
        {
            GameManager.Instance.PlayerProfile.HeartProfile.SetLastSessionRemainingTime(currentTime);
        }

        void UpdateRemainingTimeToUI()
        {
            if (unlimitedHeartIsActive) return;
            UIMainManager.Instance.homePanel.HeartTimer.text = TimeSpan.FromSeconds(currentTime).ToString(@"mm\:ss");
        }

        void UpdateEmptyRemainingTimeToUI()
        {
            if (unlimitedHeartIsActive) return;
            UIMainManager.Instance.homePanel.HeartTimer.text = "";
        }

        #endregion

        #region Time Checker

        [SerializeField] private long lastTimeLogin;
        [SerializeField] private long currentTimestamp;
        [SerializeField] private int lastRemainingTime;
        [SerializeField] private long timePassed;

        [SerializeField] private int heartsToAdd;
        [SerializeField] private int remainderSeconds;
        [SerializeField] private bool unlimitedHeartIsActive = false;

        private TimeSpan unlimitedHeartTimeSpan;

        public void CheckTime()
        {
            if (currentHeart >= maxHeart) return;

            lastTimeLogin = GameManager.Instance.PlayerProfile.HeartProfile.LastLoginTime;
            currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            timePassed = currentTimestamp - lastTimeLogin;

            if (timePassed >= 0)
            {
                heartsToAdd = (int)(timePassed / timeToRecoverOneHeart_InSeconds);

                GainHeart(heartsToAdd, true);
                //currentHeart += heartsToAdd;
                ClampHeartValue();
                SaveHeartData();

                if (currentHeart >= maxHeart)
                {
                    StopHeartClock();
                    //UpdateHeartDataToUI();
                    //Update UI
                    return;
                }
                //Update UI
                
                remainderSeconds = (int)(timePassed % timeToRecoverOneHeart_InSeconds);

                lastRemainingTime = GameManager.Instance.PlayerProfile.HeartProfile.LastSessionRemainingTime;
                int temp = GameManager.Instance.PlayerProfile.HeartProfile.LastSessionRemainingTime - remainderSeconds;
                if (temp > 0)
                {
                    currentTime = temp;
                    StartHeartClock(true);
                    //UpdateHeartDataToUI();
                }
                else
                {
                    GainHeart();
                    heartsToAdd = 1;
                    if (currentHeart >= maxHeart)
                    {
                        StopHeartClock();
                        //UpdateHeartDataToUI();
                    }
                    else
                    {
                        currentTime = timeToRecoverOneHeart_InSeconds + temp;
                        StartHeartClock(true);
                        //UpdateHeartDataToUI();
                    }
                }
            }
            else
            {
                currentTime = timeToRecoverOneHeart_InSeconds;
                StartHeartClock(true);
                //UpdateHeartDataToUI();
            }
        }

        #endregion
        
        #region Unlimited Heart Time Checker

        public void CheckForUnlimitedHeart()
        {
            remainingUnlimitedHeartTime = GameManager.Instance.PlayerProfile.HeartProfile.UnlimitedHeartRemainingTime;
            if (remainingUnlimitedHeartTime > 0)
            {
                unlimitedHeartIsActive = true;
                UpdateUnlimitedHeartStatusToUI(true);
                if(unlimitedHeartClock == null)
                    unlimitedHeartClock = StartCoroutine(UnlimitedHeartClock());
            }
            else
            {
                unlimitedHeartIsActive = false;
                UpdateUnlimitedHeartStatusToUI(false);
            }
        }
        
        public void AddUnlimitedHeartTime(int timeToAdd)
        {
            remainingUnlimitedHeartTime += timeToAdd;

            unlimitedHeartIsActive = true;
            UpdateUnlimitedHeartStatusToUI(true);
            SaveUnlimitedHeartTime();
            if (unlimitedHeartClock == null)
                unlimitedHeartClock = StartCoroutine(UnlimitedHeartClock());
        }

        public void SaveUnlimitedHeartTime()
        {
            GameManager.Instance.PlayerProfile.HeartProfile.SetUnlimitedHeartTime(remainingUnlimitedHeartTime);
        }
        
        IEnumerator UnlimitedHeartClock()
        {
            while (remainingUnlimitedHeartTime >= 0)
            {
                yield return new WaitForSeconds(1);
                remainingUnlimitedHeartTime--;
                GameManager.Instance.PlayerProfile.HeartProfile.SetUnlimitedHeartRunTime(remainingUnlimitedHeartTime);
                if (remainingUnlimitedHeartTime < 0)
                {
                    //Update UI
                    unlimitedHeartIsActive = false;
                    UpdateUnlimitedHeartStatusToUI(false);
                    UpdateHeartDataToUI();
                    UpdateEmptyRemainingUnlimitedHeartTimeToUI();
                }
                else
                {
                    //Update UI
                    UpdateRemainingUnlimitedHeartTimeToUI();
                }
            }

            unlimitedHeartClock = null;
        }
        
        void UpdateUnlimitedHeartStatusToUI(bool status)
        {
            UIMainManager.Instance.homePanel.ChangeInfiniteHeartIconVisibility(status);
        }
        
        void UpdateRemainingUnlimitedHeartTimeToUI()
        {
            unlimitedHeartTimeSpan = TimeSpan.FromSeconds(remainingUnlimitedHeartTime);
            
            if (unlimitedHeartTimeSpan.Hours > 0)
            {
                UIMainManager.Instance.homePanel.HeartTimer.SetText($"{(int)unlimitedHeartTimeSpan.TotalHours:D2}:{unlimitedHeartTimeSpan.Minutes:D2}:{unlimitedHeartTimeSpan.Seconds:D2}");
            }
            else
            {
                UIMainManager.Instance.homePanel.HeartTimer.SetText($"{unlimitedHeartTimeSpan.Minutes:D2}:{unlimitedHeartTimeSpan.Seconds:D2}");
            }
            //UIMainManager.Instance.homePanel.HeartTimer.text = TimeSpan.FromSeconds(remainingUnlimitedHeartTime).ToString(@"mm\:ss");
        }
        
        void UpdateEmptyRemainingUnlimitedHeartTimeToUI()
        {
            UIMainManager.Instance.homePanel.HeartTimer.text = "";
        }
        
        #endregion
        
        #region Getters, Setters

        public bool IsUnlimitedHeartActive()
        {
            return remainingUnlimitedHeartTime > 0;
        }
        public int GetRemainingUnlimitedHeartTime()
        {
            return remainingUnlimitedHeartTime;
        }

        #endregion
    }
}
