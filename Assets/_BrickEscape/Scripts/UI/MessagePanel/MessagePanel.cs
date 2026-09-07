using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using NabaGame.UI;
using TMPro;
using UnityEngine;

namespace BrickEscape
{
    public class MessagePanel : BaseUI
    {
        [SerializeField] private List<string> warningMessages = new List<string>();
        [SerializeField] private float textReadSpeed = 0.04f;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private CanvasGroup textCanvasGroup;
        [SerializeField] private CanvasGroup textBackgroundCanvasGroup;
        [SerializeField] private bool messageIsActive = false;

        private Sequence messageAnimation;
        
        #region Start, Update, Validate

        public void SetInfo()
        {
            textCanvasGroup.alpha = 0f;
            textBackgroundCanvasGroup.alpha = 0f;
        }
        
        #endregion
        
        #region Message Control

        public void StartShowingMessage(string message)
        {
            StopShowingMessage();

            messageIsActive = true;
            messageText.SetText(message);
            messageAnimation = DOTween.Sequence();

            messageAnimation
                .Append(DG.Tweening.DOTweenModuleUI.DOFade(textCanvasGroup, 1, 0.3f))
                .Join(DG.Tweening.DOTweenModuleUI.DOFade(textBackgroundCanvasGroup, 1, 0.3f))
                .AppendInterval(message.Length * textReadSpeed)
                .Append(DG.Tweening.DOTweenModuleUI.DOFade(textCanvasGroup, 0, 0.3f))
                .Join(DG.Tweening.DOTweenModuleUI.DOFade(textBackgroundCanvasGroup, 0, 0.3f))
                .OnComplete(delegate
                {
                    messageAnimation = null;
                    messageIsActive = false;
                });
        }

        public void StopShowingMessage()
        {
            if (messageAnimation != null)
            {
                messageAnimation.Kill();
                messageAnimation = null;
                
                textCanvasGroup.alpha = 0f;
                textBackgroundCanvasGroup.alpha = 0f;

                messageIsActive = false;
            }
        }

        public void ShowWarningMessage()
        {
            StartShowingMessage(GetRandomWarningText());
        }
        
        #endregion
        
        #region Warning Text

        string GetRandomWarningText()
        {
            return warningMessages[Random.Range(0, warningMessages.Count)];
        }
        
        #endregion
        
        #region Getters, Setters
        
        public bool IsMessageActive => messageIsActive;
        
        #endregion
    }
}
