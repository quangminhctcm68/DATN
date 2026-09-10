using System;
using System.Collections;
using System.Collections.Generic;
using BMH.Ads;

#if BMH_FIREBASE_ANALYTIC
using NabaGame.Tracking;
#endif
using UnityEngine;

public class AOAView : AdsCallShow, IAdsView
{
    public AOAView(AdManager adManager, string adUnit) : base(adManager, adUnit)
    {
        MaxSdkCallbacks.AppOpen.OnAdHiddenEvent += OnAppOpenDismissedEvent;
        MaxSdkCallbacks.AppOpen.OnAdLoadFailedEvent += OnAppOpenLoadFailedEvent;
        MaxSdkCallbacks.AppOpen.OnAdLoadedEvent += OnAppOpenLoadEvent;
        MaxSdkCallbacks.AppOpen.OnAdDisplayedEvent += OnAppOpenDisplayEvent;
        MaxSdkCallbacks.AppOpen.OnAdDisplayFailedEvent += OnAppOpenDisplayFailedEvent;

        MaxSdkCallbacks.AppOpen.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent;
        Load();
    }

    private void OnAdRevenuePaidEvent(string arg1, MaxSdkBase.AdInfo arg2)
    {
        adManager.OnAdRevenuePaidEvent(adUnit, arg2, AdsType.AppOpen);
    }

    private void OnAppOpenDisplayFailedEvent(string arg1, MaxSdkBase.ErrorInfo arg2, MaxSdkBase.AdInfo arg3)
    {
        status = AsdStatus.None;
    }

    private void OnAppOpenDisplayEvent(string arg1, MaxSdkBase.AdInfo arg2)
    {
#if BMH_FIREBASE_ANALYTIC
        TrackingManager.TrackEvent("AdsAOA_Displayed");
#endif
    }

    private void OnAppOpenLoadEvent(string arg1, MaxSdkBase.AdInfo arg2)
    {
        status = AsdStatus.Loaded;
        Debug.Log("AOA Done");
        LoadSusscess();
#if BMH_FIREBASE_ANALYTIC
        TrackingManager.TrackEvent("AdsAOA_LoadDone", "load_interval", ToStringTimeLoad());
#endif
    }

    private void OnAppOpenLoadFailedEvent(string arg1, MaxSdkBase.ErrorInfo arg2)
    {
        Debug.Log("AOA Failed");
        status = AsdStatus.None;
        LoadFailed();
#if BMH_FIREBASE_ANALYTIC
        TrackingManager.TrackEvent("AdsAOA_LoadFailded", "load_interval", ToStringTimeLoad());
#endif
    }

    private void OnAppOpenDismissedEvent(string arg1, MaxSdkBase.AdInfo arg2)
    {
        status = AsdStatus.None;
#if BMH_FIREBASE_ANALYTIC
        TrackingManager.TrackEvent("AdsAOA_Closed");
#endif
    }

    public void Load()
    {
        if (!adManager.adsConfig.shouldShowAppOpen) return;
        if (!CanLoadAds()) return;
        if (IsAdsLoaded()) return;

        lastLoadAds = DateTime.Now;
        status = AsdStatus.Loading;
        MaxSdk.LoadAppOpenAd(adUnit);
        Debug.Log("AOA call load");
#if BMH_FIREBASE_ANALYTIC
        TrackingManager.TrackEvent("AdsAOA_CallLoad");
#endif
    }

    public void CheckLoad()
    {
        if (!IsAdsLoaded())
            Load();
    }

    public bool IsAdsLoaded()
    {
        if (!adManager.adsConfig.shouldShowAppOpen) return false;
        return MaxSdk.IsAppOpenAdReady(adUnit);
    }

    public void Show(Action callback, string placement)
    {
        if (!adManager.adsConfig.shouldShowAppOpen) return;
        if (!adManager.CheckTimeShowOpen()) return;
        if (IsAdsLoaded())
        {
            status = AsdStatus.None;
            adManager.lastTimeShowOpen = DateTime.Now;
            MaxSdk.ShowAppOpenAd(adUnit);
            Debug.Log("AOA Call Show");
#if BMH_FIREBASE_ANALYTIC
            TrackingManager.TrackEvent("AdsAOA_CallShow", "placement", placement);
#endif
        }
#if BMH_FIREBASE_ANALYTIC
        TrackingManager.TrackEvent("AdsAOA_PassedLogic", "placement", placement);
#endif
    }

    public void ShowReplace(Action callback, string placement)
    {
    }

    public void Hide()
    {
    }
}