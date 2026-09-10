using System;
using System.Collections;
using System.Collections.Generic;
#if BMH_FIREBASE_ANALYTIC
using NabaGame.Tracking;
#endif
using UnityEngine;

namespace BMH.Ads
{
    public class InterstitialView : AdsCallShow, IAdsView
    {
        public InterstitialView(AdManager adManager, string adUnit) : base(adManager, adUnit)
        {
            MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnInterstitialLoadedEvent;
            MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += OnInterstitialFailedEvent;
            MaxSdkCallbacks.Interstitial.OnAdClickedEvent += OnInterstitialClickedEvent;
            MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += OnInterstitialDismissedEvent;
            MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent += OnInterstitialDisplayEvent;
            MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += OnInterstitialDisplayFailedEvent;
            MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += OnInterstitialRevenuePaidEvent;
            Load();
        }

        public void Load()
        {
            if (!CanLoadAds()) return;
            if (IsAdsLoaded()) return;

            if (adManager.adsConfig.shouldShowInter || adManager.adsConfig.isShowInterWhenNoReward)
            {
                lastLoadAds = DateTime.Now;
                status = AsdStatus.Loading;
                MaxSdk.LoadInterstitial(adUnit);
                Debug.Log("Inter Call Load");
#if BMH_FIREBASE_ANALYTIC
                TrackingManager.TrackEvent("AdsInter_CallLoad");
#endif
            }
        }

        public void CheckLoad()
        {
            if (!IsAdsLoaded())
                Load();
        }

        public bool IsAdsLoaded()
        {
            return MaxSdk.IsInterstitialReady(adUnit);
        }

        private void OnInterstitialRevenuePaidEvent(string arg1, MaxSdkBase.AdInfo arg2)
        {
            adManager.OnAdRevenuePaidEvent(adUnit, arg2, AdsType.Banner);
        }

        private void OnInterstitialClickedEvent(string arg1, MaxSdkBase.AdInfo arg2)
        {
#if BMH_FIREBASE_ANALYTIC
            TrackingManager.TrackEvent("AdsInter_Click");
#endif
        }

        private void OnInterstitialDisplayEvent(string arg1, MaxSdkBase.AdInfo arg2)
        {
#if BMH_FIREBASE_ANALYTIC
            TrackingManager.TrackEvent("AdsInter_Display");
#endif
        }

        private void OnInterstitialDisplayFailedEvent(string arg1, MaxSdkBase.ErrorInfo arg2, MaxSdkBase.AdInfo arg3)
        {
            status = AsdStatus.None;
#if BMH_FIREBASE_ANALYTIC
            TrackingManager.TrackEvent("AdsInter_DisplayFailed");
#endif
        }

        private void OnInterstitialDismissedEvent(string arg1, MaxSdkBase.AdInfo arg2)
        {
            onAdsClosed?.Invoke();
            adManager.ReseLastShowIntervalInterFromInter();
            status = AsdStatus.None;
            adManager.IsShowingAds(false);
        }

        private void OnInterstitialFailedEvent(string arg1, MaxSdkBase.ErrorInfo arg2)
        {
            status = AsdStatus.None;
            LoadFailed();
            Debug.Log("Inter Load Failed");
#if BMH_FIREBASE_ANALYTIC
            TrackingManager.TrackEvent("AdsInter_LoadFailed", "load_interval", ToStringTimeLoad());
#endif
        }

        private void OnInterstitialLoadedEvent(string arg1, MaxSdkBase.AdInfo arg2)
        {
            status = AsdStatus.Loaded;
            LoadSusscess();
            Debug.Log("Inter Loaded");
#if BMH_FIREBASE_ANALYTIC
            TrackingManager.TrackEvent("AdsInter_LoadDone", "load_interval", ToStringTimeLoad());
#endif
        }

        public void Show(Action callback, string placement)
        {
            if (IsAdsLoaded())
            {
                status = AsdStatus.None;
                adManager.IsShowingAds(true);
                MaxSdk.ShowInterstitial(adUnit);
                onAdsClosed = callback;
                Debug.Log("Inter Call Show");
#if BMH_FIREBASE_ANALYTIC
                TrackingManager.TrackEvent("AdsInter_CallShow", "placement", placement);
#endif
            }
#if BMH_FIREBASE_ANALYTIC
            TrackingManager.TrackEvent("AdsInter_PassedLogic", "placement", placement);
#endif
        }

        public void ShowReplace(Action callback, string placement)
        {
            if (IsAdsLoaded())
            {
                status = AsdStatus.None;
                adManager.IsShowingAds(true);
                MaxSdk.ShowInterstitial(adUnit);
                onAdsClosed = callback;
#if BMH_FIREBASE_ANALYTIC
                TrackingManager.TrackEvent("AdsReward_Replace_CallShow", "placement", placement);
#endif
            }
#if BMH_FIREBASE_ANALYTIC
            TrackingManager.TrackEvent("AdsReward_Replace_PassedLogic", "placement", placement);
#endif
        }

        public void Hide()
        {
        }
    }
}