using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BrickEscape
{
    public class Cloud: MonoBehaviour
    {
        public RectTransform rect;
        public List<RectTransform> destination;
        public RectTransform PlayerDestiation;
        public bool isFinalCloud = false;
        public CanvasGroup cg;
        public void OnValidate() 
        {
            if(rect== null) 
            {
                rect = GetComponent<RectTransform>();
            }

            if (cg == null)
            {
                cg = rect.GetComponent<CanvasGroup>();
                if (cg == null) cg = rect.gameObject.AddComponent<CanvasGroup>();
            }

          
        }


        public void Show() 
        {
            DOTween.Kill(rect);

            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;
            cg.alpha = 1f;

        }

        [Button]
        public void Disappear()
        {
            if(isFinalCloud) return;
            DOTween.Kill(rect);

            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;

         
            cg.alpha = 1f;

            Sequence seq = DOTween.Sequence();

            // 1. Phồng nhẹ (overshoot)
            seq.Append(
                rect.DOScale(Vector3.one * 1.25f, 0.12f)
                    .SetEase(Ease.OutBack)
            );

            // 2. Squash nhẹ (tạo cảm giác vật lý)
            seq.Append(
                rect.DOScale(new Vector3(0.9f, 1.1f, 1f), 0.08f)
                    .SetEase(Ease.InOutBack)
            );

            // 3. Co lại + fade out
            seq.Append(
                rect.DOScale(Vector3.zero, 0.2f)
                    .SetEase(Ease.InBack)
            );

            seq.Join(
                cg.DOFade(0f, 0.2f)
            );

            // 4. Optional: xoay nhẹ cho sống động
            seq.Join(
                rect.DORotate(new Vector3(0, 0, Random.Range(-90f, 90f)), 0.2f)
                    .SetEase(Ease.OutSine)
            );
        }
    }
}
