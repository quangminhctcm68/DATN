using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;
using NabaGame.UI;
using UnityEngine.UI;

namespace BrickEscape
{
    public class FindingPanel : BaseUI
    {
        public List<Race_Avatar> avatars;
        public TextMeshProUGUI FindingText;

        [SerializeField] private float _duration = 2f;
        [SerializeField] private float _avatarDelay = 0.08f;
        [SerializeField] private ParticleSystem StarEffect;
        [SerializeField] private Animator _animator;
        public Button Continue;

        public override void OnInAnimationStart()
        {
            base.OnInAnimationStart();
            PlayAnim();
        }

        public void SetInfor() 
        {
            Continue.onClick.AddListener(Close);
        }


        public void Close() 
        {
            AudioManager.Instance.PlayButtonSound();
            Hide();
            UIMainManager.Instance.racePanel.Show();
        }

        [Button]
        public void PlayAnim()
        {
            _animator.Play("Finding");
        }
        public void ResetText()
        {
            FindingText.text =
                $"Finding players on your level\n<size={150}%>{0}/100</size>";
        }


        [Button]
        public void ShowAnimFinding()
        {
            DG.Tweening.DOTween.Kill(this);

            // reset avatar
            foreach (var ava in avatars)
            {
                if (ava != null && ava.rect != null) 
                {
                    ava.rect.localScale = Vector3.zero;
                    ava.SetInfor();
                }
            }

            int current = 0;
            float sizePercent = 150f;
            FindingText.text =
                $"Finding players on your level\n<size={sizePercent}%>{0}/100</size>";
            Sequence seq = DG.Tweening.DOTween.Sequence().SetTarget(this);

            // =========================
            // TEXT COUNT
            // =========================
            seq.Append(
                DG.Tweening.DOTween.To(() => current, x =>
                {
                    current = x;

                    FindingText.text =
                        $"Finding players on your level\n<size={sizePercent}%>{current}/100</size>";
                },
                100,
                _duration
                ).SetEase(Ease.Linear)
            );

            // =========================
            // AVATAR POP (stagger)
            // =========================
            for (int i = 0; i < avatars.Count; i++)
            {
                int index = i;

                seq.Insert(i * _avatarDelay,
                    avatars[index].rect.DOScale(1f, 0.25f)
                        .SetEase(Ease.OutBack)
                );
            }

            // =========================
            // PUNCH SIZE (fake bằng size)
            // =========================
            seq.Append(
                DG.Tweening.DOTween.To(() => sizePercent, x =>
                {
                    sizePercent = x;

                    FindingText.text =
                        $"Finding players on your level\n<size={sizePercent}%>100/100</size>";
                },
                180f,
                0.15f
                ).SetEase(Ease.OutQuad)
            );

            // thu lại
            seq.Append(
                DG.Tweening.DOTween.To(() => sizePercent, x =>
                {
                    sizePercent = x;

                    FindingText.text =
                        $"Finding players on your level\n<size={sizePercent}%>100/100</size>";
                },
                150f,
                0.2f
                ).SetEase(Ease.OutBack).OnComplete(() => { StarEffect.Play(); })
            );
        }
    }
}