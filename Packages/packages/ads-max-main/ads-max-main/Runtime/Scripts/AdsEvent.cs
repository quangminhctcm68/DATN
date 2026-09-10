using NabaGame.Core.Runtime.EventManager;
using UnityEngine;

namespace BMH.Ads
{
    public class AdsRevPaidEvent : GameEvent
    {
#if BMH_APPLOVIN_MAX
        public MaxSdk.AdInfo info;
        public AdsType adsType;
        public string adUnitId;
#endif
    }
}