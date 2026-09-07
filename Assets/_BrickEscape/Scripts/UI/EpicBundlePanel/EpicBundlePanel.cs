using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using NabaGame.Tracking;
using NabaGame.UI;
using TMPro;
using UnityEngine;

namespace BrickEscape
{
    public class EpicBundlePanel : BaseUI
    {
        [SerializeField] private EditorTweenManager editorTweenManager;
        
        [SerializeField] private TextMeshProUGUI goldBonusText;
        [SerializeField] private TextMeshProUGUI infiniteHeartBonusText;
        [SerializeField] private TextMeshProUGUI hintBoosterBonusText;
        [SerializeField] private TextMeshProUGUI magicWandBoosterBonusText;
        [SerializeField] private TextMeshProUGUI hammerBoosterBonusText;
        
        [SerializeField] private TextMeshProUGUI priceText;
        [SerializeField] private string iapProductID;
        [SerializeField] private RawShop iapData;
        
        private CheatData cheatData;
        private PlayerProfile playerProfile;
        private HeartManager heartManager;
        
        #region Start, Update, Validate

        




        public void SetInfo()
        {
            SetupIAPData();
            SetupData();
            if (!playerProfile.isFirstLogin && !playerProfile.buySupperOffer) 
            {
                DOVirtual.DelayedCall(0.2f,()=> { Show(); });
            }
        }
        
        #endregion
        
        #region Open/Close

        public void Open()
        {
            Show();
        }

        public void Close()
        {
            AudioManager.Instance.PlayButtonSound();
            Hide();
        }

        void Close(bool noSound)
        {
            Hide();
        }

        public override void OnInAnimationStart()
        {
            editorTweenManager.PlayAllAnimations();
        }

        public override void OnOutAnimationFinish()
        {
            editorTweenManager.StopAllAnimations();
        }
        
        #endregion
        
        #region Setup

        void SetupData()
        {
            if (heartManager == null) heartManager = GameController.Instance.heartManager;
            if (playerProfile == null) playerProfile = GameManager.Instance.PlayerProfile;
            if (cheatData == null) cheatData = GameManager.Instance.cheatData;
            
            goldBonusText.SetText($"{iapData.GoldBonus}");
            infiniteHeartBonusText.SetText($"{iapData.InfinityHeartBonus}h");
            hintBoosterBonusText.SetText($"x{iapData.HintBonus}");
            magicWandBoosterBonusText.SetText($"x{iapData.MagicWandBonus}");
            hammerBoosterBonusText.SetText($"x{iapData.HammerBonus}");
        }
        
        void SetupIAPData()
        {
            List<RawShop> IAPData = GameManager.Instance.iapData.rawShops;
            foreach (RawShop rawShop in IAPData)
            {
                if (rawShop.ShopPackageType == ShopPackageType.SuperOffer)
                {
                    iapData = rawShop;
                    iapProductID = rawShop.PackageID;
                    break;
                }
            }
            
            //priceText.SetText(IAPManager.Instance.GetPriceLocal(iapData.PackageID));
            priceText.SetText(iapData.Price.Replace(",", ".") + " $");
        }
        
        #endregion
        
        #region Buy Action

        public void BuyAction()
        {
            AudioManager.Instance.PlayButtonSound();

            if (cheatData.AllowFreeIAP)
            {
                OnIAPComplete();
                return;
            }
            
            IAPManager.Instance.InitiatePurchase(iapProductID, (success) =>
            {
                if (success) 
                {
                    OnIAPComplete();
                    TrackingManager.TrackEvent(TrackingEvent.Purchased_IAP,
                        TrackingParamter.Type,iapProductID,
                        TrackingParamter.Level,GameManager.Instance.PlayerProfile.LevelProfile.CurrentLevel.ToString());
                }
                else 
                {
                    Debug.Log("Mua thất bại");
                }
                    
            });
        }
        
        void OnIAPComplete()
        {
            playerProfile.ChangeCoin(iapData.GoldBonus);
            heartManager.AddUnlimitedHeartTime(iapData.InfinityHeartBonus * 3600);
            playerProfile.ChangeHintBoosterUseCount(iapData.HintBonus);
            playerProfile.ChangeHammerBoosterUseCount(iapData.HammerBonus);
            playerProfile.ChangeMagicWandBoosterUseCount(iapData.MagicWandBonus);
            UIMainManager.Instance.purchaseCompletePanel.SetData(iapData);
            UIMainManager.Instance.purchaseCompletePanel.Open();
            GameManager.Instance.PlayerProfile.buySupperOffer  = true;
            UIMainManager.Instance.homePanel.EpicBundleBtn.gameObject.SetActive(false);
            Close(true);
        }
        
        #endregion
    }
}
