using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BrickEscape
{
    public class AnimButton : MonoBehaviour
    {
        public Button AcceptBtn;
        public Image up;
        public Image down;
        public Image Shadow;
        public TextMeshProUGUI btnTxt;
        private Sequence _animSeq;
        private Vector2 _upStartPos;
        private Vector2 _shadowStartPos;

        public float ButonUpOffsetY = 10f;
        public float ButtonShadowOffsetY = 5f;
        public float recoilOffset = 5f;
        public float DelayBetweenLoop = 1.5f;

        public float Phase1Duration = 0.3f;
        public float Phase2duration = 0.25f;
        public float Phase3duration = 0.15f;
        public bool autoPlayOnAwake = true;

        private void Awake()
        {
             _upStartPos = up.rectTransform.anchoredPosition;
             _shadowStartPos = Shadow.rectTransform.anchoredPosition;
        }
        public void OnEnable()
        {
            if (autoPlayOnAwake)
            {
                Play();
            }
        }
        public void OnDisable()
        {
            if (autoPlayOnAwake)
            {
                ResetAnim(); 
            }
           
        }

        public void Play()
        {
            _animSeq?.Kill();

            _animSeq = DOTween.Sequence();

            // ===== Phase 1: Overshoot =====
            _animSeq.Append(
                DOTweenModuleUI.DOAnchorPosY(
                    up.rectTransform,
                    ButonUpOffsetY,
                    Phase1Duration,
                    false
                ).SetEase(Ease.OutSine)
            );

            _animSeq.Join(
                DOTweenModuleUI.DOAnchorPosY(
                    Shadow.rectTransform,
                    ButtonShadowOffsetY,
                    Phase1Duration,
                    false
                ).SetEase(Ease.OutSine)
            );



            // ===== Phase 2: Về vị trí gốc =====
            _animSeq.Append(
                DOTweenModuleUI.DOAnchorPosY(
                    up.rectTransform,
                    _upStartPos.y - recoilOffset,
                   Phase2duration,
                    false
                ).SetEase(Ease.OutQuad)
            );

            _animSeq.Join(
                DOTweenModuleUI.DOAnchorPosY(
                    Shadow.rectTransform,
                    _shadowStartPos.y - recoilOffset,
                    Phase2duration,
                    false
                ).SetEase(Ease.OutQuad)
            );



            // ===== Phase 3: Về vị trí gốc =====
            _animSeq.Append(
                DOTweenModuleUI.DOAnchorPosY(
                    up.rectTransform,
                    _upStartPos.y,
                    Phase3duration,
                    false
                ).SetEase(Ease.OutQuad)
            );

            _animSeq.Join(
                DOTweenModuleUI.DOAnchorPosY(
                    Shadow.rectTransform,
                    _shadowStartPos.y,
                    Phase3duration,
                    false
                ).SetEase(Ease.OutQuad)
            );
            // ===== Delay giữa mỗi loop =====
            _animSeq.AppendInterval(DelayBetweenLoop);

            // ===== Loop vô hạn =====
            _animSeq.SetLoops(-1, LoopType.Restart);

        }
        [Button]
        public void ResetAnim()
        {
            // Kill sequence nếu đang tồn tại
            if (_animSeq != null)
            {
                _animSeq.Kill();
                _animSeq = null;
            }

            // Reset vị trí về ban đầu
            if (up != null)
            {
                up.rectTransform.anchoredPosition = _upStartPos;
            }

            if (Shadow != null)
            {
                Shadow.rectTransform.anchoredPosition = _shadowStartPos;
            }
        }
        [SerializeField] private SpriteCollection spriteCollection;
        [Button]
        public void SetColor(ButtonColor upColor, ButtonColor downColor, FontColor font)
        {
            if (Application.isPlaying) 
            {
                up.color = GameManager.Instance.spriteCollection.ColorDict[upColor];
                down.color = GameManager.Instance.spriteCollection.ColorDict[downColor];
                btnTxt.fontMaterial = GameManager.Instance.spriteCollection.fontDict[font];
            }
            else 
            {
                up.color = spriteCollection.ColorDict[upColor];
                down.color = spriteCollection.ColorDict[downColor];
                btnTxt.fontMaterial = spriteCollection.fontDict[font];
            }
        }

        public void SetText(string text)
        {
            btnTxt.text = text;
        }
        public void SetTextColor(ButtonColor upColor)
        {
            btnTxt.color = GameManager.Instance.spriteCollection.ColorDict[upColor];
        }
        public void SetTextColor(Color color)
        {
            btnTxt.color = color;
        }
    }
}
