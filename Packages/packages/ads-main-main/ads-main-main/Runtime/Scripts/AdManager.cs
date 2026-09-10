using UnityEngine;
using System;
using System.Collections.Generic;
using NabaGame.Core.Runtime.Utils;
using NabaGame.Core.Runtime.EventManager;
using NabaGame.Core.Runtime.Singleton;

namespace NabaGame.Ads
{
    [Singleton("AdManger", true)]
#if ODIN_INSPECTOR
    public class AdManager : SerializedSingleton<AdManager>
#else
    public class AdManager : Singleton<AdManager>
#endif
    {
//        public bool isFakeAds = false;
        public bool isFakeBanner;
        public bool isFakeInter;
        public bool isFakeOpenApp;
        public bool isFakeReward;

        public AdsNetworkConfig adsNetworkConfig;
        public AdsConfig adsConfig;

        [SerializeField] private Dictionary<AdsType, string> AndroidUnitId;
        [SerializeField] private Dictionary<AdsType, string> IOSUnitId;

        private IAdsModule _adsModule;
        private DateTime lastTimeShowInter;
        private float loadAdsTimer = 0;
        private float nextInterTime = 0;
        private DateTime lastTimePause;
        private bool isInterOrRewardShowing = true;

        private AdsRevPaidEvent adsRevPaidEvent = new AdsRevPaidEvent();

        public Dictionary<AdsType, string> IosUnitId
        {
            get => IOSUnitId;
            set => IOSUnitId = value;
        }

        public override void Init()
        {
            if (Application.isEditor)
            {
                isFakeBanner = true;
                isFakeInter = true;
                isFakeReward = true;
                isFakeOpenApp = true;
            }

            _adsModule = CreateAdModule(adsNetworkConfig.adsModule);
            _adsModule.Init(this, AndroidUnitId, IOSUnitId, adsNetworkConfig.extras);
            loadAdsTimer = adsConfig.checkLoadAdsInterval;
            lastTimeShowInter = DateTime.Now;
            // CheckShowBanner();
        }

        private void Start()
        {
            if (Application.isEditor)
            {
                isFakeBanner = true;
                isFakeInter = true;
                isFakeReward = true;
                isFakeOpenApp = true;
            }

            SetNextInterTime(false);
        }

        public void Update()
        {
            loadAdsTimer -= Time.unscaledDeltaTime;
            if (loadAdsTimer <= 0)
            {
                loadAdsTimer = adsConfig.checkLoadAdsInterval;
                _adsModule.CheckLoadAds();
            }
        }

        public void OnFake(bool fakeBanner, bool fakeInter, bool fakeOpenApp, bool fakeReward)
        {
            isFakeBanner = fakeBanner;
            isFakeInter = fakeInter;
            isFakeOpenApp = fakeOpenApp;
            isFakeReward = fakeReward;
            if (isFakeBanner)
            {
                HideBanner();
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (!pauseStatus)
            {
                if (!isInterOrRewardShowing)
                {
                    if (ShowAppOpenAds())
                    {
                        lastTimePause = DateTime.Now;
                    }
                }
                else
                {
                    isInterOrRewardShowing = false;
                }
            }
        }

        private IAdsModule CreateAdModule(AdsModuleType adsModuleType)
        {
            IAdsModule adsModule = null;
            switch (adsModuleType)
            {
                case AdsModuleType.ApplovinMax:
                    adsModule = new ApplovinModule();
                    break;
            }

            return adsModule;
        }

        public void ResetAdsParameter()
        {
            lastTimeShowInter = DateTime.Now;
            SetNextInterTime(true);
        }
#if BB_APPLOVIN_MAX
        public void OnAdsRevPaid(MaxSdkBase.AdInfo info, string _unitId, AdsType _adType)
        {
            adsRevPaidEvent.info = info;
            adsRevPaidEvent.adsType = _adType;
            adsRevPaidEvent.adUnitId = _unitId;
            EventManager.Instance.Raise(adsRevPaidEvent);
        }

#endif

        #region Banner

        private void CheckShowBanner()
        {
            if (adsConfig.shouldShowBanner)
            {
                _adsModule.ShowBanner();
            }
            else
            {
                _adsModule.HideBanner();
            }
        }

        public void ShowBanner()
        {
            if (isFakeBanner) return;
            if (adsConfig.shouldShowBanner)
            {
                _adsModule.ShowBanner();
            }
        }

        public void HideBanner()
        {
            _adsModule.HideBanner();
        }

        public void UpdatePositionBanner()
        {
            _adsModule.UpdatePositionBanner();
        }

        #endregion

        #region MREC

        public void ShowMREC()
        {
            _adsModule.ShowMREC();
        }

        public void HideMREC()
        {
            _adsModule.HideMREC();
        }

        #endregion

        #region Interstitial

        public void OnInterStarted()
        {
            isInterOrRewardShowing = true;
        }

        public void OnInterClosed()
        {
            isInterOrRewardShowing = false;
            lastTimeShowInter = DateTime.Now;
            SetNextInterTime(false);
        }

        public void ShowInterstitialAds(string placement, bool ignoreInterval = false, Action closeCallback = null)
        {
            if (!adsConfig.shouldShowInter || isFakeInter)
            {
                closeCallback?.Invoke();
                return;
            }

            if (ignoreInterval)
            {
                if (_adsModule.ShowInterAds(closeCallback, placement))
                {
                    lastTimeShowInter = DateTime.Now;
                }
                else
                {
                    closeCallback?.Invoke();
                }

                return;
            }

            if (DateTime.Now - lastTimeShowInter > TimeSpan.FromSeconds(nextInterTime))
            {
                if (_adsModule.ShowInterAds(closeCallback, placement))
                {
                    lastTimeShowInter = DateTime.Now;
                }
                else
                {
                    closeCallback?.Invoke();
                }
            }
            else
            {
                closeCallback?.Invoke();
            }
        }

        public bool CanInterstitalAdsShow()
        {
            bool isShowTime = DateTime.Now - lastTimeShowInter > TimeSpan.FromSeconds(nextInterTime);
            return isShowTime && _adsModule.IsInterstitalAdsLoaded();
        }

        public bool IsInterstitalAdsLoaded()
        {
            return _adsModule.IsInterstitalAdsLoaded();
        }

        private void SetNextInterTime(bool isAfterReward)
        {
            if (!isAfterReward)
            {
                nextInterTime = adsConfig.interAdsIntervalTime;
            }
            else
            {
                nextInterTime = adsConfig.interAfterRewardTime;
            }
        }

        #endregion

        #region Rewarded

        public void OnRewardedStarted()
        {
            isInterOrRewardShowing = true;
        }

        public void OnRewardedClosed()
        {
            isInterOrRewardShowing = false;
        }

        public bool ShowRewardedVideo(Action closeRewardCallback, Action skipRewardCallback, string placement = null)
        {
            if (Application.isEditor || isFakeReward)
            {
                closeRewardCallback?.Invoke();
                return true;
            }

            if (_adsModule == null)
            {
                Debug.Log("<color=cyan>=>" + "applovinModule is null" + "</color>");
                skipRewardCallback?.Invoke();
                return false;
            }

            if (_adsModule.ShowRewardAds(closeRewardCallback, skipRewardCallback, placement))
            {
                return true;
            }
            else
            {
                if (CheckShowInterWhenNoReward(closeRewardCallback, placement))
                {
                    return true;
                }
                else
                {
                    skipRewardCallback?.Invoke();
                    return false;
                }
            }
        }

        public bool IsRewardAdsLoaded()
        {
            return _adsModule.IsRewardAdsLoaded();
        }

        private bool CheckShowInterWhenNoReward(Action closeRewardCallback, string placement)
        {
            if (!adsConfig.isShowInterWhenNoReward)
            {
                return false;
            }

            if (_adsModule.IsInterstitalAdsLoaded())
            {
                _adsModule.ShowInterAds(closeRewardCallback, placement);
                return true;
            }

            return false;
        }

        public bool IsRewardedVideoLoaded()
        {
#if UNITY_EDITOR
            return true;
#endif
            return _adsModule.IsRewardAdsLoaded();
        }

        #endregion

        #region AppOpen

        public void OnAppOpenStarted()
        {
        }

        public void OnAppOpenClose()
        {
        }

        public void ShowOpenWhhenShowInter()
        {
            if (adsConfig.isShowOpenWhenShowInter && adsConfig.shouldShowAppOpen)
            {
                ShowAppOpenAds();
            }
        }

        public bool ShowAppOpenAds(string placement = "none", Action closeCallback = null)
        {
            if (DateTime.Now - lastTimePause < TimeSpan.FromSeconds(adsConfig.appOpenAdsInterval))
            {
                return false;
            }

            if (!adsConfig.shouldShowAppOpen)
            {
                return false;
            }

            if (isFakeOpenApp)
            {
                closeCallback?.Invoke();
                return true;
            }

            if (_adsModule.ShowAppOpenAds(closeCallback, placement))
            {
                return true;
            }

            closeCallback?.Invoke();
            return false;
        }

        public bool IsAppOpenAdsLoaded()
        {
            return _adsModule.IsAppOpenAdsLoaded();
        }

        #endregion
    }

    public enum AdsType : byte
    {
        Banner,
        Interstitial,
        Rewarded,
        AppOpen,
        MREC,
    }

    public enum AdsModuleType : byte
    {
        ApplovinMax,
        YodoAds
    }
}