using System.Collections;
using System.Collections.Generic;
using BMH.Ads;
using NabaGame.Tracking;
using NabaGame.UI;
using UnityEngine;

namespace BrickEscape
{
    public class OutOfHeartPanel : BaseUI
    {
        [SerializeField] private AnimButton animBtn;

        public override void OnInAnimationStart()
        {
            animBtn.Play();
        }

        public override void OnOutAnimationStart()
        {
            animBtn.ResetAnim();
        }

        public void Open()
        {
            Show();
        }

        public void Close()
        {
            Hide();
        }

        public void WatchAds()
        {
            AudioManager.Instance.PlayButtonSound();
            AdManager.Instance.Show(AdsType.Rewarded, () =>
            {
                GameManager.Instance.PlayerProfile.QuestProfile.AddDailyWatchAdsCount(1);
                OnWatchAdsComplete();
                TrackingManager.TrackEvent(TrackingEvent.Watched_Video,
                    TrackingParamter .Level,GameManager.Instance.PlayerProfile.LevelProfile.CurrentLevel.ToString(),
                    TrackingParamter.Type, "get_1_heart");
            }, $"get_1_heart");
        }

        void OnWatchAdsComplete()
        {
            GameController.Instance.heartManager.GainHeart();
            Close();
        }
    }
}