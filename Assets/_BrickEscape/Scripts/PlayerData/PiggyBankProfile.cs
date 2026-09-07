using System.Collections;
using System.Collections.Generic;
using NabaGame.Core.Runtime.EventManager;
using UnityEngine;

namespace BrickEscape
{
    public class PiggyBankProfile
    {
        public int currentCoins = 0;
        public int maxCoins = 0;
        private PiggyBankValueChange piggyBankValueChangeEvent;
        
        #region Edit
        
        public bool AddCoinsToPiggyBank(int coinsToAdd)
        {
            currentCoins += coinsToAdd;
            ClampPiggyBankCoins();
            return IsPiggyBankFull();
        }

        public bool AddCoinsToPiggyBank(int coinsToSet, bool setMode)
        {
            currentCoins = coinsToSet;
            ClampPiggyBankCoins();
            return IsPiggyBankFull();
        }
        
        public void AddMaxCoinsToPiggyBank(int coinsToAdd)
        {
            maxCoins += coinsToAdd;
            ClampPiggyBankCoins();
        }
        
        public void AddMaxCoinsToPiggyBank(int coinsToSet, bool setMode)
        {
            maxCoins += coinsToSet;
            ClampPiggyBankCoins();
        }

        public void CashOut()
        {
            currentCoins = 0;
        }

        public void CashOut(int specificAmount)
        {
            currentCoins -= specificAmount;
            ClampPiggyBankCoins();
        }
        
        void ClampPiggyBankCoins()
        {
            currentCoins = Mathf.Clamp(currentCoins, 0, maxCoins);
        }

        void SendEvent()
        {
            if (piggyBankValueChangeEvent == null) piggyBankValueChangeEvent = new PiggyBankValueChange();
            piggyBankValueChangeEvent.currentPiggyBankValue = currentCoins;
            EventManager.Instance.Raise(piggyBankValueChangeEvent);
        }
        
        #endregion
        
        #region Getters, Setters
        
        public bool IsPiggyBankFull()
        {
            return currentCoins >= maxCoins;
        }
        
        public void SetMaxCoins(int newMax)
        {
            maxCoins = newMax;
            ClampPiggyBankCoins();
        }
        
        public bool IsAddMoreCoinPossible(int coinsToAdd)
        {
            return maxCoins - currentCoins >= coinsToAdd;
        }
        
        #endregion
        
        #region Save, Load

        public void SavePiggyBankData()
        {
            PlayerPrefs.SetInt(StringConsts.PIGGY_BANK_VALUE, currentCoins);
            PlayerPrefs.Save();
        }
        
        public void LoadPiggyBankData()
        {
            currentCoins = PlayerPrefs.GetInt(StringConsts.PIGGY_BANK_VALUE, 0);
        }
        
        #endregion
    }
}
