using DG.Tweening;
using NabaGame.UI;
using Sirenix.OdinInspector;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BrickEscape
{
    public class ChestOpenPanel : BaseUI
    {
        public SkeletonGraphic Chest;
        public RectTransform  Gift;
        public Image Icon;
        public TextMeshProUGUI Qty_txt;
        private Sequence _seq;
        private bool _done = false;
        private Sequence _closeSq;
        public ParticleSystem StarBrush;
        public void SetInfor()
        {
            Chest.Initialize(false);
            
        }
        public void OnTab()
        {
            if (!_done) return;
            _closeSq?.Kill();
            _closeSq= DOTween.Sequence();
            _done = false;
            _closeSq.Append(
                DG.Tweening.DOTweenModuleUI.DOAnchorPosY(Gift, -700f, 0.5f)
                .SetEase(Ease.InBack)
            );

            _closeSq.Join(
                Gift.DOScale(Vector3.zero, 0.5f)
                .SetEase(Ease.InBack)
            );

            _closeSq.Play().OnComplete(() => 
            {
                _done = true;
                Hide();
                closeAction?.Invoke();   
            });
            
            
        }
        public Action closeAction;
        public void SetCloseAction(Action action) 
        {
            closeAction = null;
            closeAction = action;
        }

        public void SetGiftInfor(RewardType rewardType,float Qty,Action action) 
        {
            switch (rewardType)
            {
                case RewardType.Coin:
                    Icon.sprite = GameManager.Instance.spriteCollection.Icon[IconType.Coin];
                    Qty_txt.text = $"+{Qty}";
                    break;

                case RewardType.Hint_Booster:
                    Icon.sprite = GameManager.Instance.spriteCollection.BoostetDic[BoosterType.Hint];
              
                    Qty_txt.text = $"+{Qty}";
                    break;

                case RewardType.MagicWand_Booster:
                    Icon.sprite = GameManager.Instance.spriteCollection.BoostetDic[BoosterType.Magic];
                    Qty_txt.text = $"+{Qty}";
                    break;
                case RewardType.Hammer_Booster:
                    Icon.sprite = GameManager.Instance.spriteCollection.BoostetDic[BoosterType.Hammer];
                    Qty_txt.text = $"+{Qty}";
                    break;
                case RewardType.Infinite_Heart:
                    Icon.sprite = GameManager.Instance.spriteCollection.Icon[IconType.Infinite_Heart];
                    Qty_txt.text = $"{Qty}m";
                    break;

                case RewardType.Avatar_Icon:
                    Icon.sprite = GameManager.Instance.spriteCollection.avatarDic[(AvatarID)Qty];
                    Qty_txt.text = "";
                    break;

                case RewardType.Avatar_Frame:
                    Icon.sprite = GameManager.Instance.spriteCollection.frameDic[(FrameID)Qty];
                    Qty_txt.text = "";
                    break;
            }
            SetCloseAction(action);
            PlayOpenChest();
        }


        [Button]
        public void PlayOpenChest()
        {
            _seq?.Kill();
            _seq = DOTween.Sequence();
            _done = false;
            Show();



            Gift.localScale = Vector3.zero;
            Gift.anchoredPosition = new Vector2(
                Gift.anchoredPosition.x,
                -700f
            );

            Chest.AnimationState.SetAnimation(
                0,
                "animation",
                false
            );

            _seq.AppendInterval(1.6f);
            _seq.AppendCallback(() =>
                {
                    StarBrush.Play();
                    AudioManager.Instance.PlaySFX(SFXID.Star_3);
                }
            );
            _seq.Append(
                DG.Tweening.DOTweenModuleUI.DOAnchorPosY(Gift,500f, 0.5f)
                .SetEase(Ease.OutBack)
            );

            _seq.Join(
                Gift.DOScale(Vector3.one, 0.5f)
                .SetEase(Ease.OutBack)
            );

            _seq.Play().OnComplete(()=>_done = true);
        }



    }
}
