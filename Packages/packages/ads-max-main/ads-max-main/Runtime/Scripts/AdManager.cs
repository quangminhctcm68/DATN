using UnityEngine;
using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using NabaGame.Core.Runtime.EventManager;
using NabaGame.Core.Runtime.Singleton;
using NabaGame.Tracking;
namespace BMH.Ads
{
    [Singleton("AdManger", true)]
#if ODIN_INSPECTOR
    public class AdManager : SerializedSingleton<AdManager>
#else
    public class AdManager : Singleton<AdManager>
#endif
    {
        [SerializeField] bool isFakeBanner;
        [SerializeField] bool isFakeInter;
        [SerializeField] bool isFakeOpenApp;
        [SerializeField] bool isFakeReward;
        public AdsNetworkConfig adsNetworkConfig;
        public AdsConfig adsConfig;
        [SerializeField] private Dictionary<AdsType, string> AndroidUnitId;
        [SerializeField] private Dictionary<AdsType, string> IOSUnitId;
        private IAdsModule _adsModule;
        private bool isShowingAds;
        private AdsRevPaidEvent adsRevPaidEvent = new AdsRevPaidEvent();

        // SDK dùng trực tiếp → giữ DateTime
        public DateTime lastTimeShowOpen;

        // Nội bộ → đổi sang float, zero allocation
        private float _lastTimeShowInter;
        private float _lastTimeShowReward;
        private float _lastTimeCheck;

        private const float LONG_AGO = -9999f;
        private static float Now => Time.realtimeSinceStartup;

    
private bool _suppressNextAppOpen;


public void SuppressNextAppOpen()
{
    _suppressNextAppOpen = true;
}

        public Dictionary<AdsType, string> IosUnitId
        {
            get => IOSUnitId;
            set => IOSUnitId = value;
        }

        public override void Init()
        {
#if UNITY_EDITOR
            isFakeBanner = true;
            isFakeInter = true;
            isFakeReward = true;
            isFakeOpenApp = true;
#endif
            _adsModule = CreateAdModule(adsNetworkConfig.adsModule);
            _adsModule.Init(this, AndroidUnitId, IOSUnitId, adsNetworkConfig.extras);

            _lastTimeShowInter = LONG_AGO;
            _lastTimeShowReward = LONG_AGO;
            _lastTimeCheck = 0f;
            lastTimeShowOpen = DateTime.Now.AddDays(-1);
        }

        public void InitAdsInGame(bool hasShowBanner = true)
        {
            LoadAd(AdsType.Banner);
            LoadAd(AdsType.Interstitial);
            LoadAd(AdsType.Rewarded);

            if (adsConfig.shouldShowMREC)
                LoadAd(AdsType.MREC);

            if (hasShowBanner)
                Show(AdsType.Banner, null, "");
        }

        public void Update()
        {
            float now = Now;
            if (now - _lastTimeCheck < adsConfig.checkLoadAdsInterval) return;

            _lastTimeCheck = now;
            _adsModule.Update();
        }

        // ── Time helpers ─────────────────────────────────────────────────────────

        public bool CheckTimeShowInter()
        {
            float now = Now;
            if (now - _lastTimeShowReward < adsConfig.interAfterRewardTime) return false;
            if (now - _lastTimeShowInter < adsConfig.interAdsIntervalTime) return false;
            return true;
        }

        public bool CheckTimeShowOpen()
        {
            return DateTime.Now.Subtract(lastTimeShowOpen).TotalSeconds >= adsConfig.appOpenAdsInterval;
        }

        public void ReseLastShowIntervalInterFromInter()
        {
            _lastTimeShowInter = Now;
            lastTimeShowOpen = DateTime.Now;
        }

        public void ReseLastShowIntervalInterFromReward()
        {
            _lastTimeShowReward = Now;
            lastTimeShowOpen = DateTime.Now;
        }

        public void ResetLastShowOpenAds()
        {
            lastTimeShowOpen = DateTime.Now;
        }

        // ── Ads Control ──────────────────────────────────────────────────────────

        public void IsShowingAds(bool ishowing)
        {
            isShowingAds = ishowing;
        }

        public void OnFake(bool fakeBanner, bool fakeInter, bool fakeOpenApp, bool fakeReward)
        {
            isFakeBanner = fakeBanner;
            isFakeInter = fakeInter;
            isFakeOpenApp = fakeOpenApp;
            isFakeReward = fakeReward;

            if (isFakeBanner)
                Hide(AdsType.Banner);
        }

        private IAdsModule CreateAdModule(AdsModuleType adsModuleType)
        {
            switch (adsModuleType)
            {
                case AdsModuleType.ApplovinMax:
                    return new ApplovinModule();
                default:
                    return null;
            }
        }

        public bool IsLoaded(AdsType adsType) => _adsModule.IsLoaded(adsType);

        public void LoadAd(AdsType adsType) => _adsModule.LoadAd(adsType);

        public void ShowAdsOpenFirt() => _adsModule.ShowAds(AdsType.AppOpen, null, "");

        public void UpdatePositionBanner(MaxSdkBase.BannerPosition bannerPosition)
        {
            adsConfig.bannerPosition = bannerPosition;
            _adsModule.UpdateBannerPosition();
        }

        public void UpdatePositionMREC(MRECPos mrecPosition)
        {
            adsConfig.mrecPosition = mrecPosition;
            _adsModule.UpdateMRECPosition();
        }

        public void ShowInterstitialAds(string placement)
        {
#if UNITY_EDITOR
            return;
#endif
            Show(AdsType.Interstitial, null, placement);
        }

        public void ShowInterNow(string placement)
        {
            _adsModule.ShowAds(AdsType.Interstitial, null, placement);
        }

        public void ShowRewardedVideo(Action complete, Action skip, string placement)
        {
#if UNITY_EDITOR
            complete?.Invoke();
            return;
#endif
            Show(AdsType.Rewarded, complete, placement);
        }

        public void Show(AdsType adsType, [CanBeNull] Action callback = null, string placement = "")
        {
            float now = Now;

            switch (adsType)
            {
                case AdsType.Banner:
                    if (isFakeBanner) return;
                    _adsModule.ShowAds(adsType, callback, placement);
                    break;

                case AdsType.AppOpen:
                    if (isFakeOpenApp) return;
                    if (!adsConfig.shouldShowAppOpen) return;
                    if (isShowingAds) return;
                    _adsModule.ShowAds(adsType, callback, placement);
                    break;

                case AdsType.Interstitial:
                    if (isFakeInter) return;
                    if (now - _lastTimeShowReward < adsConfig.interAfterRewardTime) return;
                    if (now - _lastTimeShowInter < adsConfig.interAdsIntervalTime) return;
                    _adsModule.ShowAds(adsType, callback, placement);
                    break;

                case AdsType.Rewarded:
#if UNITY_EDITOR
                    callback?.Invoke();
                    return;
#endif
                    if (now - _lastTimeShowReward < adsConfig.rewardAdsIntervalTime) return;
                    if (isFakeReward)
                    {
                        callback?.Invoke();
                        return;
                    }
                    if (_adsModule.IsLoaded(adsType))
                        _adsModule.ShowAds(adsType, callback, placement);
                    else
                        _adsModule.ShowReplace(AdsType.Interstitial, callback, placement);
                    break;

                case AdsType.MREC:
                    _adsModule.ShowAds(adsType, callback, placement);
                    break;
            }
        }

        public void Hide(AdsType adsType) => _adsModule.Hide(adsType);

        private void OnApplicationPause(bool pauseStatus)
        {
             if (!pauseStatus)
    {
        // Tiêu thụ flag một lần rồi clear
        if (_suppressNextAppOpen)
        {
            _suppressNextAppOpen = false;
            return;
        }
        Show(AdsType.AppOpen, null, "");
    }
        }

        public void OnAdsRevPaid(MaxSdkBase.AdInfo info, string _unitId, AdsType _adType)
        {

            adsRevPaidEvent.info = info;
            adsRevPaidEvent.adsType = _adType;
            adsRevPaidEvent.adUnitId = _unitId;
            EventManager.Instance.Raise(adsRevPaidEvent);

        }

        public void OnAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo impressionData, AdsType adsType)
        {

            double revenue = impressionData.Revenue;
            OnAdsRevPaid(impressionData, adUnitId, adsType);
            var impressionParameters = new[]
            {
                new Firebase.Analytics.Parameter("ad_platform", "AppLovin"),
                new Firebase.Analytics.Parameter("ad_source", impressionData.NetworkName),
                new Firebase.Analytics.Parameter("ad_unit_name", impressionData.AdUnitIdentifier),
                new Firebase.Analytics.Parameter("ad_format", impressionData.AdFormat),
                new Firebase.Analytics.Parameter("ad_creative", impressionData.CreativeIdentifier),
                new Firebase.Analytics.Parameter("country", MaxSdk.GetSdkConfiguration().CountryCode),
                new Firebase.Analytics.Parameter("value", revenue),
                new Firebase.Analytics.Parameter("currency", "USD"),
            };
            TrackingManager.TrackEvent("custom_ad_impression", impressionParameters);
        }
    }

    public enum AdsType : byte
    {
        Banner,
        Interstitial,
        Rewarded,
        AppOpen,
        MREC
    }

    public enum AdsModuleType : byte
    {
        ApplovinMax,
        YodoAds
    }
}