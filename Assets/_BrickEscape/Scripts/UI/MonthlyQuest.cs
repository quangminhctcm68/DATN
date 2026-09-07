using DG.Tweening;
using NabaGame.Tracking;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

namespace BrickEscape
{
    public class MonthlyQuest : BaseQuest
    {
        public int ID;
        public GameObject detail;
        public GameObject Complete;
        public GameObject Tick;
        public GameObject Badge;
        public Image slide;

        public Button claim;
        public AnimButton anim;
        private int Require;
        private int BadgeCount;
        int currentPocess;

        public TextMeshProUGUI process;
        public TextMeshProUGUI badgeTxt;
        public TextMeshProUGUI DesTxt;
        private RawMonthlyQuest rawDailyQuest;
        private RectTransform target;
        public void SetInfor()
        {
            rawDailyQuest = GameManager.Instance.rawMonthlyQuestData.rawMonthlyQuests[ID - 1];


            Require = rawDailyQuest.RequireWeeklyBadge;
            BadgeCount = rawDailyQuest.Reward_MonthlyBadge;
            currentPocess = GetCurrentProcess();
            isReady = currentPocess >= Require;
            currentPocess = Mathf.Clamp(currentPocess, 0, Require);
            process.text = $"{currentPocess}/{Require}";
            slide.fillAmount = currentPocess / Require;
            badgeTxt.text = $"x {BadgeCount}";
            DesTxt.text = $"Collect {Require} daily badge ";
            anim.SetColor(ButtonColor.Green_Up, ButtonColor.Green_Down, FontColor.Green);
            CheckProcess();
            claim.onClick.AddListener(Claim);
            target = UIMainManager.Instance.dailyQuestPanel.monthlybadge;
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
            isDone = GameManager.Instance.PlayerProfile.QuestProfile.GetMonthlyQuest(ID);
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
            return GameManager.Instance.PlayerProfile.QuestProfile.CollectWeeklyBadge;
        }


        public void Claim()
        {
            AudioManager.Instance.PlayButtonSound();
            GameManager.Instance.PlayerProfile.QuestProfile.AddMonthlyBadgeCount(BadgeCount);
            GameManager.Instance.PlayerProfile.QuestProfile.MonthlyQuest[ID] = true;
            anim.ResetAnim();
            DOVirtual.DelayedCall(0.3f, () => {
                UIMainManager.Instance.dailyQuestPanel.CheckInfor();
            });
            DOVirtual.DelayedCall(1f, () =>
            {
     
                UIMainManager.Instance.dailyQuestPanel.monthlyProcess.CheckProcess();
            });
            UIMainManager.Instance.clickEffectPanel.PlayMonthlyEffect(target, BadgeCount);
           
            TrackingManager.TrackEvent(TrackingEvent.Claim_Monthly_Quest , TrackingParamter.Type, "Collect_Weekly_Badge" + Require);
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
