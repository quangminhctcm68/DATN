using BMH.Ads;
using DG.Tweening;
using NabaGame.Core.Runtime.EventManager;
using NabaGame.Tracking;
using NabaGame.UI;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Sequence = DG.Tweening.Sequence;

namespace BrickEscape
{
    public class WinPanel : BaseUI
    {
        public Button NextLevel;
        public Button RewardBtn;
        public Button AvatarBtn;
        public Button SettingBtn;
        public Animator _animator;
        public TextMeshProUGUI LevelText;
        public Image Avatar;
        public Image Frame;
        public TextMeshProUGUI Coin;
        public TextMeshProUGUI Star;
        public TextMeshProUGUI PiggyAddCoinText;
        public TextMeshProUGUI BattlepassAddEXPText;
        public ShinyEffect shinyEffect;
        public PiggyAnimation piggyAnimation;
        public BattlepassAnimation battlepassAnimation;
        [SerializeField] private GameObject goldReward;
        [SerializeField] private GameObject battlepassReward;
        [SerializeField] private GameObject piggyReward;
        [SerializeField] private CanvasGroup goldReward_CanvasGroup;
        [SerializeField] private CanvasGroup battlepassReward_CanvasGroup;
        [SerializeField] private CanvasGroup piggyReward_CanvasGroup;
        [SerializeField] private CanvasGroup goldReward_Text_CanvasGroup;
        [SerializeField] private CanvasGroup battlepassReward_Text_CanvasGroup;
        [SerializeField] private CanvasGroup piggyReward_Text_CanvasGroup;
        [SerializeField] private bool addSuccessful = false;

        //[SerializeField] private bool isFullAfterward = false;
        [SerializeField] private int coinToAdd = 100;
        [SerializeField] private int expToAdd = 0;
        private PiggyBankProfile piggyBankProfile;
        private LevelGenerator levelGenerator;
        private Sequence rewardAnimationSequence;
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color highlightColor = Color.green;
        public override void OnInAnimationStart()
        {
            base.OnInAnimationStart();
            PlayAnim();
            HideRewards();
            UpdateCoin();
            UpdateStar();
            UpdatePiggy();
            UpdateBattlepass();
            AdManager.Instance.Hide(AdsType.Banner);
            shinyEffect.isActive = true;
            Frame.sprite =
                GameManager.Instance.spriteCollection.frameDic[GameManager.Instance.PlayerProfile.currentFrame];
            Avatar.sprite =
                GameManager.Instance.spriteCollection.avatarDic[GameManager.Instance.PlayerProfile.currentAvatar];
        }

        public override void OnOutAnimationStart()
        {
            base.OnOutAnimationStart();
            shinyEffect.isActive = true;
        }

        [Button]
        public void FakeShow()
        {
            Show();
        }

        public void SetInfor()
        {
            NextLevel.onClick.AddListener(Next);
            RewardBtn.onClick.AddListener(Reward);
            AvatarBtn.onClick.AddListener(Profile);
            SettingBtn.onClick.AddListener(Setting);
            if (piggyBankProfile == null) piggyBankProfile = GameManager.Instance.PlayerProfile.PiggyBankProfile;
            if (levelGenerator == null) levelGenerator = GameController.Instance.levelGenerator;
        }

        static readonly int StarCountHash = Animator.StringToHash("StarCount");
        static readonly int StarBlendHash = Animator.StringToHash("WinBlend");


        private int starCount;
        public void PlayAnim()
        {
            LevelText.text = "LEVEL " + (GameController.Instance.levelGenerator.levelID).ToString();
            float star = GameController.Instance.levelGenerator.HeartCount;
            _animator.SetFloat(StarCountHash, star);
            _animator.Play(StarBlendHash, 0, 0f);
            starCount = (int)star;

        }


        public void OnPlayCoinEffect()
        {
            int endValue = GameManager.Instance.PlayerProfile.Coin;
            int starValue = GameManager.Instance.PlayerProfile.Coin-10;
            PlayCoinCollectFX(starValue, endValue);
        }


        public void PlayCoinCollectFX(int startValue, int targetValue)
        {
            // kill tween cũ
            DOTween.Kill(Coin);
            DOTween.Kill(Coin.rectTransform);

            // reset scale tránh bị lệch khi spam
            Coin.rectTransform.localScale = Vector3.one;
            Coin.color = normalColor;
            var seq = DOTween.Sequence();

            // delay 1s sau khi VFX chạy
            seq.AppendInterval(1f);

            // 1. Scale text
            seq.Append(
                Coin.rectTransform
                .DOPunchScale(Vector3.one * 0.2f, 0.3f, 10, 0.8f)
            );


            // 2. Count up
            seq.Join(
                DOTween.To(
                    () => startValue,
                    x => Coin.text = x.ToString(),
                    targetValue,
                    0.4f
                ).SetEase(Ease.OutCubic)
            );

            // 3. Flash màu
            seq.Join(
                Coin.DOColor(highlightColor, 0.15f)
                    .SetLoops(2, LoopType.Yoyo)
            );
            seq.OnComplete(() => 
            { 
                Coin.text = targetValue.ToString();
                Coin.color = normalColor;

                UIMainManager.Instance.homePanel.UpdateCoin();


            });
        }

        public void PlayPiggyAnimation()
        {
            piggyAnimation.PlayPiggyAnimation();
        }

        public void HidePiggyAnimation()
        {
            piggyAnimation.HidePiggyAnimation();
        }

        public void HideBattlepassAnimation()
        {
            battlepassAnimation.HideBattlepassAnimation();
        }

        public void Setting()
        {
            UIMainManager.Instance.settingPanel.Show();
            GameController.Instance.audioManager.PlayButtonSound();
        }

        public void Profile()
        {
            UIMainManager.Instance.profilePanel.Show();
            GameController.Instance.audioManager.PlayButtonSound();
        }

        public void UpdateCoin()
        {
            int c = GameManager.Instance.PlayerProfile.Coin-10;
            Coin.text = FormatNumber.FormatNumberInt(c);
        }

        public void UpdateStar()
        {
            int s = GameManager.Instance.PlayerProfile.Star;
            Star.text = FormatNumber.FormatNumberInt(s);
        }

        public void UpdatePiggy()
        {
            addSuccessful = piggyBankProfile.IsAddMoreCoinPossible(coinToAdd);
            if (addSuccessful) piggyBankProfile.AddCoinsToPiggyBank(coinToAdd);
            //isFullAfterward = piggyBankProfile.IsPiggyBankFull();

            //if (addSuccessful && isFullAfterward) PiggyAnim(coinToAdd, true);
            if (addSuccessful) PiggyAddCoinText.SetText($"+{coinToAdd}");
            else PiggyAddCoinText.SetText($"<color=red>FULL</color>");
            UIMainManager.Instance.homePanel.ChangePiggyButtonVisibility(UIMainManager.Instance.piggyBankPanel
                .IsPiggyContainMinimumFund);
        }

        public void UpdateBattlepass()
        {
            if (levelGenerator.IsThreeStarsRating)
            {
                expToAdd = levelGenerator.IsCurrentLevelHard ? 2 : 1;
                ChangeRewardVisibility_Battlepass(true);
                BattlepassAddEXPText.SetText($"+{expToAdd}");
            }
            else
            {
                ChangeRewardVisibility_Battlepass(false);
            }
        }

        public void Reward()
        {
            GameController.Instance.audioManager.PlayButtonSound();

            AdManager.Instance.Show(AdsType.Rewarded, () =>
            {
                GameManager.Instance.PlayerProfile.QuestProfile.AddDailyWatchAdsCount(1);
                GameManager.Instance.PlayerProfile.ChangeCoin(100);
                OnNextLevelAnimComplete();
                TrackingManager.TrackEvent(TrackingEvent.Watched_Video,
                    TrackingParamter.Level, GameManager.Instance.PlayerProfile.LevelProfile.CurrentLevel.ToString(),
                    TrackingParamter.Type, "double_reward");
            }, "double_reward");




            UIMainManager.Instance.ratingPanel.CheckConditionWithOutInter();



            if (starCount == 3 && GameManager.Instance.PlayerProfile.JoinedRace)
            {
                GameManager.Instance.PlayerProfile.CurrentRaceLv++;
                UIMainManager.Instance.racePanel.Show();
                DOVirtual.DelayedCall(1, () =>
                {
                    UIMainManager.Instance.racePanel.JumpToCloud(GameManager.Instance.PlayerProfile.CurrentRaceLv);
                });
            }
        }
        

        public void Next()
        {
            GameController.Instance.audioManager.PlayButtonSound();
            Sequence seq = DOTween.Sequence();
            seq.AppendInterval(0.3f)
                .AppendCallback(() =>
                {
                    OnNextLevelAnimComplete();
                });

            UIMainManager.Instance.ratingPanel.CheckCondition();



            if (starCount == 3 && GameManager.Instance.PlayerProfile.JoinedRace)
            {
                GameManager.Instance.PlayerProfile.CurrentRaceLv++;
                UIMainManager.Instance.racePanel.Show();
                DOVirtual.DelayedCall(1, () =>
                {
                    UIMainManager.Instance.racePanel.JumpToCloud(GameManager.Instance.PlayerProfile.CurrentRaceLv);
                });
            }
        }

        public bool isNextLevel;
        private UpdateBoosterCount updateBoosterCount;
        public void OnNextLevelAnimComplete()
        {
            if (isNextLevel) 
            {
                GameController.Instance.levelGenerator.ClearEverything();
                Hide();
                HidePiggyAnimation();
                HideBattlepassAnimation();
                int currentLevelID = GameManager.Instance.PlayerProfile.LevelProfile.CurrentLevel;
                GameController.Instance.levelGenerator.StartGeneratingLevel(currentLevelID);
                UIMainManager.Instance.gamePlayPanel.SetText(currentLevelID);


                UIMainManager.Instance.gamePlayPanel.Open();

                TrackingManager.TrackEvent(TrackingEvent.StartLevel, TrackingParamter.Level, currentLevelID.ToString());
                if (updateBoosterCount == null) updateBoosterCount = new UpdateBoosterCount();
                EventManager.Instance.Raise(updateBoosterCount);
            }
            else 
            {
                GameController.Instance.levelGenerator.ClearEverything();

                Hide();
                UIMainManager.Instance.gamePlayPanel.Hide();
                UIMainManager.Instance.homePanel.Show();
                HidePiggyAnimation();
                HideBattlepassAnimation();
            }
        }






        #region Reward Animation

        public void StartRewardAnimation()
        {
            PlayRewardAnimation();
        }

        public void StartRewardAnimation_ThreeStarVer()
        {
            if (GameManager.Instance.PlayerProfile.LevelProfile.currentLevel >= GameController.Instance.battlepassManager.LevelUnlock) 
            {
                PlayRewardAnimation(true);
            }
            else 
            {
                PlayRewardAnimation();
                ChangeRewardVisibility_Battlepass(false);
            }
           
        }

        void PlayRewardAnimation()
        {
            StopRewardAnimation();
            rewardAnimationSequence = DOTween.Sequence();
            rewardAnimationSequence
                .Append(DG.Tweening.DOTweenModuleUI.DOFade(goldReward_CanvasGroup, 1, 0.4f).SetEase(Ease.Linear))
                .Join(DG.Tweening.DOTweenModuleUI.DOFade(goldReward_Text_CanvasGroup, 1, 0.2f).SetDelay(0.2f)
                    .SetEase(Ease.Linear))
                .Append(DG.Tweening.DOTweenModuleUI.DOFade(piggyReward_CanvasGroup, 1, 0.4f).SetEase(Ease.Linear))
                .Join(DG.Tweening.DOTweenModuleUI.DOFade(piggyReward_Text_CanvasGroup, 1, 0.2f).SetDelay(0.2f)
                    .SetEase(Ease.Linear));
            rewardAnimationSequence.OnComplete(delegate { rewardAnimationSequence = null; });
        }

        void PlayRewardAnimation(bool threeStarVer)
        {
            StopRewardAnimation();
            rewardAnimationSequence = DOTween.Sequence();
            rewardAnimationSequence
                .Append(DG.Tweening.DOTweenModuleUI.DOFade(goldReward_CanvasGroup, 1, 0.4f).SetEase(Ease.Linear))
                .Join(DG.Tweening.DOTweenModuleUI.DOFade(goldReward_Text_CanvasGroup, 1, 0.2f).SetDelay(0.2f)
                    .SetEase(Ease.Linear))
                .Append(DG.Tweening.DOTweenModuleUI.DOFade(battlepassReward_CanvasGroup, 1, 0.4f).SetEase(Ease.Linear))
                .Join(DG.Tweening.DOTweenModuleUI.DOFade(battlepassReward_Text_CanvasGroup, 1, 0.2f).SetDelay(0.2f)
                    .SetEase(Ease.Linear))
                .Append(DG.Tweening.DOTweenModuleUI.DOFade(piggyReward_CanvasGroup, 1, 0.4f).SetDelay(0.4f)
                    .SetEase(Ease.Linear))
                .Join(DG.Tweening.DOTweenModuleUI.DOFade(piggyReward_Text_CanvasGroup, 1, 0.2f).SetDelay(0.2f)
                    .SetEase(Ease.Linear));
            rewardAnimationSequence.OnComplete(delegate { rewardAnimationSequence = null; });
        }

        void StopRewardAnimation()
        {
            if (rewardAnimationSequence != null)
            {
                rewardAnimationSequence.Rewind();
                rewardAnimationSequence.Kill();
                rewardAnimationSequence = null;
            }
        }

        void ChangeRewardVisibility_Gold(bool status)
        {
            goldReward.SetActive(status);
        }

        void ChangeRewardVisibility_Battlepass(bool status)
        {
            battlepassReward.SetActive(status);
        }

        void ChangeRewardVisibility_Piggy(bool status)
        {
            piggyReward.SetActive(status);
        }

        void HideRewards()
        {
            goldReward_CanvasGroup.alpha = 0;
            battlepassReward_CanvasGroup.alpha = 0;
            piggyReward_CanvasGroup.alpha = 0;
            goldReward_Text_CanvasGroup.alpha = 0;
            battlepassReward_Text_CanvasGroup.alpha = 0;
            piggyReward_Text_CanvasGroup.alpha = 0;
        }

        #endregion
    }
}