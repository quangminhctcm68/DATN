using BMH.Ads;
using DG.Tweening;
using NabaGame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BrickEscape
{
    public class RatingPanel : BaseUI
    {
        [SerializeField] private List<GameObject> star;
        public const string RatingKey = "RK";
        public const string ShowRatingFirst = "SRF";
        public int LevelCount;
        public bool ShowFirstTime; 
        public int require;
        public void CheckCondition()
        {
           
            if(GameManager.Instance.PlayerProfile.LevelProfile.CurrentLevel >10 && PlayerPrefs.GetInt(ShowRatingFirst, 0) == 0)
            {
                ShowRating();
                PlayerPrefs.SetInt(ShowRatingFirst, 1);
                return;
            }


            // Case 1: Đã rate → luôn show ads
            if (IsRated())
            {
                ShowAds();
                return;
            }

            // Chưa rate → tăng count
            LevelCount++;

            // Case 2: Level chưa đủ → ads
            if (GameManager.Instance.PlayerProfile.LevelProfile.currentLevel < 11)
            {
                ShowAds();
                return;
            }

            // Case 3: Level đủ → check count
            if (LevelCount >= require)
            {
                ShowRating();
            }
            else
            {
                ShowAds();
            }
        }

        private bool IsRated()
        {
            return PlayerPrefs.GetInt(RatingKey, 0) == 1;
        }

        private void ShowAds()
        {
            UIMainManager.Instance.adsPanel.WatchAdsInter();
        }
        public void CheckConditionWithOutInter()
        {
            if (PlayerPrefs.GetInt(RatingKey, 0) == 1) return;
            LevelCount++;
            if (LevelCount >= 10 && GameManager.Instance.PlayerProfile.LevelProfile.currentLevel >10)
            {
                ShowRating();
            }


        }


        public void ShowRating()
        {
            require = 6;
            SetInfor();
            Show();
   
        }

        public void SetInfor()
        {
            for (int i = 0; i < star.Count; i++)
            {
                star[i].SetActive(false);
            }
            LevelCount = 0;
           
        }

        public void Close() // button 4sao
        {
            GameController.Instance.audioManager.PlayButtonSound();
            for (int i = 0; i < star.Count; i++)
            {
                star[i].SetActive(i < 4);
            }

            DOVirtual.DelayedCall(0.5f, Hide).SetUpdate(true);
        }

        public void FiveStar() // button 5sao
        {
            GameController.Instance.audioManager.PlayButtonSound();
            for (int i = 0; i < star.Count; i++)
            {
                star[i].SetActive(true);
            }
            AdManager.Instance.ResetLastShowOpenAds();
            Time.timeScale = 1;
            StartCoroutine(IeRate());
        }

        private IEnumerator IeRate()
        {
            yield return new WaitForSeconds(0.5f);
            PlayerPrefs.SetInt(RatingKey, 1);
            // if (AppReviewManager.Instance != null)
            AppReviewManager.Instance.RateAndReview();
            Hide();
        }
    }
}