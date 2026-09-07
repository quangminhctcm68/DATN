using DG.Tweening;
using EasyTransition;
using NabaGame.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BrickEscape
{
    public class FailPanel : BaseUI
    {
        public Button Home;
        public Button Retry;
        public Image Avatar;
        public Image Frame;
        public Button AvatarBtn;
        public Button SettingBtn;
        public TextMeshProUGUI Coin;
        public TextMeshProUGUI Star;
        public void SetInfor() 
        {
            Home.onClick.AddListener(HomeBtn);
            Retry.onClick.AddListener(RetryBtn);
            AvatarBtn.onClick.AddListener(Profile);
            SettingBtn.onClick.AddListener(Setting);
        }
        public override void OnInAnimationStart()
        {
            base.OnInAnimationStart();
            UpdateCoin();
            UpdateStar();
            Frame.sprite = GameManager.Instance.spriteCollection.frameDic[GameManager.Instance.PlayerProfile.currentFrame];
            Avatar.sprite = GameManager.Instance.spriteCollection.avatarDic[GameManager.Instance.PlayerProfile.currentAvatar];
            PlayAnimShowSequence();
        }

        public void HomeBtn() 
        {
            UIMainManager.Instance.adsPanel.WatchAdsInter();
            //TransitionManager.Instance().Transition(
            //         UIMainManager.Instance.transition,
            //         0f
            //  );
            Sequence seq = DOTween.Sequence();
            seq.AppendInterval(0.3f)
               .AppendCallback(() =>
               {
                   GameController.Instance.levelGenerator.ClearEverything();
                   Hide();
                   UIMainManager.Instance.gamePlayPanel.Hide();
                   UIMainManager.Instance.homePanel.Show();
               });
            GameController.Instance.audioManager.PlayButtonSound();
            
        }

        public void RetryBtn() 
        {
            UIMainManager.Instance.adsPanel.WatchAdsInter();
            if (GameManager.Instance.PlayerProfile.HeartProfile.CurrentHeart <= 0 &&
                GameManager.Instance.PlayerProfile.HeartProfile.UnlimitedHeartRemainingTime <= 0)
            {
                UIMainManager.Instance.outOfHeartPanel.Open();
                return;
            }
            DOVirtual.DelayedCall(0.3f, () => {
                GameController.Instance.levelGenerator.PrepareLevelData();
                Hide();
            });
            GameController.Instance.audioManager.PlayButtonSound();
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
            int c = GameManager.Instance.PlayerProfile.Coin;
            Coin.text = FormatNumber.FormatNumberInt(c);
        }
        public void UpdateStar()
        {
            int s = GameManager.Instance.PlayerProfile.Star;
            Star.text = FormatNumber.FormatNumberInt(s);
        }
        public Transform[] Start;
        public Transform Level_fail;
        public Transform Icon;
        public Transform Buttons;
        public GameObject Top;



        public Sequence PlayAnimShowSequence()
        {
            Sequence seq = DOTween.Sequence();

            Top.SetActive(false);

            // Reset scale
            for (int i = 0; i < Start.Length; i++)
            {
                Start[i].localScale = Vector3.zero;
            }

            Level_fail.localScale = Vector3.zero;
            Icon.localScale = Vector3.zero;
            Buttons.localScale = Vector3.zero;

            // =========================
            // ⭐ STAR APPEAR + SHAKE
            // =========================
            for (int i = 0; i < Start.Length; i++)
            {
                Transform star = Start[i];

                seq.Append(star.DOScale(1f, 0.25f).SetEase(Ease.OutBack));

                // gọi shake sau khi scale xong
                seq.AppendCallback(() => Shake(star));

                // delay giữa các star
                seq.AppendInterval(0.05f);
            }

            // =========================
            // ⏱ DELAY
            // =========================
            seq.AppendInterval(0.5f);

            // =========================
            // 📦 OTHER UI
            // =========================

            seq.Append(Level_fail.DOScale(1f, 0.25f).SetEase(Ease.OutBack));
            seq.Append(Icon.DOScale(1f, 0.25f).SetEase(Ease.OutBack));
            seq.Append(Buttons.DOScale(1f, 0.25f).SetEase(Ease.OutBack));

            // =========================
            // 🔝 SHOW TOP
            // =========================
            seq.AppendCallback(() => Top.SetActive(true));

            return seq;
        }
        public void Shake(Transform trans)
        {
            trans.DOKill();

            trans.localScale = Vector3.one;

            Sequence seq = DOTween.Sequence();

            seq.Append(trans.DORotate(new Vector3(0, 0, 20), 0.08f))
               .Append(trans.DORotate(new Vector3(0, 0, -20), 0.08f))
               .Append(trans.DORotate(new Vector3(0, 0, 15), 0.08f))
               .Append(trans.DORotate(new Vector3(0, 0, -15), 0.08f))
               .Append(trans.DORotate(Vector3.zero, 0.05f));

        }

    }
}
