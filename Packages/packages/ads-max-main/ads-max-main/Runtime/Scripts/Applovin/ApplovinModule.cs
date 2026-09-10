using System;
using UnityEngine;
using System.Collections.Generic;
#if BMH_FIREBASE_ANALYTIC
using NabaGame.Tracking;
#endif

namespace BMH.Ads
{
    public class ApplovinModule : IAdsModule
    {
        private AdManager adManger;
        private Dictionary<AdsType, string> adsUnitIds;
        private Dictionary<AdsType, IAdsView> adsViews;

        public void Init(AdManager _adManger, Dictionary<AdsType, string> androidUnitIds,
            Dictionary<AdsType, string> iosUnitIds, params string[] others)
        {
            adManger = _adManger;
            adsViews = new Dictionary<AdsType, IAdsView>();
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                adsUnitIds = iosUnitIds;
            }
            else
            {
                adsUnitIds = androidUnitIds;
            }
#if BMH_APPLOVIN_MAX
            string sdkKey = others[0];
            MaxSdkCallbacks.OnSdkInitializedEvent += (MaxSdkBase.SdkConfiguration sdkConfiguration) =>
            {
                if (adManger.adsConfig.isTestAds)
                {
                    MaxSdk.ShowMediationDebugger();
                }

                LoadAd(AdsType.AppOpen);
                // LoadAd(AdsType.Banner);
            };
            MaxSdk.SetSdkKey(sdkKey);
            MaxSdk.InitializeSdk();
#endif
        }

        private IAdsView GetAdView(AdsType adsType)
        {
            if (adsViews.TryGetValue(adsType, out var adView))
                return adView;
            return null;
        }

        public void Update()
        {
            if (adsViews.ContainsKey(AdsType.AppOpen))
            {
                adsViews[AdsType.AppOpen].CheckLoad();
            }

            if (adsViews.ContainsKey(AdsType.Interstitial))
            {
                adsViews[AdsType.Interstitial].CheckLoad();
            }

            if (adsViews.ContainsKey(AdsType.Rewarded))
            {
                adsViews[AdsType.Rewarded].CheckLoad();
            }
        }

        public void UpdateBannerPosition()
        {
            if (adsViews.ContainsKey(AdsType.Banner))
            {
                (adsViews[AdsType.Banner] as BannerView).UpdatePosition();
            }
        }

        public void UpdateMRECPosition()
        {
            if (adsViews.ContainsKey(AdsType.MREC))
            {
                (adsViews[AdsType.MREC] as MRECView).UpdatePosition();
            }
        }

        public void LoadAd(AdsType adsType)
        {
            var adView = GetAdView(adsType);
            if (adView != null)
            {
                if (!adView.IsAdsLoaded())
                {
                    adView.Load();
                    Debug.Log("AdView Null: Start Load Asd +" + adsType);
                }
                else
                {
                    Debug.Log("AdView Has+" + adsType);
                }

                return;
            }

            switch (adsType)
            {
                case AdsType.Banner:
                    var banner = new BannerView(adManger, adsUnitIds[AdsType.Banner]);
                    adsViews.Add(adsType, banner);
                    break;
                case AdsType.Interstitial:
                    var inter = new InterstitialView(adManger, adsUnitIds[AdsType.Interstitial]);
                    adsViews.Add(adsType, inter);
                    break;
                case AdsType.Rewarded:
                    var reward = new RewardView(adManger, adsUnitIds[AdsType.Rewarded]);
                    adsViews.Add(adsType, reward);
                    break;
                case AdsType.AppOpen:
                    var aoa = new AOAView(adManger, adsUnitIds[AdsType.AppOpen]);
                    adsViews.Add(adsType, aoa);
                    break;
                case AdsType.MREC:
                    var mrec = new MRECView(adManger, adsUnitIds[AdsType.MREC]);
                    adsViews.Add(adsType, mrec);
                    break;
            }
        }

        public bool IsLoaded(AdsType adsType)
        {
            var adView = GetAdView(adsType);
            return adView != null && adView.IsAdsLoaded();
        }

        public void ShowAds(AdsType adsType, Action _onCallBack, string placement)
        {
            var adView = GetAdView(adsType);
            if (adView != null)
            {
                adView.Show(_onCallBack, placement);
            }
        }

        public void ShowReplace(AdsType adsType, Action _onCallBack, string placement)
        {
            var adView = GetAdView(adsType);
            if (adView != null)
            {
                adView.ShowReplace(_onCallBack, placement);
            }
        }

        public void Hide(AdsType adsType)
        {
            // if (adsType != AdsType.Banner || adsType != AdsType.MREC) return;
            var adView = GetAdView(adsType);
            if (adView != null)
            {
                adView.Hide();
            }
        }
    }
}