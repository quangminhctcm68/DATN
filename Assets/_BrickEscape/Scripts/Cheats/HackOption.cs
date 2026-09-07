using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using BMH.Ads;
using BrickEscape;
using Sirenix.OdinInspector;
using UnityEngine;

public partial class SROptions
{
    [Category("Add money")]
    public int MoneyGainAmount { get; set; }
    [Category("Add money")]
    [Button]
    public void AddDolar()
    {
        GameManager.Instance.PlayerProfile.ChangeCoin(MoneyGainAmount);
    }
    
    [Category("Change Level (Press change twice)")]
    public int Level { get; set; }
    [Category("Change Level (Press change twice)")]
    [Button]
    public void ChangeLevel()
    {
        GameManager.Instance.PlayerProfile.LevelProfile.CheatLevel(Level);
    }

    [Category("No Ads")]
    [Button]
    public void Noads()
    {
        GameManager.Instance.SetNoAds();
    }
    
    [Category("Change Level")]
    public int Heart { get; set; }
    
    [Category("Hearts")]
    [Button]
    public void AddHeart()
    {
        GameController.Instance.heartManager.GainHeart(Heart, true);
    }


    [Category("Tutorial")]
    [Button]
    public void ChangeTutorial_1Status()
    {
        GameManager.Instance.PlayerProfile.FinishedTutorialType_1 = !GameManager.Instance.PlayerProfile.FinishedTutorialType_1;
    }
    
    [Button]
    public void ChangeTutorial_2Status()
    {
        GameManager.Instance.PlayerProfile.FinishedTutorialType_2 = !GameManager.Instance.PlayerProfile.FinishedTutorialType_2;
    }

    // [Category("Inter")] 
    // private bool interStatus { get; set; }
    // [Category("Inter")]
    // [Button]
    // public void ChangeInterCooldownStatus()
    // {
    //     if (!interStatus)
    //     {
    //         UIMainManager.Instance.interAdsPanel.RestartInterCooldown();
    //         interStatus = true;
    //     }
    //     else
    //     {
    //         UIMainManager.Instance.interAdsPanel.StopInterCooldown();
    //         interStatus = false;
    //     }
    // }

    [Category("Banner")]
    private bool bannerStatus{ get; set; }

    [Category("Banner")]
    [Button]
    public void ToggleBanner()
    {
        if (!bannerStatus)
        {
            AdManager.Instance.Show(AdsType.Banner);
            bannerStatus = true;
        }
        else
        {
            AdManager.Instance.Hide(AdsType.Banner);
            bannerStatus = false;
        }
    }
    
    [Category("Piggy Bank")]
    [Button]
    public void BreakPiggyBankForFree()
    {
        UIMainManager.Instance.piggyBankPanel.CashOutAction();
    }
    
    [Category("Piggy Bank")]
    public int MoneyToAdd { get; set; }
    [Category("Piggy Bank")]
    [Button]
    public void AddMoneyToPiggy()
    {
        UIMainManager.Instance.piggyBankPanel.SetNewValueToPiggyBank(MoneyToAdd);
    }
    
    [Category("Battlepass")]
    public int ExpToAdd { get; set; }
    [Category("Battlepass")]
    [Button]
    public void AddExpToBattlepass()
    {
        GameController.Instance.battlepassManager.AddXP(ExpToAdd);
    }
    [Category("Battlepass")]
    [Button]
    public void ResetALLBattlepassProgress()
    {
        GameManager.Instance.PlayerProfile.BattlepassProfile.ResetAll();
    }
    [Category("Battlepass")]
    [Button]
    public void ResetAllBattleClaimProgress()
    {
        GameManager.Instance.PlayerProfile.BattlepassProfile.ResetClaimStatus();
    }
    [Category("Battlepass")]
    [Button]
    public void ResetAllBattlepassPremiumPurchaseStatus()
    {
        GameManager.Instance.PlayerProfile.BattlepassProfile.ResetPremiumPurchaseStatus();
    }
    [Category("Battlepass")]
    [Button]
    public void ResetBattlepassProgress()
    {
        GameManager.Instance.PlayerProfile.BattlepassProfile.ResetProgress();
    }

    
    // [Category("Battlepass")]
    // public long newBattlepassTimer { get; set; }
    // [Category("Battlepass time")]
    // [Button]
    // public void ChangeBattlepassTimer()
    // {
    //     GameController.Instance.timeManager.ChangeBattlepassTimer(newBattlepassTimer);
    // }

    [Category("Unlock Free IAP")]
    private bool freeIAPStatus = false;
    [Category("Unlock Free IAP")]
    [Button]
    public void UnlockFreeIAP()
    {
        freeIAPStatus = !freeIAPStatus;
        GameManager.Instance.cheatData.SetFreeIAP(freeIAPStatus);
        Debug.Log("Free IAP status: " + freeIAPStatus);
    }



    [Button]
    public void CheckUnlimitedHeart()
    {
        Debug.Log("Remaining unlimited heart time: " + GameManager.Instance.PlayerProfile.HeartProfile.UnlimitedHeartRemainingTime);
        Debug.Log(" heart time: " + GameController.Instance.heartManager.GetRemainingUnlimitedHeartTime    ());

    }
}
