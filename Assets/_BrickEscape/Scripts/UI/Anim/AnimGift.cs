using DG.Tweening;
using NabaGame.Core.Runtime.TickManager;
using NabaGame.UI;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BrickEscape
{
    public class AnimGift : TickableBehaviour
    {
        // Start is called before the first frame updat
        [Header("Wobble / Squash")]
        public float bounceAmplitude = 0.15f;
        public float bounceFrequency = 3.5f;
        public float squashAmount = 0.12f;
        Vector3 _baseScale;
        float _timeOffset;
        public bool Active;

        [FoldoutGroup("Visual")] public Image icon;
        [FoldoutGroup("Visual")] public Sprite openSprite;
        [FoldoutGroup("Visual")] public Sprite originSprite;

        void Start()
        {
            _timeOffset = UnityEngine.Random.value * 10f;
            _baseScale = transform.localScale;
            //PickNewTarget();
        }
        public override void OnTickableUpdated(float dt)
        {
            if (!gameObject.activeSelf) return;
            if (!Active) return;
            ApplyWobble(dt);
        }
        void ApplyWobble(float dt)
        {

            float t = (Time.time + _timeOffset) * bounceFrequency;
            float bounce = Mathf.Sin(t) * bounceAmplitude;

            float squash = 1f - bounce * squashAmount;
            float stretch = 1f + bounce * squashAmount;

            Vector3 targetScale = new Vector3(
                _baseScale.x * stretch,
                _baseScale.y * squash,
                _baseScale.z * stretch
            );

            transform.localScale = Vector3.Lerp(
                transform.localScale,
                targetScale,
                dt * 10f
            );
        }

        [Button]
        public void Test() 
        {
            transform.DOKill();

            Active = false;
            transform.localScale = Vector3.one;

            Sequence seq = DOTween.Sequence();

            seq.Append(transform.DORotate(new Vector3(0, 0, 20), 0.08f))
               .Append(transform.DORotate(new Vector3(0, 0, -20), 0.08f))
               .Append(transform.DORotate(new Vector3(0, 0, 15), 0.08f))
               .Append(transform.DORotate(new Vector3(0, 0, -15), 0.08f))
               .Append(transform.DORotate(new Vector3(0, 0, 8), 0.06f))
               .Append(transform.DORotate(new Vector3(0, 0, -8), 0.06f))
               .Append(transform.DORotate(Vector3.zero, 0.05f));

        }

        public  void PlayAnimOpen(Action action)
        {
            transform.DOKill();

            Active = false;
            transform.localScale = Vector3.one;

            Sequence seq = DOTween.Sequence();

            seq.Append(transform.DORotate(new Vector3(0, 0, 20), 0.08f))
               .Append(transform.DORotate(new Vector3(0, 0, -20), 0.08f))
               .Append(transform.DORotate(new Vector3(0, 0, 15), 0.08f))
               .Append(transform.DORotate(new Vector3(0, 0, -15), 0.08f))
               .Append(transform.DORotate(new Vector3(0, 0, 8), 0.06f))
               .Append(transform.DORotate(new Vector3(0, 0, -8), 0.06f))
               .Append(transform.DORotate(Vector3.zero, 0.05f));

            seq.OnComplete(() =>
            {
                icon.sprite = openSprite;
                action?.Invoke();
            });

        }

        public void OpenedState()
        {
            transform.localScale = Vector3.one;
            icon.sprite = openSprite;
            Active = false;
        }

        public void ResetState() 
        {
            transform.localScale = Vector3.one;
            icon.sprite = originSprite;
            Active = false;
        }
    }
}
