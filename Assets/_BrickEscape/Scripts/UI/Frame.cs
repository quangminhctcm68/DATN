using nickeltin.SDF.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BrickEscape
{
    public class Frame : MonoBehaviour
    {
        public FrameID frameID;
        public SDFImage frameImage;
        public bool isUnlocked = false;
        public Button SelectBtn;
        public GameObject Lock;
        public GameObject Tick;


        public void SetInfor() 
        {
            FrameInfor infor  = GameManager.Instance.PlayerProfile.avatarProfile.GetFrameInfor(frameID);
            isUnlocked = infor.isUnlocked;
            frameImage.RenderOutline = frameID == GameManager.Instance.PlayerProfile.currentFrame;
            Tick.SetActive(frameID == GameManager.Instance.PlayerProfile.currentFrame);    
            SelectBtn.onClick.AddListener(OnSelect);
            Lock.SetActive(!isUnlocked);
        }

        public void OnSelect() 
        {
            GameController.Instance.audioManager.PlayButtonSound();
            UIMainManager.Instance.profilePanel.SetFrame(frameID);
            UIMainManager.Instance.profilePanel.SetAcceptBtn(isUnlocked, frameID == GameManager.Instance.PlayerProfile.currentFrame);
            frameImage.RenderOutline = true;
            UIMainManager.Instance.profilePanel.CanAccept = isUnlocked && frameID != GameManager.Instance.PlayerProfile.currentFrame;

            if (!isUnlocked) 
            {
                UIMainManager.Instance.profilePanel.ShowWarning(frameID);
            }

        }
        public void OnUnselect() 
        {
            frameImage.RenderOutline = false;
        }
    }
}
