using NabaGame.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BrickEscape
{
    public class ReplayPanel : BaseUI
    {
        public Action action;
        public Button Close;
        public Button Replay;
        public AnimButton animBtn;
        public void SetInfor()
        {
            Close.onClick.AddListener(CloseBtn);
            Replay.onClick.AddListener(OnClickClose);
        }
        public void CloseBtn() 
        {
            Hide();
            UIMainManager.Instance.pausePanel.OpenInGame();
        }
        public void OnClickClose()
        {
            action?.Invoke();
        }   
        public void OnClickReplay(Action ac)
        {
             action = ac;
            if(GameManager.Instance.PlayerProfile.HeartProfile.CurrentHeart > 0) 
            {
                animBtn.SetColor(ButtonColor.Green_Up, ButtonColor.Green_Down, FontColor.Green);
            }
            else 
            {
                animBtn.SetColor(ButtonColor.Gray_Up, ButtonColor.Gray_Down, FontColor.Gray);
            }
            Show();
        }

    }
}
