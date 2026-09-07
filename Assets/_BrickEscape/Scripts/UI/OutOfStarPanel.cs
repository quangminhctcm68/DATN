using BMH.Ads;
using DG.Tweening;
using NabaGame.UI;
using System.Collections;
using System.Collections.Generic;
using NabaGame.Tracking;
using TMPro;
using UnityConstants;
using UnityEngine;
using UnityEngine.UI;

namespace BrickEscape
{
    public class OutOfStarPanel : BaseUI
    {
        public Button Reward;
        public Button Coin;
        public Button Close;
        public Image up;
        public Image down;
        public TextMeshProUGUI coinText;
        public AnimButton animBtn;
        public bool showWarning;
        public RectTransform content;

        public override void OnInAnimationStart()
        {
            base.OnInAnimationStart();
            if (GameManager.Instance.PlayerProfile.Coin < 1000)
            {
                up.color = GameManager.Instance.spriteCollection.ColorDict[ButtonColor.Gray_Up];
                down.color = GameManager.Instance.spriteCollection.ColorDict[ButtonColor.Gray_Down];
                coinText.color = Color.red;
            }
            else
            {
                up.color = GameManager.Instance.spriteCollection.ColorDict[ButtonColor.Green_Up];
                down.color = GameManager.Instance.spriteCollection.ColorDict[ButtonColor.Green_Down];
                coinText.color = Color.white;
            }

            UpdateCoin();
            animBtn.Play();
            Vector2 pos = content.anchoredPosition;
            pos.x = 0;
            content.anchoredPosition = pos;
            showWarning = false;
        }

        public TextMeshProUGUI _CoinText;

        public void UpdateCoin()
        {
            int c = GameManager.Instance.PlayerProfile.Coin;
            _CoinText.text = FormatNumber.FormatNumberInt(c);
        }

        public override void OnOutAnimationStart()
        {
            base.OnOutAnimationStart();
            animBtn.ResetAnim();
        }

        public void SetInfor()
        {
            Reward.onClick.AddListener(UseReward);
            Coin.onClick.AddListener(UseCoin);
            Close.onClick.AddListener(OnClose);
        }

        public void UseCoin()
        {
            GameController.Instance.audioManager.PlayButtonSound();
            if (GameManager.Instance.PlayerProfile.Coin < 1000) return;
            GameController.Instance.levelGenerator.Revive(true);
            GameManager.Instance.PlayerProfile.ChangeCoin(-1000);
            Hide();
            TrackingManager.TrackEvent("BuyStar", "Type", "Gold");
            UpdateCoin();
        }

        public void UseReward()
        {
            GameController.Instance.audioManager.PlayButtonSound();
            AdManager.Instance.Show(AdsType.Rewarded, () =>
            {
                GameManager.Instance.PlayerProfile.QuestProfile.AddDailyWatchAdsCount(1);
                GameController.Instance.levelGenerator.Revive(true);
                Hide();
                TrackingManager.TrackEvent(TrackingEvent.Watched_Video,
                    TrackingParamter .Level,GameManager.Instance.PlayerProfile.LevelProfile.CurrentLevel.ToString(),
                    TrackingParamter.Type, "revive");
            }, "revive");
        }

        public void OnClose()
        {
            if (!showWarning && !GameController.Instance.heartManager.IsUnlimitedHeartActive())
            {
                float TaskTargetX = -1000;
                DG.Tweening.DOTweenModuleUI
                    .DOAnchorPosX(content, TaskTargetX, 0.5f)
                    .SetEase(Ease.InOutBack);
                showWarning = true;
            }
            else
            {
                GameController.Instance.audioManager.PlayButtonSound();
                DOVirtual.DelayedCall(0.3f, () => { UIMainManager.Instance.failPanel.Show(); });
                GameController.Instance.heartManager.ReduceHeart();
                Hide();
            }
        }
    }
}