using NabaGame.Core.Runtime.EventManager;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BrickEscape
{
    public class BoosterBtn : MonoBehaviour
    {
        public Button Btn;
        public GameObject ring;
        public TextMeshProUGUI countTxt;
        public GameObject BonusReward;
        public BoosterType type;
        public int LevelUnlock;
        public GameObject Lock;
        public GameObject Count;
        public const string UnlockBoosterEvent = "UnlockBooster";
        



        void Start()
        {
            Btn.onClick.AddListener(OnClickBooster);
        }

        private void OnEnable()
        {
            EventManager.Instance.AddListener<UpdateBoosterCount>(SetInfor);
        }
        private void OnDisable()
        {
            EventManager.Instance.RemoveListener<UpdateBoosterCount>(SetInfor);
        }
        public void OnClickBooster()
        {
            if(GameManager.Instance.PlayerProfile.LevelProfile.currentLevel < LevelUnlock)  return;
            AudioManager.Instance.PlayButtonSound();
            if (BoosterNumber() <= 0) 
            {
                ShowBonus();
            }
            else
            {
                switch (type)
                {
                    case BoosterType.Hammer:
                        GameController.Instance.boosterManager.ActivateHammerBooster();
                        break;
                    case BoosterType.Hint:
                        GameController.Instance.boosterManager.ActivateHintBooster();
                        break;
                    case BoosterType.Magic:
                        GameController.Instance.boosterManager.ActivateMagicWandBooster();
                        break;
                }

            }
                

        }
        public void SetInfor(UpdateBoosterCount e = null) 
        {
            if(GameManager.Instance.PlayerProfile.LevelProfile.currentLevel < LevelUnlock) 
            {
                SetLock();
            }
            else 
            {
                if (PlayerPrefs.GetInt(UnlockBoosterEvent + type.ToString(), 0) == 0) 
                {
                    UIMainManager.Instance.unlockBoosterPanel.SetInfor(type);
                    PlayerPrefs.SetInt(UnlockBoosterEvent + type.ToString(),1);
                }
                SetUnlock();
            }



            if (BoosterNumber() > 0)
            {
                countTxt.text = BoosterNumber().ToString();
                if (BonusReward.activeSelf)
                    BonusReward.SetActive(false);
            }
            else
            {
                if (PlayerPrefs.GetInt(UnlockBoosterEvent + type.ToString(), 0) != 0)
                        BonusReward.SetActive(true);
            }

                
        }
        public int BoosterNumber()
        {
            int count = 0;
            switch (type)
            {
                case BoosterType.Hammer:
                    count = GameManager.Instance.PlayerProfile.HammerBoosterUseCount;
                    break;
                case BoosterType.Hint:
                    count = GameManager.Instance.PlayerProfile.HintBoosterUseCount;
                    break;
                case BoosterType.Magic:
                    count = GameManager.Instance.PlayerProfile.MagicWandBoosterUseCount;
                    break;
            }
            return count;
        }
        public void ShowBonus() 
        {
            UIMainManager.Instance.moreBoosterPanel.SetInfor(type);
            UIMainManager.Instance.moreBoosterPanel.Show();
        }

        public void SetLock() 
        {
            Lock.SetActive(true);
            Count.SetActive(false);
        } 
        public void SetUnlock() 
        {
            Lock.SetActive(false);
            Count.SetActive(true);
        }

        

    }
    
}
