using DG.Tweening;
using NabaGame.Tracking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BrickEscape
{
    public class DailyQuest : BaseQuest
    {
        public int ID;
        public QuestType type;
        public GameObject detail;
        public GameObject Complete;
        public GameObject Tick;
        public GameObject Badge;
        public Image slide;
        public Image Icon;
        public Button claim;
        public AnimButton anim;
        public int Require;
        private int BadgeCount;
        int currentPocess;
        public TextMeshProUGUI process;
        public TextMeshProUGUI badgeTxt;
        public TextMeshProUGUI DesTxt;
        private RawDailyQuest rawDailyQuest;
        private RectTransform target;

        public void SetInfor()
        {
            rawDailyQuest = GameManager.Instance.rawDailyQuestData.rawDailyQuests[ID - 1];
            Require = rawDailyQuest.Qty;
            BadgeCount = rawDailyQuest.Reward_DailyBadge;
            type = rawDailyQuest.QuestType;
            currentPocess = GetCurrentProcess();
            isReady = currentPocess >= Require;
            currentPocess = Mathf.Clamp(currentPocess, 0, Require);
            process.text = $"{currentPocess}/{Require}";
            slide.fillAmount = currentPocess / Require;
            badgeTxt.text = $"x {BadgeCount}";
            anim.SetColor(ButtonColor.Green_Up, ButtonColor.Green_Down, FontColor.Green);
            SetDesScription();
            CheckProcess();
            Icon.sprite = GameManager.Instance.spriteCollection.QuesrIcon[type];
            claim.onClick.AddListener(Claim);
            target = UIMainManager.Instance.dailyQuestPanel.dailybadge;
        }

        public void CheckProcess()
        {
            currentPocess = GetCurrentProcess();
            isReady = currentPocess >= Require;
            currentPocess = Mathf.Clamp(currentPocess, 0, Require);
            process.text = $"{currentPocess}/{Require}";
            slide.fillAmount = currentPocess / Require;
            badgeTxt.text = $"x{BadgeCount}";
            claim.gameObject.SetActive(isReady);
            isDone = GameManager.Instance.PlayerProfile.QuestProfile.GetDailyQuest(ID);
            if (isDone)
            {
                SetComplete();
            }
            else
            {
                if (isReady)
                {
                    SetReady();
                }
                else
                {
                    SetInProcess();
                }
            }
        }

        public int GetCurrentProcess()
        {
            int i = 0;
            switch (type)
            {
                case QuestType.Collect_Coin:
                    i = GameManager.Instance.PlayerProfile.QuestProfile.CoinCollectDaily;
                    return i;
                case QuestType.Clear_Block:
                    i = GameManager.Instance.PlayerProfile.QuestProfile.ClearBlockCountDaily;
                    return i;
                case QuestType.Watch_Ads:
                    i = GameManager.Instance.PlayerProfile.QuestProfile.WatchAdsCountDaily;
                    return i;
                case QuestType.Use_Booster:
                    i = GameManager.Instance.PlayerProfile.QuestProfile.UseBoosterCountDaily;
                    return i;
                default:
                    return i;
            }
        }

        public void SetDesScription()
        {
            switch (type)
            {
                case QuestType.Collect_Coin:
                    DesTxt.text = $"Collect {Require} coins";
                    break;
                case QuestType.Clear_Block:
                    DesTxt.text = $"Clear {Require} Blocks";
                    break;
                case QuestType.Watch_Ads:
                    DesTxt.text = $"Watch {Require} Video";
                    break;
                case QuestType.Use_Booster:
                    DesTxt.text = $"Use {Require} Booster";
                    break;
                default:
                    break;
            }
        }

        public void Claim()
        {
            AudioManager.Instance.PlayButtonSound();
            GameManager.Instance.PlayerProfile.QuestProfile.AddDailyBadgeCount(BadgeCount);
            GameManager.Instance.PlayerProfile.QuestProfile.DailyQuest[ID] = true;
            anim.ResetAnim();
           
            UIMainManager.Instance.clickEffectPanel.PlayDailyEffect(target, BadgeCount);
            DOVirtual.DelayedCall(0.3f, () => {
                UIMainManager.Instance.dailyQuestPanel.CheckInfor();
            });
        
            DOVirtual.DelayedCall(1f, () => { 
                UIMainManager.Instance.dailyQuestPanel.dailyProcess.CheckProcess();
            });
          
            TrackingManager.TrackEvent(TrackingEvent.Claim_Daily_Quest,
                TrackingParamter.Type, $"{type}_{ID}");
        }

        public void SetComplete()
        {
            anim.ResetAnim();
            claim.gameObject.SetActive(false);
            detail.gameObject.SetActive(true);
            Badge.gameObject.SetActive(false);
            Tick.gameObject.SetActive(true);
            Complete.gameObject.SetActive(true);
        }

        public void SetReady()
        {
            claim.gameObject.SetActive(true);
            detail.gameObject.SetActive(false);
            Tick.gameObject.SetActive(false);
            Complete.gameObject.SetActive(false);
            Badge.gameObject.SetActive(true);
            anim.Play();
        }

        public void SetInProcess()
        {
            detail.gameObject.SetActive(true);
            claim.gameObject.SetActive(false);
            Tick.gameObject.SetActive(false);
            Complete.gameObject.SetActive(false);
            Badge.gameObject.SetActive(true);
            anim.ResetAnim();
        }

        public void Process()
        {
        }
    }
}