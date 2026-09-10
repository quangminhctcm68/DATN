using UnityEngine;
using System;
using System.Collections.Generic;
#if NB_APPFLYER
using AppsFlyerSDK;
#endif
using NabaGame.Tracking;

#if NB_BYTEBREW_ANALYTIC
using ByteBrewSDK;
#endif

namespace NabaGame.Ads
{
    public class ApplovinModule : IAdsModule
    {
        private Action onRewardAdsSkip;
        private Action onRewardAdsClosed;
        private Action onInterAdsClosed;
        private Action onAppOpenAdsClosed;
        private bool IsRewarded;
        private AdManager adManger;
        private Dictionary<AdsType, string> adsUnitIds;

        private const string place = "placement";
        // private string placement = "";
        private bool isInit;

        public void Init(AdManager _adManger, Dictionary<AdsType, string> androidUnitIds,
            Dictionary<AdsType, string> iosUnitIds, params string[] others)
        {
            adManger = _adManger;
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                adsUnitIds = iosUnitIds;
            }
            else
            {
                adsUnitIds = androidUnitIds;
            }

            Debug.Log("Init Applovin");
#if BB_APPLOVIN_MAX
            string sdkKey = others[0];
            MaxSdkCallbacks.OnSdkInitializedEvent += (MaxSdkBase.SdkConfiguration sdkConfiguration) =>
            {
                if (adManger.adsConfig.isTestAds)
                {
                    MaxSdk.ShowMediationDebugger();
                }
                LoadAppOpenAd();
                if (!adManger.adsConfig.shouldShowAppOpen)
                    LoadAdsInGame();
            };
#if NB_APPFLYER
             MaxSdk.SetUserId(AppsFlyer.getAppsFlyerId());
#endif
            MaxSdk.SetSdkKey(sdkKey);
            if (adManger.adsConfig.isTestAds)
            {
                MaxSdk.SetVerboseLogging(true);
            }

            MaxSdk.InitializeSdk();
#if UNITY_EDITOR
            MaxSdk.DisableStubAds();
#endif
            MaxSdkCallbacks.AppOpen.OnAdHiddenEvent += OnAppOpenDismissedEvent;
            MaxSdkCallbacks.AppOpen.OnAdLoadFailedEvent += OnAppOpenLoadFailedEvent;
            MaxSdkCallbacks.AppOpen.OnAdLoadedEvent += OnAppOpenLoadEvent;
            MaxSdkCallbacks.AppOpen.OnAdDisplayedEvent += OnAppOpenDisplayEvent;
            MaxSdkCallbacks.AppOpen.OnAdDisplayFailedEvent += OnAppOpenDisplayFailedEvent;

            MaxSdkCallbacks.AppOpen.OnAdRevenuePaidEvent += (x, y) => OnAdRevenuePaidEvent(x, y, AdsType.AppOpen);
#endif
        }

        private void LoadAdsInGame()
        {
            if (isInit) return;
            isInit = true;

            InitializeBannerAds();
            LoadInterstitial();
            LoadRewardedAd();
            InitializeMRecAds();
#if BB_APPLOVIN_MAX
            MaxSdkCallbacks.Banner.OnAdLoadedEvent += OnBannerLoadedEvent;
            MaxSdkCallbacks.Banner.OnAdLoadFailedEvent += OnBannerLoadFailedEvent;

            MaxSdkCallbacks.MRec.OnAdLoadedEvent += OnMRecAdLoadedEvent;
            MaxSdkCallbacks.MRec.OnAdLoadFailedEvent += OnMRecAdFailedEvent;
            MaxSdkCallbacks.MRec.OnAdClickedEvent += OnMRecAdClickedEvent;


            MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnInterstitialLoadedEvent;
            MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += OnInterstitialFailedEvent;
            MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += InterstitialFailedToDisplayEvent;
            MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent += OnInterstitialDisplayEvent;
            MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += OnInterstitialDismissedEvent;
            MaxSdkCallbacks.Interstitial.OnAdClickedEvent += OnInterstitialClickedEvent;

            MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnRewardedAdLoadedEvent;
            MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnRewardedLoadFailedEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += OnRewardedAdFailedToDisplayEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += OnRewardDisplayEvent;
            MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += OnRewardedAdDismissedEvent;
            MaxSdkCallbacks.Rewarded.OnAdClickedEvent += OnRewardedAdClickedEvent;
            MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += OnRewardedAdReceivedRewardEvent;


            MaxSdkCallbacks.MRec.OnAdRevenuePaidEvent += (x, y) => OnAdRevenuePaidEvent(x, y, AdsType.MREC);
            MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += (x, y) => OnAdRevenuePaidEvent(x, y, AdsType.Banner);
            MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += (x, y) => OnAdRevenuePaidEvent(x, y, AdsType.Rewarded);
            MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent +=
                (x, y) => OnAdRevenuePaidEvent(x, y, AdsType.Interstitial);
#endif
        }


#if BB_APPLOVIN_MAX
        private void OnAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo impressionData, AdsType adsType)
        {
            double revenue = impressionData.Revenue;
            this.adManger.OnAdsRevPaid(impressionData, adUnitId, adsType);
#if NB_FIREBASE_ANALYTIC
            var impressionParameters = new[]
            {
                new Firebase.Analytics.Parameter("ad_platform", "AppLovin"),
                new Firebase.Analytics.Parameter("ad_source", impressionData.NetworkName),
                new Firebase.Analytics.Parameter("ad_unit_name", impressionData.AdUnitIdentifier),
                new Firebase.Analytics.Parameter("ad_format", impressionData.AdFormat),
                new Firebase.Analytics.Parameter("ad_creative", impressionData.CreativeIdentifier),
                new Firebase.Analytics.Parameter("country", MaxSdk.GetSdkConfiguration().CountryCode),
                new Firebase.Analytics.Parameter("value", revenue),
                new Firebase.Analytics.Parameter("currency", "USD"), // All AppLovin revenue is sent in USD
            };
            TrackingManager.TrackEvent("custom_ad_impression", impressionParameters);
//            Firebase.Analytics.FirebaseAnalytics.LogEvent("custom_ad_impression", impressionParameters);
            //      Firebase.Analytics.FirebaseAnalytics.LogEvent("ad_impression",impressionParameters);
#endif
#if NB_BYTEBREW_ANALYTIC
    if (ByteBrew.Instance != null)
        {
            string networkName = impressionData.NetworkName.Replace(" ","_");
            ByteBrew.TrackAdEvent(impressionData.AdFormat,"in_game",adUnitId,networkName);
        }
#endif
#if NB_SINGULAR
            SingularAdData data = new SingularAdData("AppLovin", "USD", revenue);

            data.WithAdUnitId(impressionData.AdUnitIdentifier).WithNetworkName(impressionData.NetworkName)
                .WithPlacementId(impressionData.Placement);
            SingularSDK.AdRevenue(data);
#endif
#if NB_APPFLYER
            AppsFlyer.sendEvent("af_ad_revenue", new Dictionary<string, string>()
            {
                ["ad_platform"] = "AppLovin",
                ["ad_source"] = impressionData.NetworkName,
                ["ad_unit_name"] = impressionData.AdUnitIdentifier,
                ["ad_format"] = impressionData.AdFormat,
                ["value"] = revenue.ToString("0.0000"),
                ["currency"] = revenue.ToString("USD"),
            });

            Dictionary<string, string> additionalParams = new Dictionary<string, string>();
            additionalParams.Add(AFAdRevenueEvent.COUNTRY, MaxSdk.GetSdkConfiguration().CountryCode);
            additionalParams.Add(AFAdRevenueEvent.AD_UNIT, impressionData.AdUnitIdentifier);
            additionalParams.Add(AFAdRevenueEvent.AD_TYPE, impressionData.AdFormat);
            additionalParams.Add(AFAdRevenueEvent.PLACEMENT, impressionData.Placement);

            AppsFlyerAdRevenue.logAdRevenue("Applovin",
                AppsFlyerAdRevenueMediationNetworkType.AppsFlyerAdRevenueMediationNetworkTypeApplovinMax,
                revenue,
                "USD",
                additionalParams);
#endif
        }
#endif
        public void CheckLoadAds()
        {
#if BB_APPLOVIN_MAX
            if (Application.internetReachability != NetworkReachability.NotReachable)
            {
                if (!MaxSdk.IsInterstitialReady(adsUnitIds[AdsType.Interstitial]))
                {
                    LoadInterstitial();
                }

                if (!MaxSdk.IsRewardedAdReady(adsUnitIds[AdsType.Rewarded]))
                {
                    LoadRewardedAd();
                }

                if (adManger.adsConfig.shouldShowAppOpen && !MaxSdk.IsAppOpenAdReady(adsUnitIds[AdsType.AppOpen]))
                {
                    LoadAppOpenAd();
                }
            }
#endif
        }
        #region Interstitial
        private void LoadInterstitial()
        {
#if BB_APPLOVIN_MAX
            if (adManger.adsConfig.shouldShowInter || adManger.adsConfig.isShowInterWhenNoReward)
            {
                Debug.Log("Applovin Load Inter");
                MaxSdk.LoadInterstitial(adsUnitIds[AdsType.Interstitial]);
                TrackingManager.TrackEvent("AdsInter_CallLoad");
            }
#endif
        }
        public bool IsInterstitalAdsLoaded()
        {
#if BB_APPLOVIN_MAX
            return MaxSdk.IsInterstitialReady(adsUnitIds[AdsType.Interstitial]);
#endif
            return false;
        }

        public bool ShowInterAds(Action _onInterClosed, string _placement)
        {
            bool isshow = false;
#if BB_APPLOVIN_MAX
            Debug.Log("Show Inter");
            if (MaxSdk.IsInterstitialReady(adsUnitIds[AdsType.Interstitial]))
            {
                MaxSdk.ShowInterstitial(adsUnitIds[AdsType.Interstitial], _placement);
                adManger.OnInterStarted();
                onInterAdsClosed = _onInterClosed;
                isshow = true;
                TrackingManager.TrackEvent("AdsInter_CallShow", place, _placement);
            }
            TrackingManager.TrackEvent("AdsInter_PassedLogic", place, _placement);
#endif
            return isshow;
        }

#if BB_APPLOVIN_MAX
        private void OnInterstitialLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            TrackingManager.TrackEvent("AdsInter_LoadDone");
        }

        private void OnInterstitialFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            Debug.LogWarning($"OnInterstitial Failed {adUnitId} : {errorInfo.Message}");

            string networkName = errorInfo.MediatedNetworkErrorMessage.Replace(" ", "_");
        }

        private void InterstitialFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo,
            MaxSdkBase.AdInfo arg3)
        {
            Debug.LogWarning("OnInterstitial Failed To Displayed");
            onInterAdsClosed?.Invoke();
            adManger.OnInterClosed();
            string networkName = errorInfo.MediatedNetworkErrorMessage.Replace(" ", "_");
            TrackingManager.TrackEvent("AdsInter_FailedToDisplay", place,"none");
        }

        private void OnInterstitialDisplayEvent(string arg1, MaxSdkBase.AdInfo arg2)
        {
#if NB_APPFLYER
            AppsFlyer.sendEvent("event_interstitial_ad_impression", new Dictionary<string, string>() { { "event_interstitial_ad_impression", "event_interstitial_ad_impression" } });
#endif
            TrackingManager.TrackEvent("AdsInter_Displayed", place, "none");
        }

        private void OnInterstitialDismissedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            // Interstitial ad is hidden. Pre-load the next ad
            Debug.Log("Applovin Inter Closed");
            onInterAdsClosed?.Invoke();
            adManger.OnInterClosed();
            adManger.ShowOpenWhhenShowInter();
            TrackingManager.TrackEvent("AdsInter_Closed", place, "none");
        }

        private void OnInterstitialClickedEvent(string adUnitId, MaxSdkBase.AdInfo arg2)
        {
#if NB_APPFLYER
                        AppsFlyer.sendEvent("event_interstitial_ad_clicked", new Dictionary<string, string>() { { "interstitial_ad_clicked", "interstitial_ad_clicked" } });

#endif
        }
#endif

        #endregion Interstitial

        #region Banner

        public void InitializeBannerAds()
        {
            if (!adManger.adsConfig.shouldShowBanner) return;
#if BB_APPLOVIN_MAX
            // Banners are automatically sized to 320x50 on phones and 728x90 on tablets
            // You may use the utility method `MaxSdkUtils.isTablet()` to help with view sizing adjustments

            var bannerPos = ExtensionApplovinModule.GetBannerPosition(adManger.adsConfig.bannerPos);
            MaxSdk.CreateBanner(adsUnitIds[AdsType.Banner], bannerPos);

            string auto_size = adManger.adsConfig.auto_size_banner ? "true" : "false";
            MaxSdk.SetBannerExtraParameter(adsUnitIds[AdsType.Banner], "adaptive_banner", auto_size);
            // Set background or background color for banners to be fully functional
            MaxSdk.SetBannerBackgroundColor(adsUnitIds[AdsType.Banner], adManger.adsConfig.bannerBackgroundColor);

            TrackingManager.TrackEvent($"AdsBanner_CallLoad");
#endif
        }


        public void UpdatePositionBanner()
        {
#if BB_APPLOVIN_MAX
            // var bannerPos = ExtensionApplovinModule.GetBannerPosition(adManger.adsConfig.bannerPos);
            // MaxSdk.UpdateBannerPosition(adsUnitIds[AdsType.Banner], bannerPos);

            var bannerPos = ExtensionApplovinModule.GetBannerPosition(adManger.adsConfig.bannerPos);
            MaxSdk.UpdateBannerPosition(adsUnitIds[AdsType.Banner], bannerPos);
#endif
        }

        public void UpdateAdaptiveBanner()
        {
#if BB_APPLOVIN_MAX
            string auto_size = adManger.adsConfig.auto_size_banner ? "true" : "false";
            MaxSdk.SetBannerExtraParameter(adsUnitIds[AdsType.Banner], "adaptive_banner", auto_size);
#endif
        }

        public void ShowBanner()
        {
#if BB_APPLOVIN_MAX
            MaxSdk.ShowBanner(adsUnitIds[AdsType.Banner]);
#endif
        }

        public void HideBanner()
        {
#if BB_APPLOVIN_MAX
            MaxSdk.HideBanner(adsUnitIds[AdsType.Banner]);
#endif
        }
#if BB_APPLOVIN_MAX
        private void OnBannerLoadFailedEvent(string arg1, MaxSdkBase.ErrorInfo arg2)
        {
            TrackingManager.TrackEvent($"AdsBanner_LoadFailed");
        }

        private void OnBannerLoadedEvent(string arg1, MaxSdkBase.AdInfo arg2)
        {
            Debug.Log("Banner Loaded");
            Rect bannerLayout = MaxSdk.GetBannerLayout(adsUnitIds[AdsType.Banner]);
            Debug.Log($"w:{bannerLayout.width},h:{bannerLayout.height},pos:{bannerLayout.position}");
            TrackingManager.TrackEvent($"AdsBanner_LoadDone");
        }
#endif

        #endregion Banner

        #region Reward

        private void LoadRewardedAd()
        {
#if BB_APPLOVIN_MAX
            MaxSdk.LoadRewardedAd(adsUnitIds[AdsType.Rewarded]);
            TrackingManager.TrackEvent("AdsReward_CallLoad");
#endif
        }

        public bool IsRewardAdsLoaded()
        {
#if BB_APPLOVIN_MAX
            return MaxSdk.IsRewardedAdReady(adsUnitIds[AdsType.Rewarded]);
#endif
            return false;
        }

        public bool ShowRewardAds(Action _onRewardClose, Action _onRewardAdSkip, string _placement)
        {
            bool isshow = false;
#if BB_APPLOVIN_MAX
            Debug.Log("Show Reward");
            if (MaxSdk.IsRewardedAdReady(adsUnitIds[AdsType.Rewarded]))
            {
                IsRewarded = false;
                adManger.OnRewardedStarted();
                MaxSdk.ShowRewardedAd(adsUnitIds[AdsType.Rewarded], _placement);
                onRewardAdsClosed = _onRewardClose;
                onRewardAdsSkip = _onRewardAdSkip;
                isshow = true;
                TrackingManager.TrackEvent("AdsReward_CallShow", place, _placement);
            }
            else
            {
                
            }

            TrackingManager.TrackEvent("AdsReward_PassedLogic", place, _placement);

#endif
            return isshow;
        }

#if BB_APPLOVIN_MAX
        private void OnRewardedAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            Debug.Log("Load done reward");
            TrackingManager.TrackEvent("AdsReward_LoadDone");
        }

        private void OnRewardedLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            Debug.LogWarning("OnRewardedAdFailedEvent");
            string networkName = errorInfo.MediatedNetworkErrorMessage.Replace(" ", "_");
        }

        private void OnRewardDisplayEvent(string arg1, MaxSdkBase.AdInfo arg2)
        {
            TrackingManager.TrackEvent("AdsReward_Displayed", place, "none");
        }

        private void OnRewardedAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo,
            MaxSdkBase.AdInfo arg3)
        {
            Debug.LogWarning("OnRewardedAd Failed To Display");

            // Rewarded ad failed to display. We recommend loading the next ad
            onRewardAdsClosed?.Invoke();
            adManger.OnRewardedClosed();
            string networkName = errorInfo.MediatedNetworkErrorMessage.Replace(" ", "_");

            TrackingManager.TrackEvent("AdsReward_FailedToDisplay", place, "none");
        }

        private void OnRewardedAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo arg2)
        {
#if NB_APPFLYER
                        AppsFlyerSDK.AppsFlyer.sendEvent("event_video_reward_clicked", new Dictionary<string, string>() { { "clicked_video", "clicked_video" } });
#endif
        }

        private void OnRewardedAdDismissedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            // Rewarded ad is hidden. Pre-load the next ad
            Debug.Log("Applovin reward Closed");
            if (IsRewarded)
            {
                adManger.ResetAdsParameter();
                onRewardAdsClosed?.Invoke();
                TrackingManager.TrackEvent("AdsReward_ReceivedReward", place, "none");
            }
            else
            {
                onRewardAdsSkip?.Invoke();
                TrackingManager.TrackEvent("AdsReward_Skip", place, "none");
            }

            adManger.OnRewardedClosed();
            TrackingManager.TrackEvent("AdsReward_Closed", place, "none");
        }

        private void OnRewardedAdReceivedRewardEvent(string adUnitId, MaxSdk.Reward reward, MaxSdkBase.AdInfo adInfo)
        {
            IsRewarded = true;
        }
#endif

        #endregion

        #region App Open

        private void LoadAppOpenAd()
        {
#if BB_APPLOVIN_MAX
            if (adManger.adsConfig.shouldShowAppOpen)
            {
                MaxSdk.LoadAppOpenAd(adsUnitIds[AdsType.AppOpen]);
                TrackingManager.TrackEvent("AdsAOA_CallLoad");
            }
#endif
        }

        public bool IsAppOpenAdsLoaded()
        {
#if BB_APPLOVIN_MAX
            return MaxSdk.IsAppOpenAdReady(adsUnitIds[AdsType.AppOpen]);
            TrackingManager.TrackEvent("AdsAOA_CallLoad");
#endif
            return false;
        }


        public bool ShowAppOpenAds(Action _onAppOpenClose, string _placement = "none")
        {
            bool isshow = false;
#if BB_APPLOVIN_MAX
            Debug.Log("Show App Open");
            if (!adManger.adsConfig.shouldShowAppOpen) return false;
            if (MaxSdk.IsAppOpenAdReady(adsUnitIds[AdsType.AppOpen]))
            {
                IsRewarded = false;
                adManger.OnAppOpenStarted();
                MaxSdk.ShowAppOpenAd(adsUnitIds[AdsType.AppOpen], _placement);
                isshow = true;
                TrackingManager.TrackEvent("AdsAOA_CallShow", place, _placement);
            }

            TrackingManager.TrackEvent("AdsAOA_PassedLogic", place, _placement);
#endif
            return isshow;
        }
#if BB_APPLOVIN_MAX
        private void OnAppOpenDismissedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            Debug.Log("Applovin Open App Ads Closed");
            onAppOpenAdsClosed?.Invoke();
            adManger.OnAppOpenClose();
            TrackingManager.TrackEvent("AdsAOA_Closed", place, "none");
        }

        private void OnAppOpenLoadEvent(string arg1, MaxSdkBase.AdInfo arg2)
        {
            LoadAdsInGame();
            TrackingManager.TrackEvent("AdsAOA_LoadDone");
        }

        private void OnAppOpenLoadFailedEvent(string arg1, MaxSdkBase.ErrorInfo arg2)
        {
            LoadAdsInGame();
            TrackingManager.TrackEvent("AdsAOA_LoadFailded");
        }
        private void OnAppOpenDisplayFailedEvent(string arg1, MaxSdkBase.ErrorInfo arg2, MaxSdkBase.AdInfo arg3)
        {
            string networkName = arg2.MediatedNetworkErrorMessage.Replace(" ", "_");
            TrackingManager.TrackEvent("AdsAOA_FailedToDisplay", place, "none");
        }
        private void OnAppOpenDisplayEvent(string arg1, MaxSdkBase.AdInfo arg2)
        {
            TrackingManager.TrackEvent("AdsAOA_Displayed", place, "none");
        }
#endif
        #endregion
        #region MREC

        private void InitializeMRecAds()
        {
#if BB_APPLOVIN_MAX
            if (!adManger.adsConfig.shouldShowMREC) return;

            if (adsUnitIds[AdsType.MREC] == String.Empty) return;

            // MaxSdkBase.AdViewPosition posMREC = ExtensionApplovinModule.GetAdViewPosition(adManger.adsConfig.mrecPos);
            // if (adManger.adsConfig.mrecPos != BannerPos.CustomPos)
            // {
            //     MaxSdk.CreateMRec(adsUnitIds[AdsType.MREC], posMREC);
            // }
            // else
            // {
            // float x = adManger.adsConfig.posMREC.x;
            // float y = adManger.adsConfig.posMREC.y;
            var mrec_px =
                ExtensionApplovinModule.GetPositionBannerExtra(adManger.adsConfig.mrecExtraPos, new Vector2(300, 250));
            MaxSdk.CreateMRec(adsUnitIds[AdsType.MREC], mrec_px.Item1, mrec_px.Item2);
            // }
#endif
        }

        public void ShowMREC()
        {
#if BB_APPLOVIN_MAX
            if (!adManger.adsConfig.shouldShowMREC) return;
            TrackingManager.TrackEvent($"AdsMREC_CallShow");
            MaxSdk.ShowMRec(adsUnitIds[AdsType.MREC]);
#endif
        }

        public void HideMREC()
        {
#if BB_APPLOVIN_MAX
            if (!adManger.adsConfig.shouldShowMREC) return;
            MaxSdk.HideMRec(adsUnitIds[AdsType.MREC]);
#endif
        }
#if BB_APPLOVIN_MAX
        private void OnMRecAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            TrackingManager.TrackEvent($"AdsMREC_Loaded");
        }

        private void OnMRecAdFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            TrackingManager.TrackEvent($"AdsMREC_Failed");
        }

        private void OnMRecAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
        }
#endif

        #endregion
    }

    #region Extention Position

#if BB_APPLOVIN_MAX
    public static class ExtensionApplovinModule
    {
        public static MaxSdkBase.AdViewPosition GetAdViewPosition(BannerPos pos)
        {
            var posMREC = MaxSdkBase.AdViewPosition.Centered;
            switch (pos)
            {
                case BannerPos.TopCenter:
                    posMREC = MaxSdkBase.AdViewPosition.TopCenter;
                    break;
                case BannerPos.BottomCenter:
                    posMREC = MaxSdkBase.AdViewPosition.BottomCenter;
                    break;
                case BannerPos.TopLeft:
                    posMREC = MaxSdkBase.AdViewPosition.TopLeft;
                    break;
                case BannerPos.TopRight:
                    posMREC = MaxSdkBase.AdViewPosition.TopRight;
                    break;
                case BannerPos.Centered:
                    posMREC = MaxSdkBase.AdViewPosition.Centered;
                    break;
                case BannerPos.CenterLeft:
                    posMREC = MaxSdkBase.AdViewPosition.CenterLeft;
                    break;
                case BannerPos.CenterRight:
                    posMREC = MaxSdkBase.AdViewPosition.CenterRight;
                    break;
                case BannerPos.BottomLeft:
                    posMREC = MaxSdkBase.AdViewPosition.BottomLeft;
                    break;
                case BannerPos.BottomRight:
                    posMREC = MaxSdkBase.AdViewPosition.BottomRight;
                    break;
            }

            return posMREC;
        }

        public static MaxSdkBase.BannerPosition GetBannerPosition(BannerPos pos)
        {
            var bannerPos = MaxSdkBase.BannerPosition.BottomCenter;
            switch (pos)
            {
                case BannerPos.TopCenter:
                    bannerPos = MaxSdkBase.BannerPosition.TopCenter;
                    break;
                case BannerPos.BottomCenter:
                    bannerPos = MaxSdkBase.BannerPosition.BottomCenter;
                    break;
                case BannerPos.TopLeft:
                    bannerPos = MaxSdkBase.BannerPosition.TopLeft;
                    break;
                case BannerPos.TopRight:
                    bannerPos = MaxSdkBase.BannerPosition.TopRight;
                    break;
                case BannerPos.Centered:
                    bannerPos = MaxSdkBase.BannerPosition.Centered;
                    break;
                case BannerPos.CenterLeft:
                    bannerPos = MaxSdkBase.BannerPosition.CenterLeft;
                    break;
                case BannerPos.CenterRight:
                    bannerPos = MaxSdkBase.BannerPosition.CenterRight;
                    break;
                case BannerPos.BottomLeft:
                    bannerPos = MaxSdkBase.BannerPosition.BottomLeft;
                    break;
                case BannerPos.BottomRight:
                    bannerPos = MaxSdkBase.BannerPosition.BottomRight;
                    break;
            }

            return bannerPos;
        }

        public static (float, float) GetPositionBannerExtra(BannerExtraPos bannerExtraPos, Vector2 size)
        {
            // Lấy kích thước màn hình
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;

            // Lấy density (mật độ điểm ảnh)
            float density = MaxSdkUtils.GetScreenDensity();

            // Chuyển đổi từ dp sang pixel
            // float mrecWidth_px = 300 * density;
            // float mrecHeight_px = 250 * density;

            float bannerWith_px = size.x * density;
            float bannerHeight_px = size.y * density;

            float centerX_px = 0, centerY_px = 0, pX = 0, pY = 0;

            switch (bannerExtraPos)
            {
                case BannerExtraPos.TopCenter:
                    centerX_px = screenWidth / 2;

                    pX = centerX_px - (bannerWith_px / 2);
                    break;
                case BannerExtraPos.BottomCenter:
                    centerX_px = screenWidth / 2;

                    pX = centerX_px - (bannerWith_px / 2);
                    pY = screenHeight - bannerHeight_px;
                    break;
                case BannerExtraPos.CenterLeft:
                    centerY_px = screenHeight / 2;
                    pY = centerY_px - (bannerHeight_px / 2);
                    break;
                case BannerExtraPos.CenterRigh:
                    centerY_px = screenHeight / 2;
                    pX = screenWidth - bannerWith_px;
                    pY = centerY_px - (bannerHeight_px / 2);
                    break;
                case BannerExtraPos.Centered:
                    centerX_px = screenWidth / 2;
                    centerY_px = screenHeight / 2;
                    pX = centerX_px - (bannerWith_px / 2);
                    pY = centerY_px - (bannerHeight_px / 2);
                    break;
                case BannerExtraPos.TopCenterCenter:
                    centerX_px = screenWidth / 2;
                    pX = centerX_px - (bannerWith_px / 2);
                    pY = screenHeight / 4 - (bannerHeight_px / 2);
                    break;
                case BannerExtraPos.BottomCenterCenter:
                    centerX_px = screenWidth / 2;
                    pX = centerX_px - (bannerWith_px / 2);
                    pY = screenHeight * 3 / 4 - (bannerHeight_px / 2);
                    break;
                case BannerExtraPos.CenterCenterLeft:
                    centerY_px = screenHeight / 2;
                    pX = screenWidth / 4 - bannerWith_px / 2;
                    pY = centerY_px - (bannerHeight_px / 2);
                    break;
                case BannerExtraPos.CenterCenterRigh:
                    centerY_px = screenHeight / 2;
                    pX = screenWidth * 3 / 4 - bannerWith_px / 2;
                    pY = centerY_px - (bannerHeight_px / 2);
                    break;
                case BannerExtraPos.TopLeft:
                    pX = 0;
                    pY = 0;
                    break;
                case BannerExtraPos.TopRight:
                    pX = screenWidth - bannerWith_px;
                    pY = 0;
                    break;
                case BannerExtraPos.BottomLeft:
                    pX = 0;
                    pY = screenHeight - bannerHeight_px;
                    break;
                case BannerExtraPos.BottomRight:
                    pY = screenHeight - bannerHeight_px;
                    pX = screenWidth - bannerWith_px;
                    break;
            }

            // mrecX_px += rateX;
            // mrecY_px += rateY;

            // Chuyển đổi tọa độ gốc từ pixel sang dp
            float mrecX_dp = pX / density;
            float mrecY_dp = pY / density;

            return (mrecX_dp, mrecY_dp);
        }
    }
#endif

    public enum BannerPos
    {
        TopCenter = 0,
        BottomCenter = 1,
        TopLeft = 2,
        TopRight = 3,
        Centered = 4,
        CenterLeft = 5,
        CenterRight = 6,
        BottomLeft = 7,
        BottomRight = 8,
        CustomPos = 9
    }

    public enum BannerExtraPos
    {
        TopCenter = 0,
        BottomCenter = 1,
        CenterLeft = 2,
        CenterRigh = 3,
        Centered = 4,
        TopCenterCenter = 5,
        BottomCenterCenter = 6,
        CenterCenterLeft = 7,
        CenterCenterRigh = 8,

        TopLeft = 9,
        TopRight = 10,
        BottomLeft = 11,
        BottomRight = 12,
    }

    #endregion
}