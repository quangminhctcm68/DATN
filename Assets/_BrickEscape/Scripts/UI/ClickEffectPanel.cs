using AssetKits.ParticleImage;
using BMH.Ads;
using DG.Tweening;
using NabaGame.UI;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BrickEscape
{
    public class ClickEffectPanel : MonoBehaviour
    {
        public Camera worldCamera;
        public Canvas canvas;
        public Transform trans;
        public RectTransform _canvasRect;
        public ParticleImage coinEffect;
        public ParticleImage dailyEffect;
        public ParticleImage weeklyEffect;
        public ParticleImage monthlyEffect;
        public RectTransform transTEst;
        //[Button]
        public void PlayDailyEffect(RectTransform trans, int count) 
        {
            PlayParticleImage(dailyEffect, trans, count);
        }

        public void PlayWeeklyEffect(RectTransform trans, int count)
        {
            PlayParticleImage(weeklyEffect, trans, count);
        }
        public void PlayMonthlyEffect(RectTransform trans, int count)
        {
            PlayParticleImage(monthlyEffect, trans, count);
        }
        public void PlayParticleImage(ParticleImage img,RectTransform trans, int count) 
        {
            img.SetBurst(0,0,count);
            img.attractorTarget = trans;
            img.Play();
        }

        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color highlightColor = Color.green;





        public void PlayCoinCollectFX(TextMeshProUGUI coinText, int startValue, int targetValue)
        {
            // kill tween cũ
            DOTween.Kill(coinText);
            DOTween.Kill(coinText.rectTransform);

            // reset state
            coinText.rectTransform.localScale = Vector3.one;
            coinText.color = normalColor; // 👉 đảm bảo start đúng màu

            // play VFX
            coinEffect.Play();

            var seq = DOTween.Sequence();

            // delay 1s
            seq.AppendInterval(1f);

            // scale
            seq.Append(
                 coinText.rectTransform
                .DOPunchScale(Vector3.one * 0.2f, 0.3f, 10, 0.8f)
            );

            // count
            seq.Join(
                DOTween.To(
                    () => startValue,
                    x => coinText.text = x.ToString(),
                    targetValue,
                    0.4f
                ).SetEase(Ease.OutCubic)
            );

            // color flash
            seq.Join(
                coinText.DOColor(highlightColor, 0.15f)
                    .SetLoops(2,LoopType.Yoyo)
            );

            // complete
            seq.OnComplete(() =>
            {
                // 👉 đảm bảo state cuối luôn đúng
                coinText.text = targetValue.ToString();
                coinText.color = normalColor;

                UIMainManager.Instance.homePanel.UpdateCoin();
            });
        }


        public void PlayHightLight(RewardType rewardType, int Qty, RectTransform sourceRect)
        {
            IconEffect effect = GameManager.Instance.pooling.SpawnIconEffect(trans);

            RectTransform effectRect = effect.GetComponent<RectTransform>();

            // 👉 Convert từ sourceRect → về local của effect parent
            Vector2 worldPos = sourceRect.TransformPoint(sourceRect.rect.center);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                (RectTransform)trans, // parent của effect
                RectTransformUtility.WorldToScreenPoint(null, worldPos),
                null,
                out Vector2 localPos
            );

            effect.PlayEffect(rewardType, Qty, localPos);
        }
















        public void PlayEffect(Vector3 screenPos)
        {
            // Spawn từ pool
            ClickEffect click = GameManager.Instance.pooling.SpawnClickEffect(_canvasRect);

            // Convert screen → canvas local
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvasRect,
                screenPos,
                null, // Overlay => null
                out Vector2 localPoint);

            // Set vị trí đúng
            click.rect.anchoredPosition = localPoint;

            click.PlayEffect();
        }


        public Vector2 WorldToCanvasPosition(Vector3 worldPosition)
        {
            // Convert world → screen
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(worldCamera, worldPosition);

            // Convert screen → canvas local position
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenPoint,
                canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : worldCamera,
                out Vector2 localPoint);

            return localPoint;
        }
    }
}
