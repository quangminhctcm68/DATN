using System.Collections;
using System.Collections.Generic;
using NabaGame.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BrickEscape
{
    public class PurchaseCompletePanel : BaseUI
    {
        [SerializeField] private GameObject rewardIcon_noAds;
        [SerializeField] private GameObject rewardIcon_gold;
        [SerializeField] private TextMeshProUGUI rewardAmountText_gold;
        [SerializeField] private GameObject rewardIcon_infiniteHeart;
        [SerializeField] private TextMeshProUGUI rewardAmountText_infiniteHeart;
        [SerializeField] private GameObject rewardIcon_hintBooster;
        [SerializeField] private TextMeshProUGUI rewardAmountText_hintBooster;
        [SerializeField] private GameObject rewardIcon_hammerBooster;
        [SerializeField] private TextMeshProUGUI rewardAmountText_hammerBooster;
        [SerializeField] private GameObject rewardIcon_magicWandBooster;
        [SerializeField] private TextMeshProUGUI rewardAmountText_magicWandBooster;
        [SerializeField] private AnimButton animBtn;
        [SerializeField] private EditorTweenManager editorTweenManager;

        private RawShop purchaseData;
        
        #region Start, Update, Validate

        public void Open()
        {
            Show();
        }

        public void Close()
        {
            AudioManager.Instance.PlayButtonSound();
            Hide();
        }
        
        public override void OnInAnimationStart()
        {
            animBtn.Play();
            editorTweenManager.PlayAllAnimations();
        }
        
        public override void OnOutAnimationStart()
        {
            animBtn.ResetAnim();
        }

        public override void OnOutAnimationFinish()
        {
            editorTweenManager.StopAllAnimations();
        }
        
        #endregion
        
        #region Set Data

        public void SetData(RawShop purchaseData)
        {
            this.purchaseData = purchaseData;
            ProcessReward_NoAds();
            ProcessReward_Gold();
            ProcessReward_InfiniteHeart();
            ProcessReward_HintBooster();
            ProcessReward_HammerBooster();
            ProcessReward_MagicWandBooster();
        }

        void ProcessReward_NoAds()
        {
            rewardIcon_noAds.SetActive(purchaseData.IsNoads);
        }

        void ProcessReward_Gold()
        {
            if (purchaseData.GoldBonus <= 0)
                rewardIcon_gold.SetActive(false);
            else
            {
                rewardIcon_gold.SetActive(true);
                rewardAmountText_gold.SetText(purchaseData.GoldBonus.ToString());
            }
        }

        void ProcessReward_InfiniteHeart()
        {
            if(purchaseData.InfinityHeartBonus <= 0)
                rewardIcon_infiniteHeart.SetActive(false);
            else
            {
                rewardIcon_infiniteHeart.SetActive(true);
                rewardAmountText_infiniteHeart.SetText($"{(float)purchaseData.InfinityHeartBonus}h");
            }
        }

        void ProcessReward_HintBooster()
        {
            if (purchaseData.HintBonus <= 0)
                rewardIcon_hintBooster.SetActive(false);
            else
            {
                rewardIcon_hintBooster.SetActive(true);
                rewardAmountText_hintBooster.SetText(purchaseData.HintBonus.ToString());
            }
        }

        void ProcessReward_HammerBooster()
        {
            if (purchaseData.HammerBonus <= 0)
                rewardIcon_hammerBooster.SetActive(false);
            else
            {
                rewardIcon_hammerBooster.SetActive(true);
                rewardAmountText_hammerBooster.SetText(purchaseData.HammerBonus.ToString());
            }
        }

        void ProcessReward_MagicWandBooster()
        {
            if (purchaseData.MagicWandBonus <= 0)
                rewardIcon_magicWandBooster.SetActive(false);
            else
            {
                rewardIcon_magicWandBooster.SetActive(true);
                rewardAmountText_magicWandBooster.SetText(purchaseData.MagicWandBonus.ToString());
            }
        }
        
        #endregion
    }
}
