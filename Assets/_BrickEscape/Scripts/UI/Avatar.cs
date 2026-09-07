using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;
namespace BrickEscape
{
    public class Avatar : MonoBehaviour
    {
        public AvatarID avatarID;
        public Image avatarImage;
        public Image Ring;
        public bool isUnlocked = false;
        public Button SelectBtn;
        public GameObject Lock;
        public GameObject Tick;
        public void SetInfor()
        {
            AvatarInfor infor = GameManager.Instance.PlayerProfile.avatarProfile.GetAvatarInfor(avatarID);
            isUnlocked = infor.isUnlocked;
            Ring.gameObject.SetActive(avatarID == GameManager.Instance.PlayerProfile.currentAvatar);
            Tick.SetActive(avatarID == GameManager.Instance.PlayerProfile.currentAvatar);
            SelectBtn.onClick.AddListener(OnSelect);
            Lock.SetActive(!isUnlocked);
            avatarImage.sprite = GameManager.Instance.spriteCollection.avatarDic[avatarID];
        }
        public void OnSelect()
        {
            Ring.gameObject.SetActive(true);
            GameController.Instance.audioManager.PlayButtonSound();
            UIMainManager.Instance.profilePanel.SetAvatar(avatarID);
            UIMainManager.Instance.profilePanel.SetAcceptBtn(isUnlocked, avatarID == GameManager.Instance.PlayerProfile.currentAvatar);
            UIMainManager.Instance.profilePanel.CanAccept = isUnlocked && avatarID != GameManager.Instance.PlayerProfile.currentAvatar;
            
            if (!isUnlocked)
            {
                UIMainManager.Instance.profilePanel.ShowWarning(avatarID);
            }

        }
        public void OnUnselect()
        {
            Ring.gameObject.SetActive(false);
        }
    }
}
