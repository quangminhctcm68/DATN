using System;
using System.Collections;
using System.Collections.Generic;
using BMH.Ads;
using NabaGame.Core.Runtime.EventManager;
using NabaGame.Core.Runtime.Singleton;
using Sirenix.OdinInspector;
using UnityEngine;
#if BMH_SINGULAR
using Singular;
#endif

#if BMH_SOLAR
using SolarEngine;
#endif

[Singleton("MMPTrackingManager", true)]
public class MMPTrackingManager : Singleton<MMPTrackingManager>
{
#if BMH_SINGULAR
    public string txSingular = "Has Singular";
#endif

#if BMH_SOLAR
    public string txSolar = "Has Solar";
#endif
    public override void Init()
    {
    }
    private void Start()
    {
        EventManager.Instance.AddListener<IAPPurchaseSuccessEvent>(IAPPurchaseSuccessEvent);
        EventManager.Instance.AddListener<AdsRevPaidEvent>(AdsRevPaidEvent);
    }

    #region IAP Processing

    private void IAPPurchaseSuccessEvent(IAPPurchaseSuccessEvent e)
    {
        SingularIAP(e.amout);
        SolarTracking(e.amout);

        void SingularIAP(float amout)
        {
#if BMH_SINGULAR
            SingularSDK.Revenue("USD",amout);
#endif
        }
        void SolarTracking(float amout)
        {
#if BMH_SOLAR
#endif
        }
    }

    #endregion

    #region Ad Revenue Paid

    private void AdsRevPaidEvent(AdsRevPaidEvent e)
    {
        Singular(e);
        Solar(e);

        void Singular(AdsRevPaidEvent e)
        {
#if BMH_SINGULAR
  SingularAdData data = new SingularAdData("AppLovin", "USD", e.info.Revenue);
        // data.WithAdUnitId(e.adUnitId).WithNetworkName("AppLovin")
        //     .WithPlacementId(e.adsType.ToString());
        SingularSDK.AdRevenue(data);
#endif
        }

        void Solar(AdsRevPaidEvent e)
        {
#if BMH_SOLAR
  ImpressionAttributes impressionAttributes = new ImpressionAttributes();
        impressionAttributes.ad_platform = "Applovin";
        //impressionAttributes.ad_appid = "ad_appid";
        impressionAttributes.mediation_platform = "Applovin";
        impressionAttributes.ad_id = e.adUnitId;
        impressionAttributes.ad_type = (int)e.adsType;
        impressionAttributes.ad_ecpm = e.info.Revenue * 1000f;
        impressionAttributes.currency_type = "USD";
        impressionAttributes.is_rendered = true;
        SolarEngine.Analytics.trackAdImpression(impressionAttributes);
#endif
        }
    }

    #endregion
}