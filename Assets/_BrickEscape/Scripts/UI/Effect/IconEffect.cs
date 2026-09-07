using NabaGame.Core.Runtime.TickManager;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BrickEscape
{
    public class IconEffect : TickableBehaviour
    {
        public RectTransform trans;
        public CanvasGroup group;
        public TextMeshProUGUI speedText;

        private Vector2 _startPos;
        private float _timer;

        private float _duration = 0.8f;
        private float _moveDuration = 0.5f;

        private float _targetY;
        private bool _playing;

        private const int Step = 20;
        private const int Min = 300;
        private const int Max = 400;
        public Image Icon;



        public void PlayEffect(RewardType rewardType, float Qty, Vector2 startPos)
        {
            // TMP không alloc
            //if (Mathf.Abs(_lastSpeed - speed) > EPSILON)
            //{
            //    _lastSpeed = speed;
            //    speedText.text = "+" + speed;
            //}
            SetGiftInfor(rewardType, Qty);
            _startPos = startPos;
            trans.anchoredPosition = _startPos;
            group.alpha = 0f;

            int amount = Random.Range(0, ((Max - Min) / Step) + 1) * Step + Min;
            _targetY = _startPos.y + amount;

            _timer = 0f;
            _playing = true;

            gameObject.SetActive(true);
        }


        public void SetGiftInfor(RewardType rewardType, float Qty)
        {
            switch (rewardType)
            {
                case RewardType.Coin:
                    Icon.sprite = GameManager.Instance.spriteCollection.Icon[IconType.Coin];
                    //Qty_txt.text = $"+{Qty}";
                    break;

                case RewardType.Hint_Booster:
                    Icon.sprite = GameManager.Instance.spriteCollection.BoostetDic[BoosterType.Hint];

                    //Qty_txt.text = $"+{Qty}";
                    break;

                case RewardType.MagicWand_Booster:
                    Icon.sprite = GameManager.Instance.spriteCollection.BoostetDic[BoosterType.Magic];
                    //Qty_txt.text = $"+{Qty}";
                    break;
                case RewardType.Hammer_Booster:
                    Icon.sprite = GameManager.Instance.spriteCollection.BoostetDic[BoosterType.Hammer];
                    //Qty_txt.text = $"+{Qty}";
                    break;
                case RewardType.Infinite_Heart:
                    Icon.sprite = GameManager.Instance.spriteCollection.Icon[IconType.Infinite_Heart];
                    //Qty_txt.text = $"{Qty}m";
                    break;

                case RewardType.Avatar_Icon:
                    Icon.sprite = GameManager.Instance.spriteCollection.avatarDic[(AvatarID)Qty];
                    //Qty_txt.text = "";
                    break;

                case RewardType.Avatar_Frame:
                    Icon.sprite = GameManager.Instance.spriteCollection.frameDic[(FrameID)Qty];
                    //Qty_txt.text = "";
                    break;
            }

        }


        public override void OnTickableUpdated(float dt)
        {
            base.OnTickableUpdated(dt);
            if (!_playing) return;
            _timer += dt;

            float t = _timer / _duration;

            // 🔹 MOVE (ease OutBack approximation)
            float moveT = Mathf.Clamp01(_timer / _moveDuration);
            float ease = EaseOutBack(moveT);

            float y = Mathf.LerpUnclamped(_startPos.y, _targetY, ease);
            trans.anchoredPosition = new Vector2(_startPos.x, y);

            // 🔹 FADE IN + OUT
            if (_timer < 0.25f)
            {
                group.alpha = _timer / 0.25f;
            }
            else
            {
                float fadeOutT = (_timer - 0.25f) / 0.3f;
                group.alpha = 1f - Mathf.Clamp01(fadeOutT);
            }

            // 🔹 DONE
            if (_timer >= _duration)
            {
                _playing = false;
                OnEffectComplete();
            }
        }

        // Ease.OutBack custom (không alloc)
        private float EaseOutBack(float t)
        {
            float c1 = 1.70158f;
            float c3 = c1 + 1f;
            return 1 + c3 * Mathf.Pow(t - 1, 3) + c1 * Mathf.Pow(t - 1, 2);
        }

        private void OnEffectComplete()
        {
            GameManager.Instance.pooling.DestroyEffect(gameObject);
        }
    }
}
