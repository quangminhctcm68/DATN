using BMH.Ads;
using DG.Tweening;
using EasyTransition;
using NabaGame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BrickEscape
{
    public class PausePanel : BaseUI
    {
        [SerializeField] Button btnSound;
        [SerializeField] Button btnMusic;
        [SerializeField] Button btnHaptic;
        [SerializeField] Button btnTheme;
        [SerializeField] Button btnClose;
        [SerializeField] Button btnRestart;

        [SerializeField] Button btnHome;
        [SerializeField] ToggleSetting btnSettingSound;
        [SerializeField] ToggleSetting btnSettingMusic;
        [SerializeField] ToggleSetting btnSettingHaptic;
        [SerializeField] ToggleSetting btnSettingTheme;
        #region Start, Update, Validate

        public void SetInfo()
        {
            btnSound.onClick.AddListener(ChangeSound);
            btnMusic.onClick.AddListener(ChangeMusic);
            btnHaptic.onClick.AddListener(ChangeHaptic);
            btnTheme.onClick.AddListener(ChangeTheme);
            btnClose.onClick.AddListener(Close);
            btnRestart.onClick.AddListener(RetryBtn);
            btnHome.onClick.AddListener(HomeBtn);

            btnSettingSound.SetToggle(GameManager.Instance.PlayerProfile.SoundSetting);
            btnSettingMusic.SetToggle(GameManager.Instance.PlayerProfile.MusicSetting);
            btnSettingHaptic.SetToggle(GameManager.Instance.PlayerProfile.VibrationSetting);
            btnSettingTheme.SetToggle(GameManager.Instance.PlayerProfile.isDarkTheme);
        }

        #endregion

        #region Public Functions

        public void Open()
        {
            //Time.timeScale = 0;
            btnSettingSound.SetToggle(GameManager.Instance.PlayerProfile.SoundSetting);
            btnSettingMusic.SetToggle(GameManager.Instance.PlayerProfile.MusicSetting);
            btnSettingHaptic.SetToggle(GameManager.Instance.PlayerProfile.VibrationSetting);
            btnSettingTheme.SetToggle(GameManager.Instance.PlayerProfile.isDarkTheme);
            Show();
        }

        public void OpenInGame()
        {
            //if (GameController.Instance.levelController.IsLevelOver) return;
            btnSettingSound.SetToggle(GameManager.Instance.PlayerProfile.SoundSetting);
            btnSettingMusic.SetToggle(GameManager.Instance.PlayerProfile.MusicSetting);
            btnSettingHaptic.SetToggle(GameManager.Instance.PlayerProfile.VibrationSetting);
            btnSettingTheme.SetToggle(GameManager.Instance.PlayerProfile.isDarkTheme);
            Show();
        }
        public void Close()
        {
            //Time.timeScale = 1;
            AudioManager.Instance.PlayButtonSound();
            Hide();
        }

        public void HomeBtn()
        {
            //TransitionManager.Instance().Transition(
            //         UIMainManager.Instance.transition,
            //         0f
            //  );
            GameController.Instance.audioManager.PlayButtonSound();
            UIMainManager.Instance.adsPanel.WatchAdsInter();
            Sequence seq = DOTween.Sequence();
            seq.AppendInterval(0.3f)
               .AppendCallback(() =>
               {
                   GameController.Instance.levelGenerator.ClearEverything();
                   Hide();
                   UIMainManager.Instance.gamePlayPanel.Hide();
                   UIMainManager.Instance.homePanel.Show();
                   AdManager.Instance.Hide(AdsType.Banner);
               });


        }
       
        public void RetryBtn()
        {
            GameController.Instance.audioManager.PlayButtonSound();
            UIMainManager.Instance.adsPanel.WatchAdsInter();

            if (GameController.Instance.heartManager.GetRemainingUnlimitedHeartTime() <= 0) 
            {
                UIMainManager.Instance.replayPanel.OnClickReplay(() =>
                {
                    if (GameManager.Instance.PlayerProfile.HeartProfile.CurrentHeart > 0)
                    {
                        GameController.Instance.heartManager.ReduceHeart();
                        GameController.Instance.levelGenerator.PrepareLevelData();
                        UIMainManager.Instance.replayPanel.Hide();
                    }

                });
                Hide();
            }
            else 
            {
                DOVirtual.DelayedCall(0.3f, () => {
                    GameController.Instance.levelGenerator.PrepareLevelData();
                    Hide();
                });
            }

        
          

        }


        #endregion

        #region Private Functions

        void ChangeSound()
        {
            AudioManager.Instance.PlayButtonSound();
            GameManager.Instance.PlayerProfile.ChangeSoundSetting();
            btnSettingSound.OnToggleChange(GameManager.Instance.PlayerProfile.SoundSetting);
        }
        void ChangeMusic()
        {
            AudioManager.Instance.PlayButtonSound();
            GameManager.Instance.PlayerProfile.ChangeMusicSetting();
            btnSettingMusic.OnToggleChange(GameManager.Instance.PlayerProfile.MusicSetting);
        }
        void ChangeHaptic()
        {
            AudioManager.Instance.PlayButtonSound();
            GameManager.Instance.PlayerProfile.ChangeVibrationSetting();
            btnSettingHaptic.OnToggleChange(GameManager.Instance.PlayerProfile.VibrationSetting);
        }
        void ChangeTheme()
        {
            AudioManager.Instance.PlayButtonSound();
            GameManager.Instance.PlayerProfile.ChangeThemeSetting();
            btnSettingTheme.SetToggle(GameManager.Instance.PlayerProfile.isDarkTheme);
        }
        #endregion
    }
}
