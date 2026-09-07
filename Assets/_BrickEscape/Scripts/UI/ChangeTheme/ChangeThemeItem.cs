using DG.Tweening;
using NabaGame.Core.Runtime.EventManager;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BrickEscape
{
    public class ChangeThemeItem : MonoBehaviour
    {
        public ChangeType type;

        [Header("Opacity")]
        [SerializeField] private float lightOpacity = 1f;
        [SerializeField] private float darkOpacity = 0.5f;

        [Header("Color")]
        private Color lightColor = Color.white;
        private Color darkColor = Color.gray;

        [Header("Sprite")]
        [SerializeField] private Sprite lightSprite;
        [SerializeField] private Sprite darkSprite;

        [Header("Targets")]
        public List<Image> images = new List<Image>();

        [SerializeField] private float _duration = 0.25f;


        public void Start()
        {
            EventManager.Instance.AddListener<ThemeChange>(OnChangeTheme);
            lightColor = GameManager.Instance.spriteCollection.lightColor;
            darkColor = GameManager.Instance.spriteCollection.darkColor;
        }




        public void OnDestroy()
        {
            EventManager.Instance.RemoveListener<ThemeChange>(OnChangeTheme);
        }

        public void OnChangeTheme(ThemeChange e)
        {
            ChangeTheme(e.currentTheme);
        }



        public void ChangeTheme(bool isDark)
        {
            if (images == null || images.Count == 0) return;

            switch (type)
            {
                case ChangeType.ChangeSprite:
                    ApplySprite(isDark);
                    break;

                case ChangeType.ChangeColor:
                    ApplyColor(isDark);
                    break;

                case ChangeType.ChangeOpacity:
                    ApplyOpacity(isDark);
                    break;
            }
        }

        void ApplySprite(bool isDark)
        {
            Sprite target = isDark ? darkSprite : lightSprite;

            foreach (var img in images)
            {
                if (img == null) continue;

                img.DOKill(); // 💥 kill tất cả tween trên Image này

                var seq = DG.Tweening.DOTween.Sequence();

                seq.Append(img.DOFade(0f, 0.15f));
                seq.AppendCallback(() => img.sprite = target);
                seq.Append(img.DOFade(1f, 0.15f));
            }
        }
        void ApplyColor(bool isDark)
        {
            Color target = isDark ? darkColor : lightColor;

            foreach (var img in images)
            {
                if (img == null) continue;

                img.DOKill();

                img.DOColor(target, _duration);
            }
        }

        void ApplyOpacity(bool isDark)
        {
            float target = isDark ? darkOpacity : lightOpacity;

            foreach (var img in images)
            {
                if (img == null) continue;

                img.DOKill();

                img.DOFade(target, _duration);
            }
        }

        public enum ChangeType
        {
            ChangeSprite,
            ChangeColor,
            ChangeOpacity
        }
    }
}