using DG.Tweening;
using NabaGame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace BrickEscape
{
    public class JoinRacePanel : BaseUI
    {
        public Button JoinBtn;
        public Button CloseBtn;
        public void SetInfor() 
        {
            JoinBtn.onClick.AddListener(OnClick);
            CloseBtn.onClick.AddListener(Close);
            //if (!GameManager.Instance.PlayerProfile.JoinedRace) 
            //{
            //    DOVirtual.DelayedCall(0.25f, () =>
            //    {
            //        Show();
            //    });
            //}
        }
        public void Close() 
        {
            AudioManager.Instance.PlayButtonSound();
            Hide();
        }

        public void OnClick() 
        {
            Hide();
            AudioManager.Instance.PlayButtonSound();
            UIMainManager.Instance.findingPanel.Show();
            GameManager.Instance.PlayerProfile.JoinedRace = true;
            UIMainManager.Instance.racePanel.ResetCloud() ;
        }
    }
}
