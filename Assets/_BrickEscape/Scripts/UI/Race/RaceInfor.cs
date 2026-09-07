using NabaGame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BrickEscape
{
    public class RaceInforPanel : BaseUI
    {
        public Button CloseBtn;
        public Animator animator;
        public override void OnInAnimationStart()
        {
            base.OnInAnimationStart();
            animator.Play("ShowRaceInfor");
        }
        public void SetInfor() 
        {
            CloseBtn.onClick.AddListener(Close);
        }

        public void Close() 
        {
            Hide();
            AudioManager.Instance.PlayButtonSound();
        }
    }
}
