using System;
using UnityEngine;
using UnityEngine.Serialization;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace BMH.Ads
{
    [Serializable]
    public class AdsNetworkConfig
    {
        public AdsModuleType adsModule;
        public string[] extras;
    }

    [Serializable]
    public class AdsConfig
    {
        public bool isTestAds; // có dùng test ads không
        public float checkLoadAdsInterval; // thời gian interval kiểm tra xem có ađs chưa, nếu chưa có thì sẽ load ads 

        //------------------Banner

        public MaxSdkBase.BannerPosition bannerPosition;

        public bool shouldShowBanner; // có show banner không
        public bool isAdaptiveBanner;
        public Color bannerBackgroundColor = new Color(1, 1, 1, 0);
        //-----------------------

        public bool shouldShowAppOpen; // có show App Open Ads không
        public float appOpenAdsInterval;
        public bool shouldShowMREC;
        public MRECPos mrecPosition;
        public bool shouldShowInter; // có show interstital không
        public float interAdsIntervalTime; // thời gian interval hiển thị giữa 2 interstitial
        public float interAfterRewardTime; // thời gian hiện iterstitial sau khi hiện rewarded
        public float rewardAdsIntervalTime = 10;

        public bool
            isShowInterWhenNoReward; // có dùng interstitial thay cho rewarded khi không load được reward hay không
    }
}