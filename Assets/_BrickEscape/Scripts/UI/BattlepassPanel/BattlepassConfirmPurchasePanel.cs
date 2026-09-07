using System.Collections;
using System.Collections.Generic;
using NabaGame.Tracking;
using NabaGame.UI;
using TMPro;
using UnityEngine;

namespace BrickEscape
{
    public class BattlepassConfirmPurchasePanel : BaseUI
    {
        [SerializeField] private BattlepassPanel battlepassPanel;
        public TextMeshProUGUI battlepassTimerText;
        [SerializeField] private RawShop iapData;
        [SerializeField] private string iapProductID;
        [SerializeField] private TextMeshProUGUI priceText;
        [SerializeField] private AnimButton buyBtn;

        //[SerializeField] private EditorTweenManager editorTweenManager;
        
        private CheatData cheatData;
        
        #region Setup

        public void SetInfo()
        {
            if (cheatData == null) cheatData = GameManager.Instance.cheatData;
            if (battlepassPanel == null) battlepassPanel = UIMainManager.Instance.battlepassPanel;
            SetupIAPData();
        }
        
        void SetupIAPData()
        {
            List<RawShop> IAPData = GameManager.Instance.iapData.rawShops;
            foreach (RawShop rawShop in IAPData)
            {
                if (rawShop.ShopPackageType == ShopPackageType.SpringBattlePass)
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

        public void Close(bool noSound)
        {
            Hide();
        }
        
        public override void OnInAnimationStart()
        {
            buyBtn.Play();
            //editorTweenManager.PlayAllAnimations();
        }
        
        public override void OnOutAnimationStart()
        {
            buyBtn.ResetAnim();
        }

        public override void OnOutAnimationFinish()
        {
            //editorTweenManager.StopAllAnimations();
        }
        
        #endregion
        
        #region Purchase Logic
        
        public void BuyAction()
        {
            AudioManager.Instance.PlayButtonSound();

            if (cheatData.AllowFreeIAP)
            {
                OnIAPComplete();
                return;
            }
            
// #if UNITY_EDITOR
//             
//             OnIAPComplete();
//             return;
//                 
// #endif
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
            Debug.Log("Success");
            battlepassPanel.OnIAPComplete();
            Close(true);
        }
        
        #endregion
        
        #region Timer Logic

        public void UpdateTimer(string time)
        {
            battlepassTimerText.SetText(time);
        
        }
        
  
        #endregion
    }
}
