using NabaGame.Core.Runtime.EventManager;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BrickEscape
{
    public class HeartProfile
    {
        public int MaxHeart;
        public int CurrentHeart;
        public int LastSessionRemainingTime;

        public int UnlimitedHeartRemainingTime;
        
        public long LastLoginTime => long.Parse(LastTimeCheck);
        private string LastTimeCheck;

        private HeartChange heartChangeEvent;
        private MaxHeartChange maxHeartChangeEvent;

        #region Change Heart Values

        public void ChangeCurrentHeart(int newValue)
        {
            CurrentHeart = newValue;
            if (heartChangeEvent == null) heartChangeEvent = new HeartChange(CurrentHeart);
            heartChangeEvent.currentHeart = CurrentHeart;
            EventManager.Instance.Raise(heartChangeEvent);
        }

        public void ChangeCurrentHeart(int valueToAdd, bool addMode)
        {
            CurrentHeart += valueToAdd;
            CurrentHeart = Mathf.Clamp(CurrentHeart, 0, MaxHeart);
            if (heartChangeEvent == null) heartChangeEvent = new HeartChange(CurrentHeart);
            heartChangeEvent.currentHeart = CurrentHeart;
            EventManager.Instance.Raise(heartChangeEvent);
        }

        public void ChangeMaxHeart(int newValue)
        {
            MaxHeart = newValue;
            if (maxHeartChangeEvent == null) maxHeartChangeEvent = new MaxHeartChange(MaxHeart);
            maxHeartChangeEvent.maxHeart = MaxHeart;
            EventManager.Instance.Raise(maxHeartChangeEvent);
        }

        public void ChangeMaxHeart(int valueToAdd, bool addMode)
        {
            MaxHeart += valueToAdd;
            if (maxHeartChangeEvent == null) maxHeartChangeEvent = new MaxHeartChange(MaxHeart);
            maxHeartChangeEvent.maxHeart = MaxHeart;
            EventManager.Instance.Raise(maxHeartChangeEvent);
        }

        #endregion
        
        #region Unlimited Heart

        public void AddUnlimitedHeartTime(int timeToAdd)
        {
            UnlimitedHeartRemainingTime += timeToAdd;
        }
        
        public void SetUnlimitedHeartTime(int timeToSet)
        {
            UnlimitedHeartRemainingTime = timeToSet;
            Save();
        }
        public void SetUnlimitedHeartRunTime(int timeToSet)
        {
            UnlimitedHeartRemainingTime = timeToSet;
  
        }

        public bool IsUnlimitedHeartActive()
        {
            return UnlimitedHeartRemainingTime > 0;
        }
        
        #endregion

        #region Set Time

        public void SetLastLoginTime()
        {
            LastTimeCheck = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        }

        public void SetLastSessionRemainingTime(int time)
        {
            LastSessionRemainingTime = time;
        }

        #endregion

        #region Save Data

        public void Save()
        {
            PlayerPrefs.SetInt(StringConsts.MAX_HEART, MaxHeart);
            PlayerPrefs.SetInt(StringConsts.CURRENT_HEART, CurrentHeart);
            PlayerPrefs.SetString(StringConsts.LAST_LOGIN_TIME, LastTimeCheck.ToString());
            PlayerPrefs.SetInt(StringConsts.LAST_SESSION_HEART_REMAINING_TIME, LastSessionRemainingTime);
            PlayerPrefs.SetInt(StringConsts.UNLIMITED_HEART_REMAINING_TIME, UnlimitedHeartRemainingTime);

            PlayerPrefs.Save();
        }

        #endregion

        #region Load Data

        public void Load()
        {
            MaxHeart = PlayerPrefs.GetInt(StringConsts.MAX_HEART, 5);
            CurrentHeart = PlayerPrefs.GetInt(StringConsts.CURRENT_HEART, 5);
            LastTimeCheck = PlayerPrefs.GetString(StringConsts.LAST_LOGIN_TIME, "0");
            LastSessionRemainingTime = PlayerPrefs.GetInt(StringConsts.LAST_SESSION_HEART_REMAINING_TIME, 0);
            UnlimitedHeartRemainingTime = PlayerPrefs.GetInt(StringConsts.UNLIMITED_HEART_REMAINING_TIME, 0);
        }

        #endregion
    }
}
