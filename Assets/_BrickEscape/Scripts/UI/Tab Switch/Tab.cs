using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace BrickEscape
{
    public class Tab : MonoBehaviour
    {
        public int index;
        public GameObject TabObject;
        public bool isReady;
        public Tween scaleTween;
        bool isSelected = false;
        public Button button;
        public RectTransform Content;

        public void SetInfor() 
        {
            button.onClick.AddListener(OnClick);
        }
        public void OnClick()
        {
            if(isSelected) return;
            AudioManager.Instance.PlayButtonSound();
            UIMainManager.Instance.dailyQuestPanel.SetTab(index);
            UIMainManager.Instance.dailyQuestPanel.scrollRect.content = Content;
        }

        public void OnSelected()
        {
            TabObject.SetActive(true);
            isSelected = true;
            TabObject.transform.localScale = Vector3.zero;
            scaleTween = ShortcutExtensions.DOScale(TabObject.transform, Vector3.one * 1, 0.2f).SetEase(Ease.OutQuad);
        }
        public void OnUnSelected()
        {
            if (scaleTween != null) scaleTween.Kill();
            isSelected = false;
            TabObject.transform.localScale = Vector3.one;
            TabObject.SetActive(false);
        }
    }
}
