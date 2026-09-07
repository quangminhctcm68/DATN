using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using NabaGame.Tracking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BrickEscape
{
    public class TimerReward : MonoBehaviour
    {
        public Button Reward;
        public bool allowRecive = true;
        public GameObject redDot;
        public TextMeshProUGUI _cooldownText;
        private TextMeshProUGUI coinText;
        private void Start()
        {
            Reward.onClick.AddListener(ClaimCoin);
            coinText = UIMainManager.Instance.homePanel.Coin;
        }


        public void UpdateText(int cooldown)
        {
            if (!UIMainManager.Instance.homePanel.IsVisible())
                return;

            // Clamp để tránh âm
            if (cooldown < 0)
                cooldown = 0;

            int minutes = cooldown / 60;
            int seconds = cooldown % 60;

            // Format tay để tránh GC
            _cooldownText.text =
                (minutes < 10 ? "0" : "") + minutes +
                ":" +
                (seconds < 10 ? "0" : "") + seconds;
        }

        public void ClaimCoin()
        {
            GameController.Instance.audioManager.PlayButtonSound();
            if(!allowRecive) return;
            allowRecive = false;

            GameController.Instance.timeManager.ReceiveCoinAction();
            UIMainManager.Instance.clickEffectPanel.coinEffect.Play();
            
            int startValue = GameManager.Instance.PlayerProfile.Coin;
            int endValue = startValue + 50;
            UIMainManager.Instance.clickEffectPanel.PlayCoinCollectFX(coinText, startValue, endValue);
            GameManager.Instance.PlayerProfile.ChangeCoin(50);


            AudioManager.Instance.PlaySFX(SFXID.Coin);
            redDot.SetActive(false);
            UIMainManager.Instance.homePanel.Notify.gameObject.SetActive(false);
            int count = PlayerPrefs.GetInt("timer_reward", 0);
            count++;
            PlayerPrefs.SetInt("timer_reward",count);
            TrackingManager.TrackEvent("Free_Gold" , TrackingParamter.Count, count.ToString() );
        }
        public void CanRecive() 
        {
            allowRecive= true;
            redDot.SetActive(true);
            _cooldownText.text = "CLAIM";
        }
    }
}
