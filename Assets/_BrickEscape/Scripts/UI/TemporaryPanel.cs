using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace BrickEscape
{
    public class TemporaryPanel : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI levelText;

        private void Start()
        {
            UpdateLevelText();
        }

        public void NextLevel()
        {
            //GameManager.Instance.PlayerProfile.ChangeLevel(GameManager.Instance.PlayerProfile.CurrentLevel + 1);
            
            GameController.Instance.levelGenerator.StartGeneratingLevel(true);
            UpdateLevelText();
        }

        public void PreviousLevel()
        {
            // int temp = GameManager.Instance.PlayerProfile.CurrentLevel - 1;
            // if (temp < 1) temp = 1;
            // GameManager.Instance.PlayerProfile.ChangeLevel(temp);
            
            GameController.Instance.levelGenerator.StartGeneratingLevel(false);
            UpdateLevelText();
        }

        public void ClearLevel()
        {
            GameController.Instance.levelGenerator.ClearEverything();
        }

        void UpdateLevelText()
        {
            levelText.SetText($"Level {GameController.Instance.levelGenerator.loadedLevel}");
        }
    }
}
