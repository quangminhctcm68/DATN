using AssetKits.ParticleImage;
using BMH.Ads;
using DG.Tweening;
using NabaGame.UI;
using System.Collections;
using System.Collections.Generic;
using NabaGame.Tracking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using JetBrains.Annotations;

namespace BrickEscape
{
    public class MoreBoosterPanel : BaseUI
    {
        public BoosterType boosterType;
        public Image type;
        public Button Reward;
        public Button UseCoin;
        public Button Close;
        public AnimButton animBtn;
        public Image up;
        public Image down;
        public TextMeshProUGUI text;
        public TextMeshProUGUI DesScription;
        public DOTweenAnimation anim;
        public ParticleImage[] particleImages;
        public RectTransform[] rectTransforms;
        public TextMeshProUGUI Title;
        [SerializeField] TextMeshProUGUI hint_Txt;
        [SerializeField] TextMeshProUGUI magicWand_Txt;
        [SerializeField] TextMeshProUGUI hammer_Txt;
        //public 
        public RectTransform BoosterPos;
        public override void OnInAnimationStart()
        {
            base.OnInAnimationStart();
            SetState();
            animBtn.Play();
            UpdateCoin();
            anim.DOPlay();
            foreach (var item in particleImages)
            {
                item.gameObject.SetActive(false);
            }

            foreach (var item in rectTransforms)
            {
                item?.gameObject.SetActive(false);
            }

            TrackingManager.TrackEvent(TrackingEvent.Booster_Event,
                TrackingParamter.Level, GameManager.Instance.PlayerProfile.LevelProfile.CurrentLevel.ToString(),
                TrackingParamter.Type, "open_panel");
        }

        public override void OnOutAnimationStart()
        {
            base.OnOutAnimationStart();
            animBtn.ResetAnim();
            anim.DOPause();
        }

        public void SetInfor()
        {
            Reward.onClick.AddListener(RewardBtn);
            UseCoin.onClick.AddListener(UseCoinBtn);
            Close.onClick.AddListener(CloseBtn);
        }

        public TextMeshProUGUI CoinText;

        public void UpdateCoin()
        {
            int c = GameManager.Instance.PlayerProfile.Coin;
            CoinText.text = FormatNumber.FormatNumberInt(c);
        }

        public void SetInfor(BoosterType bType)
        {
            boosterType = bType;
            type.sprite = GameManager.Instance.spriteCollection.BoostetDic[boosterType];
            SetDesScription();

        }

        public void SetState()
        {
            if (GameManager.Instance.PlayerProfile.Coin < 1000)
            {
                up.color = GameManager.Instance.spriteCollection.ColorDict[ButtonColor.Gray_Up];
                down.color = GameManager.Instance.spriteCollection.ColorDict[ButtonColor.Gray_Down];
                text.color = Color.red;
            }
            else
            {
                up.color = GameManager.Instance.spriteCollection.ColorDict[ButtonColor.Green_Up];
                down.color = GameManager.Instance.spriteCollection.ColorDict[ButtonColor.Green_Down];
                text.color = Color.white;
            }
        }

        public void RewardBtn()
        {
            AudioManager.Instance.PlayButtonSound();
            AdManager.Instance.Show(AdsType.Rewarded, () =>
            {
                OnComplete();
                GameManager.Instance.PlayerProfile.QuestProfile.AddDailyWatchAdsCount(1);
                // TrackingManager.TrackEvent("BuyBooster", "Reward", type.ToString());
                TrackingManager.TrackEvent(TrackingEvent.Booster_Event,
                    TrackingParamter.Level, GameManager.Instance.PlayerProfile.LevelProfile.CurrentLevel.ToString(),
                    TrackingParamter.Type, boosterType.ToString());
                TrackingManager.TrackEvent(TrackingEvent.Watched_Video,
                    TrackingParamter .Level,GameManager.Instance.PlayerProfile.LevelProfile.CurrentLevel.ToString(),
                    TrackingParamter.Type, boosterType.ToString());
            }, boosterType.ToString());
        }

        public void UseCoinBtn()
        {
            AudioManager.Instance.PlayButtonSound();
            if (GameManager.Instance.PlayerProfile.Coin < 1000) return;
            GameManager.Instance.PlayerProfile.ChangeCoin(-1000);
            OnComplete();
            // TrackingManager.TrackEvent("BuyBooster", "Gold", type.ToString());
            SetState();
            UpdateCoin();
            TrackingManager.TrackEvent(TrackingEvent.Booster_Event,
                TrackingParamter.Level, GameManager.Instance.PlayerProfile.LevelProfile.CurrentLevel.ToString(),
                TrackingParamter.Type, "used_coin");
        }

        public void CloseBtn()
        {
            AudioManager.Instance.PlayButtonSound();
            Hide();
            TrackingManager.TrackEvent(TrackingEvent.Booster_Event,
                TrackingParamter.Level, GameManager.Instance.PlayerProfile.LevelProfile.CurrentLevel.ToString(),
                TrackingParamter.Type, "close");
        }


        public void OnComplete()
        {
            switch (boosterType)
            {
                case BoosterType.Hammer:
                    GameManager.Instance.PlayerProfile.ChangeHammerBoosterUseCount(1);
                    
                    break;
                case BoosterType.Hint:
                    GameManager.Instance.PlayerProfile.ChangeHintBoosterUseCount(1);
                    break;
                case BoosterType.Magic:
                    GameManager.Instance.PlayerProfile.ChangeMagicWandBoosterUseCount(1);
                    break;
            }
            UpdateText();
            PlayEffect(boosterType);
        }

        public void SetDesScription()
        {
            switch (boosterType)
            {
                case BoosterType.Hammer:
                    Title.text = "HAMMER";
                    DesScription.text = "Choose one block to break";
                    break;
                case BoosterType.Hint:
                    Title.text = "HINT";
                    DesScription.text = "Show a block that can be removed";
                    break;
                case BoosterType.Magic:
                    Title.text = "MAGIC WAND";
                    DesScription.text = "Three random blocks disappear.";
                    break;
            }
            UpdateText();


        }

        public void UpdateText() 
        {
            magicWand_Txt.text = GameManager.Instance.PlayerProfile.MagicWandBoosterUseCount.ToString();
            hint_Txt.text = GameManager.Instance.PlayerProfile.HintBoosterUseCount.ToString();
            hammer_Txt.text = GameManager.Instance.PlayerProfile.HammerBoosterUseCount.ToString();

        }
        public void PlayEffect(BoosterType type)
        {
            particleImages[(int)type].gameObject.SetActive(true);
            particleImages[(int)type].Play();
            rectTransforms[(int)type].transform.localScale = Vector3.one;
            DOVirtual.DelayedCall(1.5f, () =>
            {
                rectTransforms[(int)type].gameObject.SetActive(true);
                ShortcutExtensions.DOScale(rectTransforms[(int)type].transform, Vector3.one * 1.2f, 0.25f)
                    .SetEase(Ease.OutQuad)
                    .OnComplete(() =>
                    {
                        DOVirtual.DelayedCall(0.25f,
                            () => { rectTransforms[(int)type].gameObject.SetActive(false); });
                    });
            });
        }
    }
}