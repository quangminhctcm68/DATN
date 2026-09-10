using System;
using System.Collections.Generic;

namespace NabaGame.Ads
{
    public interface IAdsModule
    {
        void Init(AdManager _adManger, Dictionary<AdsType, string> androidUnitIds,
            Dictionary<AdsType, string> iosUnitIds, params string[] others);

        bool IsInterstitalAdsLoaded();
        bool ShowInterAds(Action _onInterClosed, string placement);
        void InitializeBannerAds();
        void ShowBanner();
        void HideBanner();
        void UpdatePositionBanner();
        void UpdateAdaptiveBanner();

        bool IsRewardAdsLoaded();

        bool ShowRewardAds(Action _onRewardClose, Action _onRewardAdSkip, string placement);
        void CheckLoadAds();

        bool IsAppOpenAdsLoaded();
        bool ShowAppOpenAds(Action _onAppOpenClose, string placement);

        void ShowMREC();
        void HideMREC();
    }
}