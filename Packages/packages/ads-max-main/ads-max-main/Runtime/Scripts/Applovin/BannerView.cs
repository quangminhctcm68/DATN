using System;
using System.Collections;
using System.Collections.Generic;
using NabaGame.Core.Runtime.EventManager;
#if BMH_FIREBASE_ANALYTIC
using NabaGame.Tracking;
#endif
using UnityEngine;

namespace BMH.Ads
{
    public class BannerView : IAdsView
    {
        private string AdUnitId;
        private AdManager adManager;
        private bool isLoaded;

        public BannerView(AdManager _adManager, string ID)
        {
            AdUnitId = ID;
            adManager = _adManager;
            Load();
        }

        public bool IsLoading()
        {
            return true;
        }

        public void Load()
        {
            if (!adManager.adsConfig.shouldShowBanner) return;
#if BMH_APPLOVIN_MAX
            MaxSdkCallbacks.Banner.OnAdLoadedEvent += OnBannerAdLoadedEvent;
            MaxSdkCallbacks.Banner.OnAdLoadFailedEvent += OnBannerAdFailedEvent;
            MaxSdkCallbacks.Banner.OnAdClickedEvent += OnBannerAdClickedEvent;
            MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += OnBannerAdRevenuePaidEvent;

            MaxSdk.CreateBanner(AdUnitId, adManager.adsConfig.bannerPosition);
            string _adaptive = adManager.adsConfig.isAdaptiveBanner ? "true" : "false";
            MaxSdk.SetBannerExtraParameter(AdUnitId, "adaptive_banner", _adaptive);
            MaxSdk.SetBannerExtraParameter(AdUnitId,"use_safe_area", "false");
            MaxSdk.SetBannerBackgroundColor(AdUnitId, adManager.adsConfig.bannerBackgroundColor);
#if BMH_FIREBASE_ANALYTIC
            TrackingManager.TrackEvent($"AdsBanner_CallLoad");
#endif
#endif
        }

        public void UpdatePosition()
        {
            #if BMH_APPLOVIN_MAX
            MaxSdk.UpdateBannerPosition(AdUnitId,adManager.adsConfig.bannerPosition);
            #endif
        }
        
        public void CheckLoad()
        {
        }
        private void OnBannerAdRevenuePaidEvent(string arg1, MaxSdkBase.AdInfo arg2)
        {
            adManager.OnAdRevenuePaidEvent(AdUnitId, arg2, AdsType.Banner);
        }
        private void OnBannerAdClickedEvent(string arg1, MaxSdkBase.AdInfo arg2)
        {
// #if BMH_FIREBASE_ANALYTIC
//             TrackingManager.TrackEvent($"AdsBanner_Click");
// #endif
        }
        private void OnBannerAdFailedEvent(string arg1, MaxSdkBase.ErrorInfo arg2)
        {
// #if BMH_FIREBASE_ANALYTIC
//             Debug.Log("Banner Load Failed");
//             TrackingManager.TrackEvent($"AdsBanner_LoadFailed");
// #endif
        }
        private void OnBannerAdLoadedEvent(string arg1, MaxSdkBase.AdInfo arg2)
        {
            isLoaded = true;
// #if BMH_FIREBASE_ANALYTIC
//             Debug.Log("Banner Loaded");
//             TrackingManager.TrackEvent($"AdsBanner_LoadDone");
// #endif
        }
        public void Show(Action action, string placement)
        {
#if BMH_APPLOVIN_MAX
            MaxSdk.ShowBanner(AdUnitId);
#endif
        }

        public void ShowReplace(Action callback, string placement)
        {
            
        }

        public void Hide()
        {
#if BMH_APPLOVIN_MAX
            MaxSdk.HideBanner(AdUnitId);
#endif
        }
        public bool IsAdsLoaded()
        {
            // MaxSdk.is
            return isLoaded;
        }
    }
}