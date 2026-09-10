using System;
using System.Collections.Generic;

namespace BMH.Ads
{
    public interface IAdsModule
    {
        void Init(AdManager _adManger, Dictionary<AdsType, string> androidUnitIds,
            Dictionary<AdsType, string> iosUnitIds, params string[] others);

        void Update();
        void UpdateBannerPosition();
        void UpdateMRECPosition();
        bool IsLoaded(AdsType adsType);
        void LoadAd(AdsType adsType);
      
        void ShowAds(AdsType adsType, Action _onCallBack, string placement);
        void ShowReplace(AdsType adsType, Action _onCallBack, string placement);
        void Hide(AdsType adsType);

    }
}