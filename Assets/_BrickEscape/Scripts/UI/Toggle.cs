using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BrickEscape
{
    public class Toggle : MonoBehaviour
    {
        public RectTransform toggleTranform;
        public bool isOn;
        public Sprite spriteOn;
        public Sprite spriteOff;
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
            imgBackground.sprite = isOn ? spriteOn : spriteOff;
            //txtStatus.text = isOn ? "ON" : "      OFF";
        }

        public void OnToggleChange(bool isOn)
        {
            this.isOn = isOn;

            float targetX = isOn ? posOnX : posOffX;

            // Kill tween cũ nếu còn sống
            if (_moveTween != null && _moveTween.IsActive())
            {
                _moveTween.Kill();
            }
            //imgBackground.sprite = isOn ? GameManager.Instance.spriteCollection.spriteDic[SpriteUI.Setting_On] : GameManager.Instance.spriteCollection.spriteDic[SpriteUI.Setting_Off];
            _moveTween = DG.Tweening.DOTweenModuleUI.DOAnchorPosX(
                    toggleTranform,
                    targetX,
                    tweenDuration,
                    false
                )
                .SetEase(Ease.OutCubic)
                .SetUpdate(true).OnComplete(() =>
                {
                    imgBackground.sprite = isOn ? spriteOn : spriteOff;

                }); // nếu cần chạy cả khi TimeScale = 0


        }
    }
}
