using NabaGame.Core.Runtime.EventManager;

namespace NabaGame.Ads
{
    public class AdsRevPaidEvent : GameEvent
    {
#if BB_APPLOVIN_MAX
        public MaxSdk.AdInfo info;
        public AdsType adsType;
        public string adUnitId;
#endif
    }
}