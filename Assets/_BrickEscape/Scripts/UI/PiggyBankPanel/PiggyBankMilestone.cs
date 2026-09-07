using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace BrickEscape
{
    public class PiggyBankMilestone : MonoBehaviour
    {
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private int milestoneValue;

        public void SetValue(int value)
        {
            milestoneValue = value;
            text.SetText(milestoneValue.ToString());
        }
        
        public int MilestoneValue => milestoneValue;
        public RectTransform RectTransform => rectTransform;
    }
}
