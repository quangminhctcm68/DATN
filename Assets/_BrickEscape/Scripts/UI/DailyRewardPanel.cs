using NabaGame.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using NabaGame.Tracking;
using UnityEngine;
using UnityEngine.UI;

namespace BrickEscape
{
    public class DailyRewardPanel : BaseUI
    {
        public Button _ClaimBtn;
        public Button _CloseBtn;
        public GameObject DotClaim;
        public GameObject NotifyInHome;
        public AnimButton animBtn;
        private PlayerProfile _profile;
        private GameManager _gameManager;
        [SerializeField] int today;
        [SerializeField] List<GiftItem> item;

        public void SetInfor()
        {
            _gameManager = GameManager.Instance;
            _profile = GameManager.Instance.PlayerProfile;
            _ClaimBtn.onClick.AddListener(Claim);
            _CloseBtn.onClick.AddListener(() =>
            {
                Hide();
                AudioManager.Instance.PlayButtonSound();
            });
            NotifyInHome = UIMainManager.Instance.homePanel.DailyRewardNotify;

            CheckDay();
            GenRewardPack();
            NotifyInHome.gameObject.SetActive(!HasClaimedToday());
        }

        private int _cachedToday = -1;

        void Update()
        {
            if (_gameManager == null || !IsVisible())
                return;
            int todayNow = _gameManager.GetToday();

            // nếu ngày thay đổi
            if (todayNow != _cachedToday)
            {
                _cachedToday = todayNow;
                CheckDay();
                Check();
            }
        }

        public override void OnInAnimationStart()
        {
            base.OnInAnimationStart();
            CheckDay();
            GenRewardPack();
            Check();
            TrackingManager.TrackEvent(TrackingEvent.Daily_Reward,
                TrackingParamter.Type, "open",
                TrackingParamter.Level, GameManager.Instance.PlayerProfile.LevelProfile.CurrentLevel.ToString());
        }

        public override void OnOutAnimationStart()
        {
            base.OnOutAnimationStart();
            CheckDay();
        }

        public void GenRewardPack()
        {
            List<RawDailyReward> pack = _gameManager.cookedDailyReward.dailyRewardPacks[_profile.rewardPackIndex]
                .dailyRewards;
            for (int i = 0; i < pack.Count; i++)
            {
                item[i].SetInfor(pack[i]);
            }
        }

        public void Check()
        {
            if (!HasClaimedToday())
            {
                animBtn.Play();
                animBtn.SetColor(ButtonColor.Green_Up, ButtonColor.Green_Down, FontColor.Green);
            }
            else
            {
                animBtn.SetColor(ButtonColor.Gray_Up, ButtonColor.Gray_Down, FontColor.Black);
                animBtn.ResetAnim();
            }

            NotifyInHome.gameObject.SetActive(!HasClaimedToday());
            DotClaim.gameObject.SetActive(!HasClaimedToday());
        }

        public void Claim()
        {
            if (HasClaimedToday())
                return;
            AudioManager.Instance.PlayButtonSound();
            int today = _gameManager.GetToday();
            _profile.lastClaimDay = today;
            _profile.rewardDailyIndex++;
            item[_profile.rewardDailyIndex - 1].GiveReward();

            //item[_profile.rewardDailyIndex - 1].GiveReward();
            NotifyInHome.gameObject.SetActive(!HasClaimedToday());
            DotClaim.gameObject.SetActive(!HasClaimedToday());
            animBtn.SetColor(ButtonColor.Gray_Up, ButtonColor.Gray_Down, FontColor.Black);
            animBtn.ResetAnim();
            GameManager.Instance.PlayerProfile.trackingDailyReward++;
            int count = PlayerPrefs.GetInt("c_daily_reward", 0);
            count++;
            PlayerPrefs.SetInt("c_daily_reward",count);
            TrackingManager.TrackEvent(TrackingEvent.Daily_Reward,
                TrackingParamter.Type, "claimed",
                TrackingParamter.Level, GameManager.Instance.PlayerProfile.LevelProfile.CurrentLevel.ToString()
            );
            
            TrackingManager.TrackEvent(TrackingEvent.Claim_Daily_Reward,
                TrackingParamter.Level, GameManager.Instance.PlayerProfile.LevelProfile.CurrentLevel.ToString(),
                TrackingParamter.Count, count.ToString());
            Hide();
        }

        public void CheckDay()
        {
            if (_gameManager == null)
            {
                _gameManager = GameManager.Instance;
            }

            today = _gameManager.GetToday();
            if (!HasClaimedToday())
            {
                OnNewDay();
            }
        }

        public bool HasClaimedToday()
        {
            if (_gameManager == null)
            {
                _gameManager = GameManager.Instance;
            }

            int today = _gameManager.GetToday();
            return _profile.lastClaimDay == today;
        }

        public bool CanClaim()
        {
            int today = _gameManager.GetToday();
            return today != _profile.lastClaimDay;
        }

        void OnNewDay()
        {
            animBtn.Play();
            animBtn.SetColor(ButtonColor.Green_Up, ButtonColor.Green_Down, FontColor.Green);
            NotifyInHome.gameObject.SetActive(!HasClaimedToday());
            DotClaim.gameObject.SetActive(!HasClaimedToday());
            if (_profile.rewardDailyIndex >= 7)
            {
                _profile.rewardDailyIndex = 0;
                _profile.rewardPackIndex++;
                _profile.rewardPackIndex = Mathf.Clamp(_profile.rewardPackIndex, 0, 3);
            }

            GenRewardPack();
            //TODO:CheckReward
        }
    }
}