using AssetKits.ParticleImage;
using BMH.Ads;
using DG.Tweening;
using NabaGame.Tracking;
using NabaGame.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BrickEscape
{
    public class UnlockBoosterPanel : BaseUI
    {
        public BoosterType boosterType;
        public Image type;
        public Button Reward;
        public AnimButton animBtn;
        public TextMeshProUGUI DesScription;
        public DOTweenAnimation anim;
        public ParticleImage[] particleImages;
        public RectTransform[] rectTransforms;
        public TextMeshProUGUI Title;
        [SerializeField] TextMeshProUGUI hint_Txt;
        [SerializeField] TextMeshProUGUI magicWand_Txt;
        [SerializeField] TextMeshProUGUI hammer_Txt;
        //public 
        public RectTransform BoosterPos;
        bool Claimed = false;
        public override void OnInAnimationStart()
        {
            base.OnInAnimationStart();
            animBtn.Play();
            anim.DOPlay();
            Claimed = false;
            foreach (var item in particleImages)
            {
                item.gameObject.SetActive(false);
            }

            foreach (var item in rectTransforms)
            {
                item?.gameObject.SetActive(false);
            }
        }

        public override void OnOutAnimationStart()
        {
            base.OnOutAnimationStart();
            animBtn.ResetAnim();
            anim.DOPause();
        }

        public void SetInfor()
        {
            Reward.onClick.AddListener(RewardBtn);


        }
        public void SetInfor(BoosterType bType)
        {
            boosterType = bType;
            type.sprite = GameManager.Instance.spriteCollection.BoostetDic[boosterType];
            SetDesScription();
            Show();
        }

        public void SetDesScription()
        {
            switch (boosterType)
            {
                case BoosterType.Hammer:
                    Title.text = "HAMMER";
                    DesScription.text = "Choose one block to break";
                    break;
                case BoosterType.Hint:
                    Title.text = "HINT";
                    DesScription.text = "Show a block that can be removed";
                    break;
                case BoosterType.Magic:
                    Title.text = "MAGIC WAND";
                    DesScription.text = "Three random blocks disappear.";
                    break;
            }
            UpdateText();


        }
        public void RewardBtn()
        {
            AudioManager.Instance.PlayButtonSound();
            if(!Claimed)
                OnComplete();

        }


        public void OnComplete()
        {
           
            Claimed = true;
            PlayEffect(boosterType);
        }
        public void UpdateText()
        {
            magicWand_Txt.text = GameManager.Instance.PlayerProfile.MagicWandBoosterUseCount.ToString();
            hint_Txt.text = GameManager.Instance.PlayerProfile.HintBoosterUseCount.ToString();
            hammer_Txt.text = GameManager.Instance.PlayerProfile.HammerBoosterUseCount.ToString();

        }
        public void PlayEffect(BoosterType type)
        {
            particleImages[(int)type].gameObject.SetActive(true);
            particleImages[(int)type].Play();
            rectTransforms[(int)type].transform.localScale = Vector3.one;
            DOVirtual.DelayedCall(1.5f, () =>
            {
                rectTransforms[(int)type].gameObject.SetActive(true);
                ShortcutExtensions.DOScale(rectTransforms[(int)type].transform, Vector3.one * 1.2f, 0.25f)
                    .SetEase(Ease.OutQuad)
                    .OnComplete(() =>
                    {
                        switch (boosterType)
                        {
                            case BoosterType.Hammer:
                                GameManager.Instance.PlayerProfile.ChangeHammerBoosterUseCount(1);

                                break;
                            case BoosterType.Hint:
                                GameManager.Instance.PlayerProfile.ChangeHintBoosterUseCount(1);
                                break;
                            case BoosterType.Magic:
                                GameManager.Instance.PlayerProfile.ChangeMagicWandBoosterUseCount(1);
                                break;
                        }
                        UpdateText();

                        DOVirtual.DelayedCall(0.25f,
                            () => { rectTransforms[(int)type].gameObject.SetActive(false);
                                Hide();
                            });
                    });
            });
        }
    }
}
