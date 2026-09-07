using SRDebugger;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using NabaGame.Tracking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BrickEscape
{
    public class IAPItem : MonoBehaviour
    {
        public ShopPackageType packageType;
        public RawShop data;
        [SerializeField] private TextMeshProUGUI txtPrice;
        [SerializeField] private TextMeshProUGUI txtGoldBonus;
        [SerializeField] private TextMeshProUGUI txtMagicWand;
        [SerializeField] private TextMeshProUGUI txtHammer;
        [SerializeField] private TextMeshProUGUI txtHint;
        [SerializeField] private TextMeshProUGUI txtInfityHeart;
        public Button btnPurchase;
        public AnimButton animButton;

        private RawShop shopData;
        private CheatData cheatData;
        private TextMeshProUGUI coinText;
        public void OnValidate()
        {
            animButton = GetComponentInChildren<AnimButton>();
        }
        public void SetInfor() 
        {
            shopData = GameManager.Instance.iapData.rawShops.Find(x => x.ShopPackageType == packageType);   
            data = shopData;
            if(txtPrice != null)
                txtPrice.text = data.Price.Replace(",", ".") + " $";
            //txtPrice.text = IAPManager.Instance.GetPriceLocal(data.PackageID);
            if (txtGoldBonus != null)
                txtGoldBonus.text = shopData.GoldBonus.ToString();
            if (txtMagicWand != null)
                txtMagicWand.text = "x" + shopData.MagicWandBonus.ToString();
            if (txtHammer != null)
                txtHammer.text = "x"+shopData.HammerBonus.ToString();
            if (txtHint != null)
                txtHint.text = "x"+shopData.HintBonus.ToString();
            if (txtInfityHeart != null)
                txtInfityHeart.text = shopData.InfinityHeartBonus.ToString()+"h";
            btnPurchase.onClick.AddListener(OnPurchaseSuccess);
            //animButton.Play();
            coinText = UIMainManager.Instance.homePanel.Coin;
            cheatData = GameManager.Instance.cheatData;
        }
        public void OnPurchaseSuccess()
        {
            GameController.Instance.audioManager.PlayButtonSound();
            if ((packageType == ShopPackageType.NoAds || packageType == ShopPackageType.VipNoAds) && GameManager.Instance.PlayerProfile.isNoAds == 1) return;
            if (cheatData.AllowFreeIAP)
            {
                OnComplete();
                return;
            }
           

            IAPManager.Instance.InitiatePurchase(data.PackageID, (success) =>
            {
                if (success) 
                {
                    OnComplete();
                    TrackingManager.TrackEvent(TrackingEvent.Purchased_IAP,
                        TrackingParamter.Type,data.ShopPackageType.ToString(),
                        TrackingParamter.Level,GameManager.Instance.PlayerProfile.LevelProfile.CurrentLevel.ToString());
                }
                else 
                {
                    Debug.Log("Mua thất bại");
                }
                    
            });
        }

        public void OnComplete()
        {
         
            if (data.GoldBonus > 0)
            {
                int startValue = GameManager.Instance.PlayerProfile.Coin;
                int endValue = startValue+ data.GoldBonus;
                UIMainManager.Instance.clickEffectPanel.PlayCoinCollectFX(coinText,startValue, endValue);
                GameManager.Instance.PlayerProfile.ChangeCoin(data.GoldBonus);
                AudioManager.Instance.PlaySFX(SFXID.Coin);
            }
            GameController.Instance.heartManager.AddUnlimitedHeartTime(data.InfinityHeartBonus * 3600);

            GameManager.Instance.PlayerProfile.ChangeHammerBoosterUseCount(data.HammerBonus);
            GameManager.Instance.PlayerProfile.ChangeMagicWandBoosterUseCount(data.MagicWandBonus); 
            GameManager.Instance.PlayerProfile.ChangeHintBoosterUseCount(data.HintBonus);
            if (data.IsNoads) 
            {
                GameManager.Instance.SetNoAds();
            }

            UIMainManager.Instance.purchaseCompletePanel.SetData(shopData);
            UIMainManager.Instance.purchaseCompletePanel.Open();
        }

    }
}
