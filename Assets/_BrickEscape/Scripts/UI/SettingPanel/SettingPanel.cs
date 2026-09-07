using BrickEscape;
using DG.Tweening;
using NabaGame.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BrickEscape
{
    public class SettingPanel : BaseUI
    {
        [SerializeField] Button btnSound;
        [SerializeField] Button btnMusic;
        [SerializeField] Button btnHaptic;
        [SerializeField] Button btnTheme;
        [SerializeField] Button btnClose;
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

            btnSettingSound.SetToggle(GameManager.Instance.PlayerProfile.SoundSetting);
            btnSettingMusic.SetToggle(GameManager.Instance.PlayerProfile.MusicSetting);
            btnSettingHaptic.SetToggle(GameManager.Instance.PlayerProfile.VibrationSetting);
            btnSettingTheme.SetToggle(GameManager.Instance.PlayerProfile.isDarkTheme);
        }

        #endregion

        #region Public Functions

        public void Open()
        {
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
            AudioManager.Instance.PlayButtonSound();
            UIMainManager.Instance.adsPanel.WatchAdsInter();
            UIMainManager.Instance.adsPanel.CheckOnShowNoAds();
       
            Hide();
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
[Serializable]
public class ToggleSetting
{
    public RectTransform toggleTranform;
    public bool isOn;
    public TextMeshProUGUI txtStatus;
    public float posOnX = 20f;
    public float posOffX = 20f;
    private Tween _moveTween;
    public float tweenDuration = 0.25f;
    public Image imgBackground;
    public void SetToggle(bool isOn)
    {
        this.isOn = isOn;

        float targetX = isOn ? posOnX : posOffX;
        toggleTranform.anchoredPosition =
            new Vector2(targetX, toggleTranform.anchoredPosition.y);
        imgBackground.sprite = isOn ? GameManager.Instance.spriteCollection.spriteDic[SpriteUI.Setting_On] :GameManager.Instance.spriteCollection.spriteDic[SpriteUI.Setting_Off];
        txtStatus.text = isOn ? "ON" : "      OFF";
    }

    public void OnToggleChange(bool isOn)
    {
        this.isOn = isOn;

        float targetX = isOn ? posOnX : posOffX;
        txtStatus.text = "";
        // Kill tween cũ nếu còn sống
        if (_moveTween != null && _moveTween.IsActive())
        {
            _moveTween.Kill();
        }
        imgBackground.sprite = isOn ? GameManager.Instance.spriteCollection.spriteDic[SpriteUI.Setting_On] : GameManager.Instance.spriteCollection.spriteDic[SpriteUI.Setting_Off];
        _moveTween = DG.Tweening.DOTweenModuleUI.DOAnchorPosX(
                toggleTranform,
                targetX,
                tweenDuration,
                false
            )
            .SetEase(Ease.OutCubic)
            .SetUpdate(true).OnComplete(() =>
                { 
                    txtStatus.text = isOn ? "ON" : "      OFF";
                   
                }); // nếu cần chạy cả khi TimeScale = 0

        
    }
}
