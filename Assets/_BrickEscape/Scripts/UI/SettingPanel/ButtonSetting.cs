using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BrickEscape
{
    public class ButtonSetting : MonoBehaviour
    {
        [SerializeField] GameObject textOn;
        [SerializeField] GameObject textOff;
        [SerializeField] RectTransform rectButton;
        [SerializeField] RectTransform rectHandle;
        [SerializeField] Image imageButton;
        [SerializeField] private Sprite imageOnMode;
        [SerializeField] private Sprite imageOffMode;
        
        public void ChangeButton(bool isOn)
        {
            float xOn = rectButton.rect.width / 2 - rectHandle.rect.width / 2;
            float xOff = -(rectButton.rect.width - rectHandle.rect.width) / 2;
            float targetX = isOn ? xOn : xOff;
            rectHandle.transform.DOLocalMoveX(-targetX, 0);
            rectHandle.transform.DOLocalMoveX(targetX, 0.2f)
                .SetEase(Ease.Linear)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    textOn.SetActive(isOn);
                    textOff.SetActive(!isOn);
                    imageButton.sprite = isOn ? imageOnMode : imageOffMode;
                });
        }
    }
}
