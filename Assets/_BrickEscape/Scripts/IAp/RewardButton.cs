using BMH.Ads;
using System.Collections;
using System.Collections.Generic;
using NabaGame.Tracking;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BrickEscape
{
    public class RewardButton : MonoBehaviour
    {
        public Button rewardButton;
        public int Gold;
        private TextMeshProUGUI coinText;
        public void Start()
        {
            coinText = UIMainManager.Instance.homePanel.Coin;
        }
        public void OnClick()
        {
            GameController.Instance.audioManager.PlayButtonSound();
            AdManager.Instance.Show(AdsType.Rewarded, () =>
            {
                GameManager.Instance.PlayerProfile.QuestProfile.AddDailyWatchAdsCount(1);

                int startValue = GameManager.Instance.PlayerProfile.Coin;
                int endValue = startValue + Gold;
                UIMainManager.Instance.clickEffectPanel.PlayCoinCollectFX(coinText, startValue, endValue);
                GameManager.Instance.PlayerProfile.ChangeCoin(Gold);

                AudioManager.Instance.PlaySFX(SFXID.Coin);
                TrackingManager.TrackEvent(TrackingEvent.Watched_Video,
                    TrackingParamter .Level,GameManager.Instance.PlayerProfile.LevelProfile.CurrentLevel.ToString(),
                    TrackingParamter.Type, "free_gold");
            }, "free_gold");
        }
    }
}