using System;
using System.Collections;
using System.Collections.Generic;
using BMH.Ads;
#if BMH_FIREBASE_ANALYTIC
using NabaGame.Tracking;
#endif
using UnityEngine;

namespace BMH.Ads
{
    public class RewardView : AdsCallShow, IAdsView
    {
        public RewardView(AdManager adManager, string ID) : base(adManager, ID)
        {
            MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnRewardedAdLoadedEvent;
            MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnRewardedAdFailedEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += OnRewardedAdFailedToDisplayEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += OnRewardedAdDisplayedEvent;
            MaxSdkCallbacks.Rewarded.OnAdClickedEvent += OnRewardedAdClickedEvent;
            MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += OnRewardedAdDismissedEvent;
            MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += OnRewardedAdReceivedRewardEvent;
            MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += OnRewardedAdRevenuePaidEvent;
            Load();
        }

        private void OnRewardedAdRevenuePaidEvent(string arg1, MaxSdkBase.AdInfo arg2)
        {
            adManager.OnAdRevenuePaidEvent(adUnit, arg2, AdsType.Rewarded);
        }

        private void OnRewardedAdReceivedRewardEvent(string arg1, MaxSdkBase.Reward arg2, MaxSdkBase.AdInfo arg3)
        {
            onAdsClosed?.Invoke();
        }

        private void OnRewardedAdDismissedEvent(string arg1, MaxSdkBase.AdInfo arg2)
        {
            adManager.ReseLastShowIntervalInterFromReward();
            status = AsdStatus.None;
            adManager.IsShowingAds(false);
        }

        private void OnRewardedAdClickedEvent(string arg1, MaxSdkBase.AdInfo arg2)
        {
#if BMH_FIREBASE_ANALYTIC
            TrackingManager.TrackEvent("AdsReward_Click");
#endif
        }

        private void OnRewardedAdDisplayedEvent(string arg1, MaxSdkBase.AdInfo arg2)
        {
#if BMH_FIREBASE_ANALYTIC
            TrackingManager.TrackEvent("AdsReward_Displayed");
#endif
        }

        private void OnRewardedAdFailedToDisplayEvent(string arg1, MaxSdkBase.ErrorInfo arg2, MaxSdkBase.AdInfo arg3)
        {
            status = AsdStatus.None;
#if BMH_FIREBASE_ANALYTIC
            TrackingManager.TrackEvent("AdsReward_FailedToDisplay");
#endif
        }

        private void OnRewardedAdFailedEvent(string arg1, MaxSdkBase.ErrorInfo arg2)
        {
            status = AsdStatus.None;
            LoadFailed();
            Debug.Log("Reward Load Failed");
#if BMH_FIREBASE_ANALYTIC
            TrackingManager.TrackEvent("AdsReward_LoadFailed", "load_interval", ToStringTimeLoad());
#endif
        }

        private void OnRewardedAdLoadedEvent(string arg1, MaxSdkBase.AdInfo arg2)
        {
            status = AsdStatus.Loaded;
            LoadSusscess();
            Debug.Log("Reward Loaded");
#if BMH_FIREBASE_ANALYTIC
            TrackingManager.TrackEvent("AdsReward_LoadDone", "load_interval", ToStringTimeLoad());
#endif
        }

        public void Load()
        {
            if (!CanLoadAds()) return;
            if (IsAdsLoaded()) return;


            status = AsdStatus.Loading;
            lastLoadAds = DateTime.Now;
            MaxSdk.LoadRewardedAd(adUnit);
            Debug.Log("Reward Call Load");
#if BMH_FIREBASE_ANALYTIC
            TrackingManager.TrackEvent("AdsReward_CallLoad");
#endif
        }

        public void CheckLoad()
        {
            if (!IsAdsLoaded())
                Load();
        }

        public bool IsAdsLoaded()
        {
            return MaxSdk.IsRewardedAdReady(adUnit);
        }

        public void Show(Action callback, string placement)
        {
            if (IsAdsLoaded())
            {
                onAdsClosed = callback;
                status = AsdStatus.None;
                adManager.IsShowingAds(true);
                MaxSdk.ShowRewardedAd(adUnit, placement);
                Debug.Log("Reward Call Show");
#if BMH_FIREBASE_ANALYTIC
                TrackingManager.TrackEvent("AdsReward_CallShow", "placement", placement);
#endif
            }
#if BMH_FIREBASE_ANALYTIC
            TrackingManager.TrackEvent("AdsReward_PassedLogic", "placement", placement);
#endif
        }

        public void ShowReplace(Action callback, string placement)
        {
        }

        public void Hide()
        {
        }
    }
}