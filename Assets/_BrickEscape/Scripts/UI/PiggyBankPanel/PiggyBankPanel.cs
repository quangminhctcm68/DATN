using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using NabaGame.Tracking;
using NabaGame.UI;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BrickEscape
{
    public class PiggyBankPanel : BaseUI
    {
        [SerializeField] private DOTweenAnimation textAnimation;
        [SerializeField] private ParticleSystem starEffect;
        
        [SerializeField] private List<int> piggyBankMilestones = new List<int>();
        [SerializeField] private PiggyBankMilestone piggyBankMilestonePrefab;
        [SerializeField] private Transform milestonesHolder;
        [SerializeField] private RectTransform milestonesHolderRectTransform;
        [SerializeField] private List<PiggyBankMilestone> spawnedMilestones = new List<PiggyBankMilestone>();
        [SerializeField] private PiggyBankMilestone piggyBankMaxMilestone;

        [SerializeField] private Image fillBar;
        [SerializeField] private Transform infoNoteTransform;
        
        [SerializeField] private Button cashOutBtn;
        [SerializeField] private Button showInfoBtn;
        [SerializeField] private Button closeInfoBtn;

        [SerializeField] private CanvasGroup showInfoBtnCanvasGroup;
        [SerializeField] private CanvasGroup infoNoteCanvasGroup;
        
        [SerializeField] private AnimButton cashOutBtnAnimation;

        [SerializeField] private string iapProductID;
        [SerializeField] private int currentPiggyValue;
        [SerializeField] private int maxPiggyValue;
        
        [SerializeField] private float barFillValueMultiplier;
        
        [SerializeField] private RawShop rawShopData;
        
        private PiggyBankProfile piggyBankProfile;
        
        private CheatData cheatData;

        private Tweener fillAnimation;
        private Sequence infoNoteAnimation;

        [SerializeField] TextMeshProUGUI priceText;




        #region Start, Update, Validate

        public void SetInfo()
        {
            piggyBankProfile = GameManager.Instance.PlayerProfile.PiggyBankProfile;
            SetupData();
            SetupMilestones();
            SetupFillBarValue();
            SetupButtons();
            CloseNote(true);
            
            UIMainManager.Instance.homePanel.ChangePiggyButtonVisibility(IsPiggyContainMinimumFund);
        }

        void SetupData()
        {
            piggyBankProfile.SetMaxCoins(piggyBankMilestones[piggyBankMilestones.Count - 1]);
            currentPiggyValue = piggyBankProfile.currentCoins;
            maxPiggyValue = piggyBankProfile.maxCoins;
            List<RawShop> IAPData = GameManager.Instance.iapData.rawShops;
            foreach (RawShop rawShop in IAPData)
            {
                if (rawShop.ShopPackageType == ShopPackageType.PiggyBank)
                {
                    rawShopData = rawShop;
                    iapProductID = rawShop.PackageID;
                    priceText.text = $"{rawShop.Price}$";
                    break;
                }
            }

            cheatData = GameManager.Instance.cheatData;
            //Get IAP product ID here
        }

        void SetupMilestones()
        {
            if (piggyBankMilestones.Count <= 1) return;

            for (int i = 0; i < piggyBankMilestones.Count - 1; i++)
            {
                PiggyBankMilestone tempMilestoneRef = Instantiate(piggyBankMilestonePrefab, milestonesHolder);
                tempMilestoneRef.SetValue(piggyBankMilestones[i]);
                tempMilestoneRef.gameObject.SetActive(true);
                spawnedMilestones.Add(tempMilestoneRef);
            }
            
            foreach (var milestone in spawnedMilestones)
            {
                SetMilestonePosition(milestone);
            }
            
            piggyBankMaxMilestone.SetValue(maxPiggyValue);
        }

        void SetMilestonePosition(PiggyBankMilestone milestone)
        {
            float normalizedValue = Mathf.Clamp01(milestone.MilestoneValue / (float)maxPiggyValue);
            float barWidth = milestonesHolderRectTransform.rect.width;
            float newX = normalizedValue * barWidth;
            milestone.RectTransform.anchoredPosition = new Vector2(newX, milestone.RectTransform.anchoredPosition.y);
        }

        void SetupFillBarValue()
        {
            barFillValueMultiplier = 1 / (float)maxPiggyValue;
        }

        void SetupButtons()
        {
            cashOutBtn.onClick.AddListener(delegate{ CashOutAction(); });
            showInfoBtn.onClick.AddListener(delegate{ OpenInfoNote(); });
            closeInfoBtn.onClick.AddListener(delegate{ CloseInfoNote(); });
        }
        
        #endregion
        
        #region Open, Close

        public void Open()
        {
            Show();
        }

        public void Close()
        {
            AudioManager.Instance.PlayButtonSound();
            CloseInfoNote();
            Hide();
        }

        public void Close(bool noSound)
        {
            Hide();
        }

        public override void OnInAnimationStart()
        {
            OnOpen();
        }

        public override void OnInAnimationFinish()
        {
            OnOpenFinish();
        }

        public override void OnOutAnimationStart()
        {
            OnClose();
        }
        
        public override void OnOutAnimationFinish()
        {
            OnCloseFinish();
        }
        
        #endregion
        
        #region On Open/Close Logic

        void OnOpen()
        {
            ResetFillValue();
            currentPiggyValue = piggyBankProfile.currentCoins;
        }

        void OnOpenFinish()
        {
            ChangeStarEffectStatus(true);
            PlayBarAnimation();
        }

        void OnClose()
        {
            ChangeStarEffectStatus(false);
        }

        void OnCloseFinish()
        {
            ChangeCashOutButtonAnimationStatus(false);
        }
        
        #endregion
        
        #region BarAnimation

        void PlayBarAnimation()
        {
            float endValue = currentPiggyValue * barFillValueMultiplier;
            StopBarAnimation();

            fillAnimation = DG.Tweening.DOTweenModuleUI.DOFillAmount(fillBar, endValue, 0.35f).OnComplete(() =>
            {
                fillAnimation = null;
                CheckShowCashOutButton();
            });
        }

        void StopBarAnimation()
        {
            if (fillAnimation != null)
            {
                fillAnimation.Rewind();
                fillAnimation.Kill();
                fillAnimation = null;
            }
        }

        void ResetFillValue()
        {
            fillBar.fillAmount = 0;
        }

        #endregion
        
        #region Cash Out Logic
        
        public void CashOutAction()
        {
            AudioManager.Instance.PlayButtonSound();

            if (cheatData.AllowFreeIAP)
            {
                OnIAPComplete();
                return;
            }
            
            // #if UNITY_EDITOR
            //
            // OnIAPComplete();
            // return;
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
            ChangeCashOutVisibility(false);
            AudioManager.Instance.PlaySFX(SFXID.Coin);
            GameManager.Instance.PlayerProfile.ChangeCoin(currentPiggyValue);
            UIMainManager.Instance.rewardGetPanel.SetData(RewardType.Coin, currentPiggyValue);
            UIMainManager.Instance.rewardGetPanel.Open();
            UIMainManager.Instance.clickEffectPanel.coinEffect.Play();
            AudioManager.Instance.PlaySFX(SFXID.Coin);
            piggyBankProfile.CashOut();
            currentPiggyValue = piggyBankProfile.currentCoins;
            PlayBarAnimation();
            UIMainManager.Instance.homePanel.ChangePiggyButtonVisibility(false);
        }

        void CheckShowCashOutButton()
        {
            if (currentPiggyValue < piggyBankMilestones[0])
            {
                ChangeCashOutButtonAnimationStatus(false);
                ChangeCashOutVisibility(false);
            }
            else
            {
                ChangeCashOutVisibility(true);
                ChangeCashOutButtonAnimationStatus(true);
            }
        }
        
        void ChangeCashOutButtonAnimationStatus(bool newStatus)
        {
            if (newStatus) cashOutBtnAnimation.Play();
            else cashOutBtnAnimation.ResetAnim();
        }
        
        void ChangeCashOutVisibility(bool isVisible)
        {
            cashOutBtn.gameObject.SetActive(isVisible);
        }
        
        #endregion
        
        #region Effects

        public void ChangeStarEffectStatus(bool newStatus)
        {
            if (newStatus) starEffect.Play();
            else starEffect.Stop();
        }
        
        #endregion
        
        #region Info Logic

        void OpenInfoNote()
        {
            ChangeShowInfoButtonInteractableStatus(false);
            OpenNote();
        }

        void CloseInfoNote()
        {
            ChangeCloseInfoButtonInteractableStatus(false);
            CloseNote();
        }
        
        #endregion
        
        #region Info Note Animation

        void OpenNote()
        {
            StopNoteAnimation();
            infoNoteAnimation = DOTween.Sequence();

            infoNoteAnimation
                //.Append(infoNoteTransform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack))
                .Append(DG.Tweening.DOTweenModuleUI.DOFade(infoNoteCanvasGroup, 1, 0.35f).SetEase(Ease.Linear))
                .Join(DG.Tweening.DOTweenModuleUI.DOFade(showInfoBtnCanvasGroup, 0, 0.5f).SetEase(Ease.Linear));
            
            infoNoteAnimation.OnComplete(delegate
            {
                infoNoteAnimation = null;
                ChangeCloseInfoButtonInteractableStatus(true);
            });
        }
        
        void OpenNote(bool noAnimation)
        {
            StopNoteAnimation();
            //infoNoteTransform.localScale = Vector3.one;
            infoNoteCanvasGroup.alpha = 1;
            showInfoBtnCanvasGroup.alpha = 0;
            ChangeCloseInfoButtonInteractableStatus(true);
        }

        void CloseNote()
        {
            StopNoteAnimation();
            infoNoteAnimation = DOTween.Sequence();
            
            infoNoteAnimation
                //.Append(infoNoteTransform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack))
                //.Append(DG.Tweening.DOTweenModuleUI.DOFade(infoNoteCanvasGroup, 0, 0.5f).SetDelay(0.15f).SetEase(Ease.Linear))
                .Append(DG.Tweening.DOTweenModuleUI.DOFade(infoNoteCanvasGroup, 0, 0.5f).SetEase(Ease.Linear))
                .Join(DG.Tweening.DOTweenModuleUI.DOFade(showInfoBtnCanvasGroup, 1, 0.5f).SetEase(Ease.Linear));
            
            infoNoteAnimation.OnComplete(delegate
            {
                infoNoteAnimation = null;
                ChangeShowInfoButtonInteractableStatus(true);
            });
        }
        
        void CloseNote(bool noAnimation)
        {
            StopNoteAnimation();
            //infoNoteTransform.localScale = Vector3.zero;
            infoNoteCanvasGroup.alpha = 0;
            showInfoBtnCanvasGroup.alpha = 1;
            ChangeShowInfoButtonInteractableStatus(true);
        }
        
        void StopNoteAnimation()
        {
            if (infoNoteAnimation != null)
            {
                infoNoteAnimation.Kill();
                infoNoteAnimation = null;
            }
        }
        
        #endregion
        
        #region Button Interactable Logic
        
        void ChangeShowInfoButtonInteractableStatus(bool newStatus)
        {
            showInfoBtn.interactable = newStatus;
        }

        void ChangeCloseInfoButtonInteractableStatus(bool newStatus)
        {
            //closeInfoBtn.interactable = newStatus;
            closeInfoBtn.gameObject.SetActive(newStatus);
        }
        
        #endregion

        #region Getters, Setters

        public bool IsPiggyContainMinimumFund => piggyBankProfile.currentCoins >= piggyBankMilestones[0];

        #endregion
        
        #region Test

        [SerializeField] int testValue;
        
        [Button]
        void TestAnimation()
        {
            currentPiggyValue = testValue;
            PlayBarAnimation();
        }
        
        [Button]
        void SetNewValueToPiggyBank()
        {
            piggyBankProfile.AddCoinsToPiggyBank(testValue, true);
            currentPiggyValue = testValue;
            PlayBarAnimation();
        }
        
        public void SetNewValueToPiggyBank(int value)
        {
            piggyBankProfile.AddCoinsToPiggyBank(value, true);
            currentPiggyValue = value;
            PlayBarAnimation();
        }
        
        #endregion
    }
}
