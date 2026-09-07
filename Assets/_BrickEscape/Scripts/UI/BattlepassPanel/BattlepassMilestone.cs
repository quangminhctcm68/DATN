using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BrickEscape
{
    public class BattlepassMilestone : MonoBehaviour
    {
        [SerializeField] private RectTransform milestoneRectTransform;
        
        [Space]
        [SerializeField] private ParticleSystem premiumRewardEffect;
        
        [Space]
        [SerializeField] private Image rewardIcon_typeRegular;
        [SerializeField] private TextMeshProUGUI rewardAmount_typeRegular;
        [SerializeField] private AnimButton receiveBtn_typeRegular;
        [SerializeField] private GameObject unlockNotification_Regular;
        [SerializeField] private bool notificationIsActive_Regular = true;
        
        [Space]
        [SerializeField] private Image rewardIcon_typePremium;
        [SerializeField] private TextMeshProUGUI rewardAmount_typePremium;
        [SerializeField] private AnimButton receiveBtn_typePremium;
        [SerializeField] private GameObject unlockNotification_Premium;
        [SerializeField] private bool notificationIsActive_Premium = true;

        [Space] 
        [SerializeField] private Image milestoneLevelIcon;
        [SerializeField] private TextMeshProUGUI milestoneLevelText;
        
        [Space]
        [SerializeField] private CanvasGroup rewardLockedMark_Regular;
        [SerializeField] private CanvasGroup rewardReceivedCheckmark_Regular;
        [SerializeField] private CanvasGroup rewardLockedMark_Premium;
        [SerializeField] private CanvasGroup rewardReceivedCheckmark_Premium;
        [SerializeField] private Vector3 lockOGPosition;
        [SerializeField] private Vector3 lockFallPosition;

        [Space] 
        [SerializeField] private BattlepassRewardData_Regular rewardData_Regular;
        [SerializeField] private BattlepassRewardData_Premium rewardData_Premium;

        [Space] 
        [SerializeField] private int milestoneLevel;
        [SerializeField] private int previousMilestoneEXPRequirement;
        [SerializeField] private int expGap;

        private DataCollection dataCollection;
        private BattlepassData battlepassData;
        private SpriteCollection spriteCollection;
        private BattlepassProfile battlepassProfile;
        
        [SerializeField] private BattlePassMilestoneData milestoneData_Regular;
        [SerializeField] private BattlePassPremiumMilestoneData milestoneData_Premium;

        private Sequence checkmarkAnimation;
        private Sequence lockAnimation;
        private Sequence notificationAnimation;
        private Sequence checkmarkAnimation_Premium;
        private Sequence lockAnimation_Premium;
        private Sequence notificationAnimation_Premium;

        #region Start, Update, Validate
        public RectTransform PremiumTrans;
        public RectTransform RegularTrans;



        public void SetMilestoneLevel(int level)
        {
            milestoneLevel = level;
            milestoneLevelText.SetText(milestoneLevel.ToString());
        }
        
        public void Init()
        {
            SetupData();
            SetupVisualizingData_Regular();
            SetupVisualizingData_Premium();
            CheckUnlockForAll(true);
            CheckRewardReceivedOnStartup();
            CheckLockOnStartup();
            PremiumTrans = rewardIcon_typePremium.GetComponent<RectTransform>();
            RegularTrans = rewardIcon_typeRegular.GetComponent<RectTransform>();
        }

        void SetupData()
        {
            if (dataCollection == null) dataCollection = GameManager.Instance.dataCollection;
            if (battlepassData == null) battlepassData = GameManager.Instance.battlepassData;
            if (spriteCollection == null) spriteCollection = GameManager.Instance.spriteCollection;
            if (battlepassProfile == null) battlepassProfile = GameManager.Instance.PlayerProfile.BattlepassProfile;
            
            rewardData_Regular = battlepassData.GetRegularRewardData(milestoneLevel);
            rewardData_Premium = battlepassData.GetPremiumRewardData(milestoneLevel);
            // if (milestoneLevel > 1) previousMilestoneEXPRequirement = battlepassData.GetRegularRewardData(milestoneLevel - 1).expRequirement;
            // else previousMilestoneEXPRequirement = 0;
            //expGap = rewardData_Regular.expRequirement - previousMilestoneEXPRequirement;
            
            milestoneData_Regular = battlepassProfile.GetMilestoneData(milestoneLevel);
            milestoneData_Premium = battlepassProfile.GetPremiumMilestoneData(milestoneLevel);
        }

        void SetupVisualizingData_Regular()
        {
            if (rewardData_Regular.rewardType == RewardType.Avatar_Icon)
                rewardIcon_typeRegular.sprite = spriteCollection.GetAvatarSprite(rewardData_Regular.avatarIconIDToUnlock);
            else if(rewardData_Regular.rewardType == RewardType.Avatar_Frame)
                rewardIcon_typeRegular.sprite = spriteCollection.GetFrameSprite(rewardData_Regular.avatarFrameIDToUnlock);
            else
                rewardIcon_typeRegular.sprite = dataCollection.GetRewardIcon(rewardData_Regular.rewardType);
                
            
            
            if (rewardData_Regular.rewardType == RewardType.Infinite_Heart) 
            {
                if (rewardData_Regular.infiniteHeartDurationToAdd >= 3600)
                    rewardAmount_typeRegular
                        .SetText($"{(float)rewardData_Regular.infiniteHeartDurationToAdd / 3600}h");
                else
                    rewardAmount_typeRegular
                        .SetText($"{(float)rewardData_Regular.infiniteHeartDurationToAdd / 60}m");
            }
               
            else if (rewardData_Regular.rewardType == RewardType.Avatar_Icon)
                rewardAmount_typeRegular.SetText("Avatar");
            else if (rewardData_Regular.rewardType == RewardType.Avatar_Frame)
                rewardAmount_typeRegular.SetText("Frame");
            else if (rewardData_Regular.rewardType == RewardType.Coin)
                rewardAmount_typeRegular.SetText($"+{rewardData_Regular.coinToAdd}");
            else
                rewardAmount_typeRegular.SetText($"+{rewardData_Regular.boosterUseCountToAdd}");
        }
        
        void SetupVisualizingData_Premium()
        {
            if (rewardData_Premium.rewardType == RewardType.Avatar_Icon)
                rewardIcon_typePremium.sprite = spriteCollection.GetAvatarSprite(rewardData_Premium.avatarIconIDToUnlock);
            else if(rewardData_Premium.rewardType == RewardType.Avatar_Frame)
                rewardIcon_typePremium.sprite = spriteCollection.GetFrameSprite(rewardData_Premium.avatarFrameIDToUnlock);
            else
                rewardIcon_typePremium.sprite = dataCollection.GetRewardIcon(rewardData_Premium.rewardType);
            
            if (rewardData_Premium.rewardType == RewardType.Infinite_Heart) 
            {
                if (rewardData_Premium.infiniteHeartDurationToAdd >= 3600)
                    rewardAmount_typePremium
                        .SetText($"{(float)rewardData_Premium.infiniteHeartDurationToAdd / 3600}h");
                else
                    rewardAmount_typePremium
                        .SetText($"{(float)rewardData_Premium.infiniteHeartDurationToAdd / 60}m");
            }
                
            else if (rewardData_Premium.rewardType == RewardType.Avatar_Icon)
                rewardAmount_typePremium.SetText("Avatar");
            else if (rewardData_Premium.rewardType == RewardType.Avatar_Frame)
                rewardAmount_typePremium.SetText("Frame");
            else if (rewardData_Premium.rewardType == RewardType.Coin)
                rewardAmount_typePremium.SetText($"+{rewardData_Premium.coinToAdd}");
            else
                rewardAmount_typePremium.SetText($"+{rewardData_Premium.boosterUseCountToAdd}");
        }
        
        #endregion
        
        #region Check For Unlock

        public void CheckUnlockForAll()
        {
            CheckUnlock_Regular();
            CheckUnlock_Premium();
        }
        
        public void CheckUnlockForAll(bool startupVer)
        {
            CheckUnlock_Regular(true);
            CheckUnlock_Premium(true);
        }

        void CheckUnlock_Regular()
        {
            if (milestoneData_Regular.unlockStatus && !milestoneData_Regular.claimedStatus)
            {
                ReceiveButton_Visibility(true);
                ReceiveButton_Animation(true);
                if (!notificationIsActive_Regular) NotificationAppear();
            }
            else
            {
                ReceiveButton_Visibility(false);
                ReceiveButton_Animation(false);
                if (notificationIsActive_Regular) NotificationDisappear();
            }
        }
        
        void CheckUnlock_Regular(bool startupVer)
        {
            if (milestoneData_Regular.unlockStatus && !milestoneData_Regular.claimedStatus)
            {
                ReceiveButton_Visibility(true);
                ReceiveButton_Animation(true);
                if (!notificationIsActive_Regular) NotificationAppear(1);
            }
            else
            {
                ReceiveButton_Visibility(false);
                ReceiveButton_Animation(false);
                if (notificationIsActive_Regular) NotificationDisappear(1);
            }
        }

        void CheckUnlock_Premium()
        {
            if (milestoneData_Premium.unlockStatus && !milestoneData_Premium.claimedStatus && battlepassProfile.IsBattlePassPurchased)
            {
                ReceiveButton_Visibility(true, true);
                ReceiveButton_Animation(true, true);
                if (!notificationIsActive_Premium) NotificationAppear(true);
            }
            else
            {
                ReceiveButton_Visibility(false, true);
                ReceiveButton_Animation(false, true);
                if (notificationIsActive_Premium) NotificationDisappear(true);
            }
        }
        
        void CheckUnlock_Premium(bool startupVer)
        {
            if (milestoneData_Premium.unlockStatus && !milestoneData_Premium.claimedStatus && battlepassProfile.IsBattlePassPurchased)
            {
                ReceiveButton_Visibility(true, true);
                ReceiveButton_Animation(true, true);
                if (!notificationIsActive_Premium) NotificationAppear(1, true);
            }
            else
            {
                ReceiveButton_Visibility(false, true);
                ReceiveButton_Animation(false, true);
                if (notificationIsActive_Premium) NotificationDisappear(1, true);
            }
        }
        
        #endregion
        
        #region Button Animation/Visibility

        void ReceiveButton_Animation(bool status)
        {
            if (status) receiveBtn_typeRegular.Play();
            else receiveBtn_typeRegular.ResetAnim();
        }

        void ReceiveButton_Visibility(bool status)
        {
            receiveBtn_typeRegular.gameObject.SetActive(status);
        }
        
        void ReceiveButton_Animation(bool status, bool premiumVer)
        {
            if (status) receiveBtn_typePremium.Play();
            else receiveBtn_typePremium.ResetAnim();
        }

        void ReceiveButton_Visibility(bool status, bool premiumVer)
        {
            receiveBtn_typePremium.gameObject.SetActive(status);
        }
        
        #endregion
        
        #region Receive Reward Action
        
        public void ReceiveRewardAction_Regular()
        {
            AudioManager.Instance.PlayButtonSound();
            GameManager.Instance.PlayerProfile.BattlepassProfile.UpdateClaimedMilestone(milestoneLevel, true);
            ProcessReward(rewardData_Regular.rewardType);
            ReceiveButton_Animation(false);
            ReceiveButton_Visibility(false);
            CheckMarkVisibility(true, false, true);
        }
        
        public void ReceiveRewardAction_Premium()
        {
            AudioManager.Instance.PlayButtonSound();
            GameManager.Instance.PlayerProfile.BattlepassProfile.UpdateClaimedPremiumMilestone(milestoneLevel, true);
            ProcessReward(rewardData_Premium.rewardType, true);
            ReceiveButton_Animation(false, true);
            ReceiveButton_Visibility(false, true);
            CheckMarkVisibility(true, true, true);
        }

        void ProcessReward(RewardType rewardType)
        {
            //Show Reward
            int qty =0;



            //switch (rewardType)
            //{
            //    case RewardType.Coin:
            //        UIMainManager.Instance.rewardGetPanel.SetData(RewardType.Coin, rewardData_Regular.coinToAdd);
            //        UIMainManager.Instance.rewardGetPanel.Open();
            //        break;
            //    case RewardType.Hint_Booster or RewardType.Hammer_Booster or RewardType.MagicWand_Booster:
            //        UIMainManager.Instance.rewardGetPanel.SetData(rewardType, rewardData_Regular.boosterUseCountToAdd);
            //        UIMainManager.Instance.rewardGetPanel.Open();
            //        break;
            //    case RewardType.Infinite_Heart:
            //        UIMainManager.Instance.rewardGetPanel.SetData(RewardType.Infinite_Heart, rewardData_Regular.infiniteHeartDurationToAdd);
            //        UIMainManager.Instance.rewardGetPanel.Open();
            //        break;
            //    case RewardType.Avatar_Icon:
            //        UIMainManager.Instance.rewardGetPanel.SetData(rewardData_Regular.avatarIconIDToUnlock);
            //        UIMainManager.Instance.rewardGetPanel.Open();
            //        break;
            //    case RewardType.Avatar_Frame:
            //        UIMainManager.Instance.rewardGetPanel.SetData(rewardData_Regular.avatarFrameIDToUnlock);
            //        UIMainManager.Instance.rewardGetPanel.Open();
            //        break;
            //}
            
            //Process Reward
            switch (rewardType)
            {
                case RewardType.Coin:
                    GameManager.Instance.PlayerProfile.ChangeCoin(rewardData_Regular.coinToAdd);
                    qty = rewardData_Regular.coinToAdd;
                    break;
                case RewardType.Hint_Booster:
                    GameManager.Instance.PlayerProfile.ChangeHintBoosterUseCount(rewardData_Regular.boosterUseCountToAdd);
                    qty = rewardData_Regular.boosterUseCountToAdd;
                    break;
                case RewardType.Hammer_Booster:
                    GameManager.Instance.PlayerProfile.ChangeHammerBoosterUseCount(rewardData_Regular.boosterUseCountToAdd);
                    qty = rewardData_Regular.boosterUseCountToAdd;
                    break;
                case RewardType.MagicWand_Booster:
                    GameManager.Instance.PlayerProfile.ChangeMagicWandBoosterUseCount(rewardData_Regular.boosterUseCountToAdd);
                    qty = rewardData_Regular.boosterUseCountToAdd;
                    break;
                case RewardType.Infinite_Heart:
                    GameController.Instance.heartManager.AddUnlimitedHeartTime(rewardData_Regular.infiniteHeartDurationToAdd);
                    qty = rewardData_Regular.infiniteHeartDurationToAdd;
                    break;
                case RewardType.Avatar_Icon:
                    GameManager.Instance.profileData.ForceUnlock_Avatar(rewardData_Regular.avatarIconIDToUnlock);
                    qty = (int)rewardData_Regular.avatarIconIDToUnlock;
                    break;
                case RewardType.Avatar_Frame:
                    GameManager.Instance.profileData.ForceUnlock_Frame(rewardData_Regular.avatarFrameIDToUnlock);
                    qty = (int)rewardData_Regular.avatarFrameIDToUnlock;
                    break;
            }
            UIMainManager.Instance.clickEffectPanel.PlayHightLight(rewardType,qty,RegularTrans);

        }
        
        void ProcessReward(RewardType rewardType, bool premiumVer)
        {
            ////Show Reward
            //switch (rewardType)
            //{
            //    case RewardType.Coin:
            //        UIMainManager.Instance.rewardGetPanel.SetData(RewardType.Coin, rewardData_Premium.coinToAdd);
            //        UIMainManager.Instance.rewardGetPanel.Open();
            //        break;
            //    case RewardType.Hint_Booster or RewardType.Hammer_Booster or RewardType.MagicWand_Booster:
            //        UIMainManager.Instance.rewardGetPanel.SetData(rewardType, rewardData_Premium.boosterUseCountToAdd);
            //        UIMainManager.Instance.rewardGetPanel.Open();
            //        break;
            //    case RewardType.Infinite_Heart:
            //        UIMainManager.Instance.rewardGetPanel.SetData(RewardType.Infinite_Heart, rewardData_Premium.infiniteHeartDurationToAdd);
            //        UIMainManager.Instance.rewardGetPanel.Open();
            //        break;
            //    case RewardType.Avatar_Icon:
            //        UIMainManager.Instance.rewardGetPanel.SetData(rewardData_Premium.avatarIconIDToUnlock);
            //        UIMainManager.Instance.rewardGetPanel.Open();
            //        break;
            //    case RewardType.Avatar_Frame:
            //        UIMainManager.Instance.rewardGetPanel.SetData(rewardData_Premium.avatarFrameIDToUnlock);
            //        UIMainManager.Instance.rewardGetPanel.Open();
            //        break;
            //}

            int qty = 0;
            //Process Reward
            switch (rewardType)
            {
                case RewardType.Coin:
                    GameManager.Instance.PlayerProfile.ChangeCoin(rewardData_Premium.coinToAdd);
                    qty = rewardData_Premium.coinToAdd;
                    break;
                case RewardType.Hint_Booster:
                    GameManager.Instance.PlayerProfile.ChangeHintBoosterUseCount(rewardData_Premium.boosterUseCountToAdd);
                    qty = rewardData_Premium.boosterUseCountToAdd;
                    break;
                case RewardType.Hammer_Booster:
                    GameManager.Instance.PlayerProfile.ChangeHammerBoosterUseCount(rewardData_Premium.boosterUseCountToAdd);
                    qty = rewardData_Premium.boosterUseCountToAdd;
                    break;
                case RewardType.MagicWand_Booster:
                    GameManager.Instance.PlayerProfile.ChangeMagicWandBoosterUseCount(rewardData_Premium.boosterUseCountToAdd);
                    qty = rewardData_Premium.boosterUseCountToAdd;
                    break;
                case RewardType.Infinite_Heart:
                    GameController.Instance.heartManager.AddUnlimitedHeartTime(rewardData_Premium.infiniteHeartDurationToAdd);
                    qty = rewardData_Premium.infiniteHeartDurationToAdd;
                    break;
                case RewardType.Avatar_Icon:
                    GameManager.Instance.profileData.ForceUnlock_Avatar(rewardData_Premium.avatarIconIDToUnlock);
                    qty = (int)rewardData_Premium.avatarIconIDToUnlock;
                    break;
                case RewardType.Avatar_Frame:
                    GameManager.Instance.profileData.ForceUnlock_Frame(rewardData_Premium.avatarFrameIDToUnlock);
                    qty = (int)rewardData_Premium.avatarFrameIDToUnlock;
                    break;
            }
            
            UIMainManager.Instance.clickEffectPanel.PlayHightLight(rewardType, qty,PremiumTrans);
        }
        
        #endregion
        
        #region Lock/Receive Checkmark Visual

        //Checkmark
        void CheckRewardReceivedOnStartup()
        {
            CheckMarkVisibility(milestoneData_Regular.claimedStatus, false, false);
            CheckMarkVisibility(milestoneData_Premium.claimedStatus, true, false);
        }
        
        void CheckMarkVisibility(bool status, bool isPremiumReward, bool isAnimated)
        {
            if (status)
            {
                if (isAnimated)
                {
                    if (isPremiumReward)
                    {
                        RewardReceivedCheckMark(true);
                    }
                    else
                    {
                        RewardReceivedCheckMark();
                    }
                }
                else
                {
                    if (isPremiumReward)
                    {
                        RewardReceivedCheckMark(0, true);
                    }
                    else
                    {
                        RewardReceivedCheckMark(0);
                    }
                }
            }
            else
            {
                if (isPremiumReward)
                {
                    DisableRewardReceivedCheckMark(true);
                }
                else
                {
                    DisableRewardReceivedCheckMark();
                }
            }
        }
        
        void DisableRewardReceivedCheckMark()
        {
            rewardReceivedCheckmark_Regular.alpha = 0;
        }
        
        void DisableRewardReceivedCheckMark(bool premiumReward)
        {
            rewardReceivedCheckmark_Premium.alpha = 0;
        }
        
        void RewardReceivedCheckMark()
        {
            StopCheckmarkAnimation();
            checkmarkAnimation = DOTween.Sequence();

            checkmarkAnimation
                .AppendCallback(delegate
                {
                    DisableRewardReceivedCheckMark();
                    rewardReceivedCheckmark_Regular.transform.localScale = Vector3.one * 1.5f;
                })
                .Append(rewardReceivedCheckmark_Regular.DOFade(1, 0.3f).SetEase(Ease.Linear))
                .Join(rewardReceivedCheckmark_Regular.transform.DOScale(Vector3.one, 0.5f).SetDelay(0.15f)
                    .SetEase(Ease.OutBack));

            checkmarkAnimation.OnComplete(delegate
            {
                checkmarkAnimation = null;
            });
        }
        
        void RewardReceivedCheckMark(bool premiumReward)
        {
            StopCheckmarkAnimation(true);
            checkmarkAnimation_Premium = DOTween.Sequence();
            
            checkmarkAnimation_Premium
                .AppendCallback(delegate
                {
                    DisableRewardReceivedCheckMark(true);
                    rewardReceivedCheckmark_Premium.transform.localScale = Vector3.one * 1.5f;
                })
                .Append(rewardReceivedCheckmark_Premium.DOFade(1, 0.3f).SetEase(Ease.Linear))
                .Join(rewardReceivedCheckmark_Premium.transform.DOScale(Vector3.one, 0.5f).SetDelay(0.15f)
                    .SetEase(Ease.OutBack));
            
            checkmarkAnimation_Premium.OnComplete(delegate
            {
                checkmarkAnimation_Premium = null;
            });
        }
        
        void RewardReceivedCheckMark(int noAnimationVer)
        {
            rewardReceivedCheckmark_Regular.alpha = 1;
        }
        
        void RewardReceivedCheckMark(int noAnimationVer, bool premiumReward)
        {
            rewardReceivedCheckmark_Premium.alpha = 1;
        }

        void StopCheckmarkAnimation()
        {
            if (checkmarkAnimation != null)
            {
                checkmarkAnimation.Rewind();
                checkmarkAnimation.Kill();
                checkmarkAnimation = null;
            }
        }
        
        void StopCheckmarkAnimation(bool premiumReward)
        {
            if (checkmarkAnimation_Premium != null)
            {
                checkmarkAnimation_Premium.Rewind();
                checkmarkAnimation_Premium.Kill();
                checkmarkAnimation_Premium = null;
            }
        }
        
        //Lock

        public void UnlockPremiumReward()
        {
            LockMarkVisibility(false, true, true);
        }

        void CheckLockOnStartup()
        {
            LockMarkVisibility(false, false, false);
            if (!battlepassProfile.IsBattlePassPurchased)
            {
                LockMarkVisibility(true, true, false);
            }
            else
            {
                LockMarkVisibility(false, true, false);
            }
        }
        
        void LockMarkVisibility(bool status, bool isPremiumReward, bool isAnimated)
        {
            if (!status)
            {
                if (isAnimated)
                {
                    if (isPremiumReward)
                    {
                        LockMark(true);
                    }
                    else
                    {
                        LockMark();
                    }
                }
                else
                {
                    if (isPremiumReward)
                    {
                        DisableLockMark(true);
                    }
                    else
                    {
                        DisableLockMark();
                    }
                }
            }
            else
            {
                if (isPremiumReward)
                {
                    LockMark(0, true);
                }
                else
                {
                    LockMark(0);
                }
            }
        }
        
        void DisableLockMark()
        {
            rewardLockedMark_Regular.alpha = 0;
        }
        
        void DisableLockMark(bool premiumReward)
        {
            rewardLockedMark_Premium.alpha = 0;
        }
        
        void LockMark()
        {
            StopLockMarkAnimation();
            lockAnimation = DOTween.Sequence();

            lockAnimation
                .AppendCallback(delegate
                {
                    LockMark(0);
                })
                .Append(rewardLockedMark_Regular.DOFade(0f, 0.3f).SetEase(Ease.Linear))
                .Join(rewardLockedMark_Regular.transform.DOLocalMoveY(lockFallPosition.y, 0.5f));

            lockAnimation.OnComplete(delegate
            {
                lockAnimation = null;
            });
        }
        
        void LockMark(bool premiumReward)
        {
            StopLockMarkAnimation(true);
            lockAnimation_Premium = DOTween.Sequence();
            
            lockAnimation_Premium
                .AppendCallback(delegate
                {
                    LockMark(0, true);
                })
                .Append(rewardLockedMark_Premium.DOFade(0f, 0.3f).SetEase(Ease.Linear))
                .Join(rewardLockedMark_Premium.transform.DOLocalMoveY(lockFallPosition.y, 0.5f));

            lockAnimation_Premium.OnComplete(delegate
            {
                lockAnimation_Premium = null;
            });
        }
        
        void LockMark(int noAnimationVer)
        {
            rewardLockedMark_Regular.transform.localPosition = Vector3.zero;
            rewardLockedMark_Regular.alpha = 1;
        }
        
        void LockMark(int noAnimationVer, bool premiumReward)
        {
            rewardLockedMark_Premium.transform.localPosition = Vector3.zero;
            rewardLockedMark_Premium.alpha = 1;
        }
        
        void StopLockMarkAnimation()
        {
            if (lockAnimation != null)
            {
                lockAnimation.Rewind();
                lockAnimation.Kill();
                lockAnimation = null;
            }
        }
        
        void StopLockMarkAnimation(bool premiumReward)
        {
            if (lockAnimation_Premium != null)
            {
                lockAnimation_Premium.Rewind();
                lockAnimation_Premium.Kill();
                lockAnimation_Premium = null;
            }
        }
        
        #endregion
        
        #region Unlock Notification Visual

        void NotificationAppear()
        {
            StopNotificationAnimation();
            notificationAnimation = DOTween.Sequence();

            notificationIsActive_Regular = true;
            
            notificationAnimation
                .AppendCallback(delegate
                {
                    unlockNotification_Regular.transform.localScale = Vector3.zero;
                })
                .Append(unlockNotification_Regular.transform.DOScale(1, 0.5f)).SetEase(Ease.Linear);
            
            notificationAnimation.OnComplete(delegate
            {
                notificationAnimation = null;
            });
        }

        void NotificationAppear(bool premiumVer)
        {
            StopNotificationAnimation(true);
            notificationAnimation_Premium = DOTween.Sequence();
            
            notificationIsActive_Premium = true;
            
            notificationAnimation_Premium
                .AppendCallback(delegate
                {
                    unlockNotification_Premium.transform.localScale = Vector3.zero;
                })
                .Append(unlockNotification_Premium.transform.DOScale(1, 0.5f)).SetEase(Ease.Linear);
            
            notificationAnimation_Premium.OnComplete(delegate
            {
                notificationAnimation_Premium = null;
            });
        }

        void NotificationAppear(int noAnimationVer)
        {
            notificationIsActive_Regular = true;
            unlockNotification_Regular.transform.localScale = Vector3.one;
        }

        void NotificationAppear(int noAnimationVer, bool premiumReward)
        {
            notificationIsActive_Premium = true;
            unlockNotification_Premium.transform.localScale = Vector3.one;
        }

        void NotificationDisappear()
        {
            StopNotificationAnimation();
            notificationAnimation = DOTween.Sequence();
            
            notificationIsActive_Regular = false;
            
            notificationAnimation
                .AppendCallback(delegate
                {
                    unlockNotification_Regular.transform.localScale = Vector3.one;
                })
                .Append(unlockNotification_Regular.transform.DOScale(0, 0.5f)).SetEase(Ease.Linear);
            
            notificationAnimation.OnComplete(delegate
            {
                notificationAnimation = null;
            });
        }

        void NotificationDisappear(bool premiumVer)
        {
            StopNotificationAnimation(true);
            notificationAnimation_Premium = DOTween.Sequence();
            
            notificationIsActive_Premium = false;
            
            notificationAnimation_Premium
                .AppendCallback(delegate
                {
                    unlockNotification_Premium.transform.localScale = Vector3.one;
                })
                .Append(unlockNotification_Premium.transform.DOScale(0, 0.5f)).SetEase(Ease.Linear);
            
            notificationAnimation_Premium.OnComplete(delegate
            {
                notificationAnimation_Premium = null;
            });
        }

        void NotificationDisappear(int noAnimationVer)
        {
            notificationIsActive_Regular = false;
            unlockNotification_Regular.transform.localScale = Vector3.zero;
        }

        void NotificationDisappear(int noAnimationVer, bool premiumReward)
        {
            notificationIsActive_Premium = false;
            unlockNotification_Premium.transform.localScale = Vector3.zero;
        }

        void StopNotificationAnimation()
        {
            if (notificationAnimation != null)
            {
                notificationAnimation.Rewind();
                notificationAnimation.Kill();
                notificationAnimation = null;
            }
        }

        void StopNotificationAnimation(bool premiumVer)
        {
            if (notificationAnimation_Premium != null)
            {
                notificationAnimation_Premium.Rewind();
                notificationAnimation_Premium.Kill();
                notificationAnimation_Premium = null;
            }
        }
        
        #endregion
        
        #region Reset Progress Logic

        public void ResetProgress()
        {
            //LockMark(0);
            LockMark(0, true);
            DisableRewardReceivedCheckMark();
            DisableRewardReceivedCheckMark(true);
            
            ReceiveButton_Visibility(false);
            ReceiveButton_Animation(false);
            
            ReceiveButton_Visibility(false, true);
            ReceiveButton_Animation(false, true);
        }
        
        #endregion
        
        #region Premium Reward Effect Logic

        public void PlayPremiumRewardEffect()
        {
            premiumRewardEffect.Play();
        }

        public void StopPremiumRewardEffect()
        {
            premiumRewardEffect.Stop();
        }
        
        #endregion
        
        #region Getters, Setters
        
        public Image MilestoneLevelIcon => milestoneLevelIcon;
        public int PreviousMilestoneEXPRequirement => previousMilestoneEXPRequirement;
        public int EXPGap => expGap;
        public int GetMilestoneLevel => milestoneLevel;
        public RectTransform MilestoneRectTransform => milestoneRectTransform;

        #endregion
    }
}
