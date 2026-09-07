using System.Collections;
using System.Collections.Generic;
using NabaGame.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BrickEscape
{
    public class RewardGetPanel : BaseUI
    {
        [SerializeField] private Image rewardIcon;
        [SerializeField] private Image infiniteIcon;
        [SerializeField] private TextMeshProUGUI rewardAmountText;
        [SerializeField] private AnimButton animBtn;
        
        #region Start, Update, Validate

        public void Open()
        {
            Show();
        }

        public void Close()
        {
            AudioManager.Instance.PlayButtonSound();
            Hide();
        }
        
        public override void OnInAnimationStart()
        {
            animBtn.Play();
        }
        
        public override void OnOutAnimationStart()
        {
            animBtn.ResetAnim();
        }
        
        #endregion
        
        #region Set Data
        
        public void SetData(RewardType rewardType)
        {
            ProcessIcon(rewardType);
            SetEmptyRewardAmount();
        }

        public void SetData(RewardType rewardType, int rewardAmount)
        {
            ProcessIcon(rewardType);
            ProcessRewardAmount(rewardType, rewardAmount);
        }
        
        public void SetData(AvatarID avatarID)
        {
            ProcessIcon_Avatar(avatarID);
            SetEmptyRewardAmount();
        }
        
        public void SetData(FrameID frameID)
        {
            ProcessIcon_Frame(frameID);
            SetEmptyRewardAmount();
        }

        void ProcessIcon(RewardType rewardType)
        {
            // if (rewardType == RewardType.Infinite_Heart)
            // {
            //     ChangeInfiniteIconVisibility(true);
            // }
            // else
            // {
            //     ChangeInfiniteIconVisibility(false);
            // }
            rewardIcon.sprite = GameManager.Instance.dataCollection.GetRewardIcon(rewardType);
        }

        void ProcessIcon_Avatar(AvatarID ID)
        {
            //ChangeInfiniteIconVisibility(false);
            rewardIcon.sprite = GameManager.Instance.spriteCollection.GetAvatarSprite(ID);
        }
        
        void ProcessIcon_Frame(FrameID ID)
        {
            //ChangeInfiniteIconVisibility(false);
            rewardIcon.sprite = GameManager.Instance.spriteCollection.GetFrameSprite(ID);
        }

        void ProcessRewardAmount(RewardType rewardType, int rewardAmount)
        {
            if (rewardType == RewardType.Infinite_Heart)
            {
                if (rewardAmount >= 3600)
                    rewardAmountText.SetText($"+{(float)rewardAmount / 3600}h");
                else
                    rewardAmountText.SetText($"+{(float)rewardAmount / 60}m");
            }
            else
            {
                rewardAmountText.SetText($"+{rewardAmount}");
            }
        }

        void SetEmptyRewardAmount()
        {
            rewardAmountText.SetText("");
        }

        void ChangeInfiniteIconVisibility(bool status)
        {
            infiniteIcon.gameObject.SetActive(status);
        }
        
        #endregion
    }
}
