using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using NabaGame.Tracking;
using UnityEngine;

namespace BrickEscape
{
    public class TabBarItem : MonoBehaviour
    {
        public GameObject TabObject;
        public GameObject TabPanel;
        public int index;
        public bool isReady;
        public Tween scaleTween;
        bool isSelected = false;

        public void OnClick()
        {
            AudioManager.Instance.PlayButtonSound();
            if (!isReady)
            {
                TabObject.SetActive(true);
                DOVirtual.DelayedCall(2, () => TabObject.SetActive(false));
                return;
            }

            if (isSelected) return;
            UIMainManager.Instance.homePanel.OnTapItemBar(index);
            TrackingManager.TrackEvent(TrackingEvent.Button_Click,
                TrackingParamter.Level, GameManager.Instance.PlayerProfile.LevelProfile.CurrentLevel.ToString(),
                TrackingParamter.Type, "shop_btn");
        }

        public void OnSelected()
        {
            TabObject.SetActive(true);
            if (isReady)
            {
                isSelected = true;
                TabObject.transform.localScale = Vector3.zero;
                scaleTween = ShortcutExtensions.DOScale(TabObject.transform, Vector3.one * 1, 0.2f)
                    .SetEase(Ease.OutQuad);
            }
        }

        public void OnUnSelected()
        {
            if (!isReady) return;
            if (scaleTween != null) scaleTween.Kill();
            isSelected = false;
            TabObject.transform.localScale = Vector3.one;
            TabObject.SetActive(false);
        }
    }
}