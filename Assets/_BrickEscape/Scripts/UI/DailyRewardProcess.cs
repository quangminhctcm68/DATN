using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BrickEscape
{
    public class RewardProcess : MonoBehaviour
    {
        public Image Process;
        public List<QuestReward> questRewards;
        public int Total;
        public void SetInfor()
        {
            for (int i = 0; i < questRewards.Count; i++)
            {
                questRewards[i].SetInfor();
            }
            UpdateProcess();
        }
            

        public void CheckProcess() 
        {
            for (int i = 0; i < questRewards.Count; i++) 
            {
                questRewards[i].CheckProcess();
            }
            UpdateProcessTween();
        }
        public void UpdateProcess()
        {
            if (questRewards == null || questRewards.Count == 0)
            {
                Process.fillAmount = 0;
                return;
            }

            int readyCount = 0;

            for (int i = 0; i < questRewards.Count; i++)
            {
                if (questRewards[i].isReady)
                    readyCount++;
            }

            float percent = (float)readyCount / questRewards.Count;

            Process.fillAmount = percent;
        }

        public void UpdateProcessTween()
        {
            if (questRewards == null || questRewards.Count == 0)
            {
                Process.fillAmount = 0;
                return;
            }

            int ready = 0;
            int total = questRewards.Count;

            for (int i = 0; i < total; i++)
            {
                if (questRewards[i].isReady)
                    ready++;
            }

            float newProcess = (float)ready / total;

            if (newProcess > Process.fillAmount)
            {
                Process.DOFillAmount(newProcess, 0.4f)
                   .SetEase(Ease.OutCubic);

            }
            else
            {
                Process.fillAmount = newProcess;
            }
        }

    }
}
