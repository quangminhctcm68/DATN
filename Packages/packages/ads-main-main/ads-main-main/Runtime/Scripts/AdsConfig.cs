using System;
using UnityEngine;
using UnityEngine.Serialization;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;

#endif

namespace NabaGame.Ads
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
        public bool shouldShowBanner; // có show banner không

        public BannerPos bannerPos; // vị trí của banner
        public bool auto_size_banner;
        public Color bannerBackgroundColor;
        //-----------------------

//----------------------MREC        
        public bool shouldShowMREC;
        public BannerExtraPos mrecExtraPos;
//----------------------------

        public bool shouldShowAppOpen; // có show App Open Ads không
#if ODIN_INSPECTOR
        [ShowIf("shouldShowAppOpen")]
#endif
        public float appOpenAdsInterval;

        public bool shouldShowInter; // có show interstital không
#if ODIN_INSPECTOR
        [ShowIf("shouldShowInter")]
#endif
        public float interAdsIntervalTime; // thời gian interval hiển thị giữa 2 interstitial
#if ODIN_INSPECTOR
        [ShowIf("shouldShowInter")]
#endif
        public float interAfterRewardTime; // thời gian hiện iterstitial sau khi hiện rewarded
#if ODIN_INSPECTOR
        [ShowIf("shouldShowInter")]
#endif
        public bool
            isShowInterWhenNoReward; // có dùng interstitial thay cho rewarded khi không load được reward hay không
#if ODIN_INSPECTOR
        [ShowIf("shouldShowInter")]
#endif
        public bool isShowOpenWhenShowInter;
    }
}