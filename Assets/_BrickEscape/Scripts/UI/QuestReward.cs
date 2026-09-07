using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BrickEscape
{
    public class QuestReward : MonoBehaviour
    {
        public AnimGift anim;
        public QuestRewardType type;
        public RewardType rwType;
        public int ID;
        public int Qty;
        public int required;
        public int process;
        public bool isReady;
        public bool isDone;
        public Button button;
        PlayerProfile profile;
        public GameObject Notify;
        private void OnValidate()
        {
            if(button == null)
            {
                button = GetComponent<Button>();
            }
            anim = GetComponent<AnimGift>();
        }
        public void SetInfor()
        {
            button.onClick.AddListener(OnClick);
            profile = GameManager.Instance.PlayerProfile;
            switch (type)
            {
                case QuestRewardType.Daily:
                    RawDailyQuestReward data = GameManager.Instance.rawDailyQuestRewardData.rawDailyQuestRewards[ID - 1];
                    required = data.RequireBadge;
                    rwType = data.RewardType;
                    Qty = data.Qty;
                    process = GameManager.Instance.PlayerProfile.QuestProfile.CollectDailyBadge;
                    break;
                case QuestRewardType.Weekly:
                    RawWeeklyReward wdata = GameManager.Instance.rawWeeklyRewardData.rawWeeklyRewards[ID - 1];
                    required = wdata.RequireBadge;
                    rwType = wdata.RewardType;
                    Qty = wdata.Qty;
                    process = GameManager.Instance.PlayerProfile.QuestProfile.CollectWeeklyBadge;
                    break;
                case QuestRewardType.Monthly:
                    RawMonthlyReward mdata = GameManager.Instance.rawMonthlyRewardData.rawMonthlyRewards[ID - 1];
                    required = mdata.RequireBadge;
                    rwType = mdata.RewardType;
                    Qty = mdata.Qty;
                    process = GameManager.Instance.PlayerProfile.QuestProfile.CollectMonthlyBadge;
                    break;

            }

        }


        public void CheckProcess()
        {
            switch (type)
            {
                case QuestRewardType.Daily:
                    process = GameManager.Instance.PlayerProfile.QuestProfile.CollectDailyBadgeReward;
                    isDone = GameManager.Instance.PlayerProfile.QuestProfile.DailyQuestReward[ID];
                    break;
                case QuestRewardType.Weekly:
                    process = GameManager.Instance.PlayerProfile.QuestProfile.CollectWeeklyBadgeReward;
                    isDone = GameManager.Instance.PlayerProfile.QuestProfile.WeeklyQuestReward[ID];
                    break;
                case QuestRewardType.Monthly:
                    isDone = GameManager.Instance.PlayerProfile.QuestProfile.MonthlyQuestReward[ID];
                    process = GameManager.Instance.PlayerProfile.QuestProfile.CollectMonthlyBadge;
                    break;

            }
            isReady = process >= required;

            if (isReady)
            {
                if (isDone)
                {
                    anim.OpenedState();
                    Notify.gameObject.SetActive(false);
                }
                else
                {
                    anim.Active = true;
                    Notify.gameObject.SetActive(true);
                }
            }
            else
            {
                Notify.gameObject.SetActive(false);
                anim.ResetState();
            }
        }


        public void OnClick()
        {
            if (isReady & !isDone)
            {
                switch (type)
                {
                    case QuestRewardType.Daily:
                        GameManager.Instance.PlayerProfile.QuestProfile.DailyQuestReward[ID] = true;
                        break;
                    case QuestRewardType.Weekly:
                        GameManager.Instance.PlayerProfile.QuestProfile.WeeklyQuestReward[ID] = true;
                        break;
                    case QuestRewardType.Monthly:
                        GameManager.Instance.PlayerProfile.QuestProfile.MonthlyQuestReward[ID] = true;
                        break;

                }
                GiveReward();
                Notify.gameObject.SetActive(false);
                anim.PlayAnimOpen(OnShow);
            }

        }

        public void GiveReward() 
        {
         
            switch (rwType)
            {
                case RewardType.Coin:
                    profile.ChangeCoin(Qty);
                    break;

                case RewardType.Hint_Booster:
                    profile.ChangeHintBoosterUseCount(Qty);
                    break;
                case RewardType.MagicWand_Booster:
                    profile.ChangeMagicWandBoosterUseCount(Qty);
                    break;
                case RewardType.Hammer_Booster:
                    profile.ChangeHammerBoosterUseCount(Qty);
                    break;
                case RewardType.Infinite_Heart:
                    GameController.Instance.heartManager.AddUnlimitedHeartTime(Qty * 60);
                    break;

                case RewardType.Avatar_Icon:
                    profile.avatarProfile.UnlockAvatar((AvatarID)Qty);
                    break;
                case RewardType.Avatar_Frame:
                    profile.avatarProfile.UnlockFrame((FrameID)Qty);
                    break;
            }
        }
        public void OnShow() 
        {
            UIMainManager.Instance.chestOpenPanel.SetGiftInfor(rwType, Qty, () =>
            {
                DOVirtual.DelayedCall(0.3f, () => { UIMainManager.Instance.dailyQuestPanel.Show(); });
            });
        }


       
    }
}
public enum QuestRewardType
{
    Daily,
    Weekly,
    Monthly,

}