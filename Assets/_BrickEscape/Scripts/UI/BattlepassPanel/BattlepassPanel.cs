using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using NabaGame.UI;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BrickEscape
{
    public class BattlepassPanel : BaseUI
    {
        [SerializeField] private Image expBar;
        [SerializeField] private Image expBar_center;
        [SerializeField] private GameObject battlepassLevelTextHolder;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private TextMeshProUGUI battlepassProgressText;
        [SerializeField] private TextMeshProUGUI battlepassLevelText;
        [SerializeField] private TextMeshProUGUI battlepassTimerText;
        
        [SerializeField] private List<BattlepassRewardData_Regular> milestones_Regular = new List<BattlepassRewardData_Regular>();
        [SerializeField] private List<BattlepassRewardData_Premium> milestones_Premium = new List<BattlepassRewardData_Premium>();
        
        [SerializeField] private List<BattlepassMilestone> spawnedMilestones = new List<BattlepassMilestone>();
        [SerializeField] private BattlepassMilestone battlePassMilestonePrefab;
        [SerializeField] private Transform milestonesHolder;
        [SerializeField] private RectTransform milestonesHolder_RectTransform;
        
        [SerializeField] private Button buyBattlepassBtn;
        [SerializeField] private bool isBuyButtonVisible = false;

        [SerializeField] private RawShop rawShopData;
        [SerializeField] private string iapProductID;
        
        [SerializeField] private int lastTimeCheckedMilestoneLevel = 0;
        [SerializeField] private int lastTimeCheckedEXP = 0;
        [SerializeField] private int nextLevelEXPRequirement = 0;
        [SerializeField] private int newMilestoneLevel;
        [SerializeField] private int newEXP;
        [SerializeField] private float fillAmount = 0;

        [SerializeField] private int levelDotCount = 0;
        [SerializeField] private int levelUpCountUponOpen = 0;
        [SerializeField] private bool expUpdateAllowed = false;
        [SerializeField] private bool checkedForUnlock = false;
        [SerializeField] private int maxLevel = 0;
        
        [SerializeField] private Button closeInstructionBtn;
        [SerializeField] private CanvasGroup instructionCanvasGroup;
        [SerializeField] private bool instructionShown = false;

        [SerializeField] private Vector3 buyPremiumBattlepassButtonOGPos;
        [SerializeField] private Vector3 buyPremiumBattlepassButtonAnimationPos;

        [SerializeField] private ParticleSystem levelUpEffect;
        //[SerializeField] private EditorTweenManager autoPlayTween;
        
        private BattlepassProfile battlepassProfile;
        private CheatData cheatData;
        private BattlepassManager battlepassManager;

        private Sequence expAnimation_top;
        private Sequence expAnimation_eachReward;
        private Tweener showInstructionAnimation;
        private Sequence buyPremiumBattlepassButtonAnimation;
        
        #region Start, Update, Validate

        public void SetInfo()
        {
            GetData();
            SpawnMilestones();
            CheckSaveData();
            SetupIAPData();
            CheckBattlepassPurchase();
            SetupButtons();
            ShowInstruction(0, false);
        }

        [Button]
        public void Open()
        {
            //UpdateData();
            Show();
        }

        [Button]
        public void Close()
        {
            AudioManager.Instance.PlayButtonSound();
            UIMainManager.Instance.homePanel.Show();
            UIMainManager.Instance.homePanel.CheckForBattlepassNotificationVisibility();
            Hide();
        }
        
        public void Close(bool noSound)
        {
            Hide();
        }

        public override void OnInAnimationStart()
        {
            if (isBuyButtonVisible)
                ChangeBuyBattlepassButtonAnimation(true);
            checkedForUnlock = false;
            //autoPlayTween.PlayAllAnimations();
            PlayAllPremiumRewardEffect();
            
            if (isBuyButtonVisible)
                PlayBuyPremiumButtonAnimation();
        }

        public override void OnInAnimationFinish()
        {
            UpdateData();
        }

        public override void OnOutAnimationStart()
        {
            if (isBuyButtonVisible)
                ChangeBuyBattlepassButtonAnimation(false);
            if (instructionShown) ShowInstruction(false);
        }
        
        public override void OnOutAnimationFinish()
        {
            CancelBothAnimation();
            //autoPlayTween.StopAllAnimations();
            StopAllPremiumRewardEffect();
            
            if (isBuyButtonVisible)
                StopBuyPremiumButtonAnimation();
        }
        
        #endregion
        
        #region Setup
        
        void SpawnMilestones()
        {
            //int milestoneCount = GameManager.Instance.battlepassData.GetNumberOfRewards();
            // for (int i = 0; i < milestoneCount; i++)
            // {
            //     BattlepassMilestone newMilestone = Instantiate(battlePassMilestonePrefab, milestonesHolder);
            //     newMilestone.SetMilestoneLevel(i + 1);
            //     newMilestone.Init();
            //     spawnedMilestones.Add(newMilestone);
            // }

            foreach (var milestone in milestones_Regular)
            {
                BattlepassMilestone newMilestone = Instantiate(battlePassMilestonePrefab, milestonesHolder);
                newMilestone.SetMilestoneLevel(milestone.level);
                newMilestone.Init();
                spawnedMilestones.Add(newMilestone);
            }
        }

        void CheckSaveData()
        {
            if (battlepassProfile == null) battlepassProfile = GameManager.Instance.PlayerProfile.BattlepassProfile;
            if (cheatData == null) cheatData = GameManager.Instance.cheatData;
            if (battlepassManager == null) battlepassManager = GameController.Instance.battlepassManager;
            
            lastTimeCheckedMilestoneLevel = battlepassProfile.CurrentLevel;
            lastTimeCheckedEXP = battlepassProfile.CurrentEXP;
            
            if (lastTimeCheckedMilestoneLevel >= maxLevel)
            {
                battlepassLevelText.SetText("Max");
                expBar.fillAmount = 1;
                expBar_center.fillAmount = 1;
                battlepassProgressText.SetText($"{lastTimeCheckedEXP}");
            }
            else
            {
                battlepassLevelText.SetText($"{lastTimeCheckedMilestoneLevel + 1}");
                nextLevelEXPRequirement = GetRequiredEXP(lastTimeCheckedMilestoneLevel + 1);
                //expBar.fillAmount = battlepassManager.GetPercentageToNextLevel();
                expBar.fillAmount = GetFillAmount(lastTimeCheckedEXP, nextLevelEXPRequirement);
                expBar_center.fillAmount = GetFillAmount_RewardVer(lastTimeCheckedMilestoneLevel);
                battlepassProgressText.SetText($"{lastTimeCheckedEXP}/{nextLevelEXPRequirement}");
            }
            
            
        }

        void SetupIAPData()
        {
            List<RawShop> IAPData = GameManager.Instance.iapData.rawShops;
            foreach (RawShop rawShop in IAPData)
            {
                if (rawShop.ShopPackageType == ShopPackageType.SpringBattlePass)
                {
                    rawShopData = rawShop;
                    iapProductID = rawShop.PackageID;
                    break;
                }
            }
        }

        void GetData()
        {
            milestones_Regular = GameManager.Instance.battlepassData.GetAllRegularRewards;
            milestones_Premium = GameManager.Instance.battlepassData.GetAllPremiumRewards;

            foreach (var milestone in milestones_Regular)
            {
                if (milestone.level > maxLevel) maxLevel = milestone.level;
            }
            //maxLevel = GameController.Instance.battlepassManager.MaxLevel;
            levelDotCount = GameManager.Instance.battlepassData.GetNumberOfRewards();
        }

        void SetupButtons()
        {
            closeInstructionBtn.onClick.AddListener(CloseInstruction);
        }
        
        #endregion
        
        #region Update Data Upon Opening

        public void UpdateData()
        {
            newMilestoneLevel = battlepassManager.CurrentBattlepassLevel;
            newEXP = battlepassManager.CurrentBattlepassEXP;

            //levelUpCountUponOpen = battlepassManager.LevelUpCount;
            levelUpCountUponOpen = Mathf.Abs(newMilestoneLevel - lastTimeCheckedMilestoneLevel);
            expUpdateAllowed = battlepassManager.ExpChanged;
            
            battlepassManager.ResetChangesStatus();
            
            //Animation start here
            if (levelUpCountUponOpen == 1)
            {
                if (newMilestoneLevel >= maxLevel)
                    fillAmount = 1;
                else
                    fillAmount = (float)newEXP / GetRequiredEXP(newMilestoneLevel);
                StartEXPBarAnimationV2_LevelUpOnceVer();
            }
            else if (levelUpCountUponOpen > 1)
            {
                if (newMilestoneLevel >= maxLevel)
                    fillAmount = 1;
                else
                    fillAmount = (float)newEXP / GetRequiredEXP(newMilestoneLevel);
                StartEXPBarAnimationV2();
            }
            else if (expUpdateAllowed)
            {
                if (lastTimeCheckedMilestoneLevel >= maxLevel)
                    fillAmount = 1;
                else
                    fillAmount = (float)newEXP / nextLevelEXPRequirement;
                StartEXPBarAnimationV2_NoLevelUpVer();
            }
        }
        
        #endregion
        
        #region Check Unlock Logic

        void CheckBattlepassPurchase()
        {
            if (battlepassProfile.IsBattlePassPurchased)
            {
                ChangeBuyBattlepassButtonVisibility(false);
            }
            else
            {
                ChangeBuyBattlepassButtonVisibility(true);
            }
        }
        
        #endregion
        
        #region Buy Battlepass

        public void BuyAction()
        {
            AudioManager.Instance.PlayButtonSound();
            UIMainManager.Instance.battlepassConfirmPurchasePanel.Open();
            // if (cheatData.AllowFreeIAP)
            // {
            //     OnIAPComplete();
            //     return;
            // }
            //
            // #if UNITY_EDITOR
            //
            // OnIAPComplete();
            // return;
            //     
            // #endif
            // IAPManager.Instance.InitiatePurchase(iapProductID, (success) =>
            // {
            //     if (success) 
            //     {
            //         OnIAPComplete();
            //     }
            //     else 
            //     {
            //         Debug.Log("Mua thất bại");
            //     }
            //         
            // });
            
        }
        
        public void OnIAPComplete()
        {
            battlepassProfile.SetBattlePassPurchased(true);
            StopBuyPremiumButtonAnimation();
            ChangeBuyBattlepassButtonVisibility(false);
            CheckUnlockForAllPremiumRewards(true);
        }

        void ChangeBuyBattlepassButtonAnimation(bool status)
        {
            // if (status) buyBattlepassBtn.Play();
            // else buyBattlepassBtn.ResetAnim();
        }

        void ChangeBuyBattlepassButtonVisibility(bool status)
        {
            isBuyButtonVisible = status;
            buyBattlepassBtn.gameObject.SetActive(status);
        }

        void CheckUnlockForAllPremiumRewards()
        {
            foreach(var milestone in spawnedMilestones)
            {
                milestone.CheckUnlockForAll();
            }
        }
        
        void CheckUnlockForAllPremiumRewards(bool alsoUnlock)
        {
            foreach(var milestone in spawnedMilestones)
            {
                milestone.UnlockPremiumReward();
                milestone.CheckUnlockForAll();
            }
        }
        
        #endregion
        
        #region EXP Animation

        private List<BattlepassMilestone> milestonesToUse = new List<BattlepassMilestone>();

        void StartEXPBarAnimationV2()
        {
            StopEXPBarAnimation();
            expAnimation_top = DOTween.Sequence();

            for (int i = 0; i <= levelUpCountUponOpen; i++)
            {
                if (i < levelUpCountUponOpen)
                {
                    expAnimation_top
                        .Append(DG.Tweening.DOTweenModuleUI.DOFillAmount(expBar, 1, 0.75f).SetEase(Ease.Linear))
                        
                        .Join(DOTween.To(
                                () => lastTimeCheckedEXP,
                                x => lastTimeCheckedEXP = x,
                                nextLevelEXPRequirement,
                                0.5f)
                            .OnUpdate(delegate
                            {
                                battlepassProgressText.SetText($"{lastTimeCheckedEXP}/{nextLevelEXPRequirement}");
                            })
                            .SetEase(Ease.Linear))
                        .AppendCallback(delegate
                        {
                            lastTimeCheckedMilestoneLevel++;
                            lastTimeCheckedEXP = 0;

                            expBar.fillAmount = 0;
                            nextLevelEXPRequirement = GetRequiredEXP(lastTimeCheckedMilestoneLevel);
                            if (lastTimeCheckedMilestoneLevel + 1 > maxLevel)
                            {
                                battlepassLevelText.SetText($"Max");
                                AudioManager.Instance.PlaySFX(SFXID.Star_3);
                                levelUpEffect.Play();
                            }
                            else
                            {
                                battlepassLevelText.SetText($"{lastTimeCheckedMilestoneLevel + 1}");
                                AudioManager.Instance.PlaySFX(SFXID.Star_3);
                                levelUpEffect.Play();
                            }
                            
                            if (lastTimeCheckedMilestoneLevel >= maxLevel)
                            {
                                battlepassProgressText.SetText($"{lastTimeCheckedEXP}");
                            }
                            else
                            {
                                battlepassProgressText.SetText($"{lastTimeCheckedEXP}/{nextLevelEXPRequirement}");
                            }
                        })
                        .AppendInterval(0.15f);
                }
                else
                {
                    if (newMilestoneLevel >= maxLevel)
                    {
                        expAnimation_top
                        .Append(DG.Tweening.DOTweenModuleUI
                            .DOFillAmount(expBar, fillAmount, 0.75f)
                            .SetEase(Ease.Linear))
                        .Join(DOTween.To(
                                () => lastTimeCheckedEXP,
                                x => lastTimeCheckedEXP = x,
                                newEXP,
                                0.5f)
                            .OnUpdate(delegate
                            {
                                battlepassProgressText.SetText($"{lastTimeCheckedEXP}");
                            })
                            .SetEase(Ease.Linear));
                    }
                    else
                    {
                        expAnimation_top
                            .Append(DG.Tweening.DOTweenModuleUI
                                .DOFillAmount(expBar, fillAmount, 0.75f)
                                .SetEase(Ease.Linear))
                            .Join(DOTween.To(
                                    () => lastTimeCheckedEXP,
                                    x => lastTimeCheckedEXP = x,
                                    newEXP,
                                    0.5f)
                                .OnUpdate(delegate
                                {
                                    battlepassProgressText.SetText($"{lastTimeCheckedEXP}/{nextLevelEXPRequirement}");
                                })
                                .SetEase(Ease.Linear));
                    }
                }
            }

            expAnimation_top.OnComplete(delegate
            {
                expAnimation_top = null;
                StartEXPRewardAnimationV2();
            });
        }
        
        void StartEXPBarAnimationV2_NoLevelUpVer()
        {
            StopEXPBarAnimation();
            expAnimation_top = DOTween.Sequence();

            expAnimation_top
                .Append(DG.Tweening.DOTweenModuleUI
                    .DOFillAmount(expBar, fillAmount, 0.75f)
                    .SetEase(Ease.Linear));
                if (lastTimeCheckedMilestoneLevel >= maxLevel)
                {
                    expAnimation_top .Join(DOTween.To(
                            () => lastTimeCheckedEXP,
                            x => lastTimeCheckedEXP = x,
                            newEXP,
                            0.5f)
                        .OnUpdate(delegate
                        {
                            battlepassProgressText.SetText($"{lastTimeCheckedEXP}");
                        })
                        .SetEase(Ease.Linear));
                    
                }
                else
                {
                    expAnimation_top .Join(DOTween.To(
                            () => lastTimeCheckedEXP,
                            x => lastTimeCheckedEXP = x,
                            newEXP,
                            0.5f)
                        .OnUpdate(delegate
                        {
                            battlepassProgressText.SetText($"{lastTimeCheckedEXP}/{nextLevelEXPRequirement}");
                        })
                        .SetEase(Ease.Linear));
                }

            expAnimation_top.OnComplete(delegate
            {
                expAnimation_top = null;
            });
        }

        void StartEXPBarAnimationV2_LevelUpOnceVer()
        {
            StopEXPBarAnimation();
            expAnimation_top = DOTween.Sequence();
            
            expAnimation_top
                .Append(DG.Tweening.DOTweenModuleUI.DOFillAmount(expBar, 1, 0.75f).SetEase(Ease.Linear))
                .Join(DOTween.To(
                        () => lastTimeCheckedEXP,
                        x => lastTimeCheckedEXP = x,
                        nextLevelEXPRequirement,
                        0.5f)
                    .OnUpdate(delegate
                    {
                        battlepassProgressText.SetText($"{lastTimeCheckedEXP}/{nextLevelEXPRequirement}");
                    })
                    .SetEase(Ease.Linear))
                .AppendCallback(delegate
                {
                    lastTimeCheckedMilestoneLevel++;
                    lastTimeCheckedEXP = 0;

                    expBar.fillAmount = 0;
                    nextLevelEXPRequirement = GetRequiredEXP(lastTimeCheckedMilestoneLevel);
                    if (lastTimeCheckedMilestoneLevel + 1 > maxLevel)
                    {
                        battlepassLevelText.SetText($"Max");
                        AudioManager.Instance.PlaySFX(SFXID.Star_3);
                        levelUpEffect.Play();
                    }
                    else
                    {
                        battlepassLevelText.SetText($"{lastTimeCheckedMilestoneLevel + 1}");
                        AudioManager.Instance.PlaySFX(SFXID.Star_3);
                        levelUpEffect.Play();
                    }
                    
                    if (lastTimeCheckedMilestoneLevel >= maxLevel)
                    {
                        battlepassProgressText.SetText($"{lastTimeCheckedEXP}");
                    }
                    else
                    {
                        battlepassProgressText.SetText($"{lastTimeCheckedEXP}/{nextLevelEXPRequirement}");
                    }
                })
                .AppendInterval(0.15f)
                .Append(DG.Tweening.DOTweenModuleUI
                    .DOFillAmount(expBar, fillAmount, 0.75f)
                    .SetEase(Ease.Linear))
                .Join(DOTween.To(
                        () => lastTimeCheckedEXP,
                        x => lastTimeCheckedEXP = x,
                        newEXP,
                        0.5f)
                    .OnUpdate(delegate
                    {
                        battlepassProgressText.SetText($"{lastTimeCheckedEXP}/{nextLevelEXPRequirement}");
                    })
                    .SetEase(Ease.Linear));
            
            expAnimation_top.OnComplete(delegate
            {
                expAnimation_top = null;
                StartEXPRewardAnimationV2();
            });
        }

        void StartEXPRewardAnimationV2()
        {
            StopRewardEXPAnimation();
            expAnimation_eachReward = DOTween.Sequence();
            
            foreach(var milestone in spawnedMilestones)
            {
                if (milestone.GetMilestoneLevel >= 0 && milestone.GetMilestoneLevel <= newMilestoneLevel)
                {
                    milestone.CheckUnlockForAll();
                }
            }

            checkedForUnlock = true;

            expAnimation_eachReward
                .Append(DG.Tweening.DOTweenModuleUI
                    .DOFillAmount(expBar_center, GetFillAmount_RewardVer(newMilestoneLevel), 0.75f)
                    .SetEase(Ease.Linear))
                .Join(DG.Tweening.DOTweenModuleUI
                    .DOAnchorPosY(
                        milestonesHolder_RectTransform,
                        CalculateNewScrollTarget(newMilestoneLevel),
                        0.75f).SetEase(Ease.OutQuad))
                .AppendCallback(delegate{if (expBar_center.fillAmount > 0) AudioManager.Instance.PlaySFX(SFXID.Star_3);});

            expAnimation_eachReward.OnComplete(delegate
            {
                expAnimation_eachReward = null;
            });
        }

        float CalculateNewScrollTarget(int targetLevel)
        {
            RectTransform targetItem = null;
            foreach (var milestone in spawnedMilestones)
            {
                if (milestone.GetMilestoneLevel == targetLevel)
                {
                    targetItem = milestone.MilestoneRectTransform;
                    break;
                }
            }

            if (targetItem == null) return -1;
            
            //RectTransform targetItem = content.GetChild(targetIndex) as RectTransform;
            float viewportHeight = scrollRect.viewport.rect.height;
    
            // Vị trí Y của item so với Content (thường là số âm nếu Pivot ở Top)
            float itemY = targetItem.anchoredPosition.y; 

            // Tính toán vị trí mới cho Content để item nằm giữa Viewport
            // Công thức: -itemY (để đưa nó lên 0) - (nửa chiều cao viewport để kéo nó xuống giữa)
            float targetY = -itemY - (viewportHeight / 2f);

            // Giới hạn không cho cuộn lố lề trên/dưới
            //float contentHeight = content.GetComponent<RectTransform>().rect.height;
            float contentHeight = milestonesHolder_RectTransform.rect.height;
            float maxY = contentHeight - viewportHeight;
            targetY = Mathf.Clamp(targetY, 0, maxY);

            return targetY;
        }

        void StopEXPBarAnimation()
        {
            if (expAnimation_top != null)
            {
                expAnimation_top.Complete();
                expAnimation_top = null;
            }
        }

        void StopRewardEXPAnimation()
        {
            if (expAnimation_eachReward != null)
            {
                expAnimation_eachReward.Complete();
                expAnimation_eachReward = null;
            }
        }
        
        void CancelBothAnimation()
        {
            StopEXPBarAnimation();
            StopRewardEXPAnimation();
            if (!checkedForUnlock)
            {
                foreach(var milestone in spawnedMilestones)
                {
                    if (milestone.GetMilestoneLevel >= lastTimeCheckedMilestoneLevel && milestone.GetMilestoneLevel <= newMilestoneLevel)
                    {
                        milestone.CheckUnlockForAll();
                    }
                }
            }
            
            lastTimeCheckedEXP = newEXP;
            lastTimeCheckedMilestoneLevel = newMilestoneLevel;
        }

        bool IsNewLevelMax()
        {
            if (newMilestoneLevel >= GameManager.Instance.battlepassData.GetNumberOfRewards())
                return true;
            return false;
        }

        bool IsThisMaxLevel(int level)
        {
            return level >= maxLevel;
        }

        int GetRequiredEXP(int level)
        {
            foreach (var data in milestones_Regular)
            {
                if (data.level == level)
                    return data.expRequirement;
            }

            return -1;
        }

        bool IsCurrentLevelMax(int level)
        {
            if (level >= maxLevel) return true;
            return false;
        }
        
        float GetFillAmount(int currentEXP, int requiredEXP)
        {
            if (currentEXP == 0) return 0;
            if (requiredEXP < 0) return 1;
            return (float)currentEXP / requiredEXP;
        }

        float GetFillAmount_RewardVer(int level)
        {
            if (level == 0) return 0;
            if (level == maxLevel) return 1;

            return (float)level / (levelDotCount - 1);
        }
        #endregion
        
        #region Instruction

        public void CloseInstruction()
        {
            CloseInstructionButtonVisibility(false);
            ShowInstruction(false);
        }

        public void ShowInstruction()
        {
            StopInstructionAnimation();
            instructionShown = true;
            showInstructionAnimation = DG.Tweening.DOTweenModuleUI.DOFade(instructionCanvasGroup, 1, 0.5f)
                .SetEase(Ease.Linear)
                .OnComplete(delegate
                {
                    showInstructionAnimation = null;
                    CloseInstructionButtonVisibility(true);
                });
        }

        public void ShowInstruction(bool closeMode)
        {
            StopInstructionAnimation();
            instructionShown = false;
            CloseInstructionButtonVisibility(false);
            showInstructionAnimation = DG.Tweening.DOTweenModuleUI.DOFade(instructionCanvasGroup, 0, 0.5f)
                .SetEase(Ease.Linear)
                .OnComplete(delegate
                {
                    showInstructionAnimation = null;
                });
        }

        void ShowInstruction(int noAnimationMode, bool status)
        {
            instructionShown = status;
            instructionCanvasGroup.alpha = status ? 1 : 0;
            CloseInstructionButtonVisibility(status);
        }
        
        void StopInstructionAnimation()
        {
            if (showInstructionAnimation != null)
            {
                showInstructionAnimation.Complete();
                showInstructionAnimation = null;
            }
        }

        void CloseInstructionButtonVisibility(bool status)
        {
            closeInstructionBtn.gameObject.SetActive(status);
        }
        
        #endregion
        
        #region Reset Progress Logic

        public void ResetProgress()
        {
            if (IsVisible()) Close(true);
            
            foreach (var milestone in spawnedMilestones)
            {
                milestone.ResetProgress();
            }
            ChangeBuyBattlepassButtonVisibility(true);

            lastTimeCheckedMilestoneLevel = -1;
            lastTimeCheckedEXP = 0;
            battlepassLevelText.SetText($"{lastTimeCheckedMilestoneLevel + 1}");
            expBar.fillAmount = 0;
            expBar_center.fillAmount = 0;
            nextLevelEXPRequirement = GetRequiredEXP(lastTimeCheckedMilestoneLevel + 1);
            battlepassProgressText.SetText($"{lastTimeCheckedEXP}/{nextLevelEXPRequirement}");
        }
        
        #endregion
        
        #region Timer Logic
        
        public void UpdateTimer(string time)
        {
            battlepassTimerText.SetText(time);
        }
        
        #endregion
        
        #region Premium Reward Effect

        void PlayAllPremiumRewardEffect()
        {
            foreach (var milestone in spawnedMilestones)
                milestone.PlayPremiumRewardEffect();
        }

        void StopAllPremiumRewardEffect()
        {
            foreach (var milestone in spawnedMilestones)
                milestone.StopPremiumRewardEffect();
        }
        
        #endregion
        
        #region Activate Premium Battlepass Button Animation

        void PlayBuyPremiumButtonAnimation()
        {
            StopBuyPremiumButtonAnimation();
            buyPremiumBattlepassButtonAnimation = DOTween.Sequence();

            buyPremiumBattlepassButtonAnimation
                .Append(buyBattlepassBtn.transform.DOLocalMoveY(buyPremiumBattlepassButtonOGPos.y + 15, 0.5f)
                    .SetEase(Ease.Linear))
                .Append(buyBattlepassBtn.transform.DOLocalMoveY(buyPremiumBattlepassButtonOGPos.y - 30, 0.5f)
                    .SetEase(Ease.OutBack))
                .Append(buyBattlepassBtn.transform.DOLocalMoveY(buyPremiumBattlepassButtonOGPos.y, 0.75f))
                .SetLoops(-1, LoopType.Restart);
        }

        void StopBuyPremiumButtonAnimation()
        {
            if (buyPremiumBattlepassButtonAnimation != null)
            {
                buyPremiumBattlepassButtonAnimation.Rewind();
                buyPremiumBattlepassButtonAnimation.Kill();
                buyPremiumBattlepassButtonAnimation = null;
            }
        }
        
        #endregion
    }
}
