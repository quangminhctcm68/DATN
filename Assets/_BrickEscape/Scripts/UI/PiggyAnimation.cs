using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace BrickEscape
{
    public class PiggyAnimation : MonoBehaviour
    {
        [SerializeField] private Transform piggyMoveableTransform;
        
        [SerializeField] private float show_X_Pos;
        [SerializeField] private float hide_X_Pos;
        
        [SerializeField] private TextMeshProUGUI currentCoins;
        [SerializeField] private TextMeshProUGUI changes_TextBox_1;
        [SerializeField] private TextMeshProUGUI changes_TextBox_2;

        [SerializeField] private CanvasGroup textBox_1_CanvasGroup;
        [SerializeField] private CanvasGroup textBox_2_CanvasGroup;

        [SerializeField] private int coinToAdd = 100;
        [SerializeField] private int currentCoinValue = 0;
        
        [SerializeField] private bool addSuccessful = false;
        [SerializeField] private bool isFullAfterward = false;
        [SerializeField] private bool isAnimPlaying = false;
        
        private PiggyBankProfile piggyBankProfile;
        
        private Sequence piggyAnimSequence;

        public void PlayPiggyAnimation()
        {
            return;
            if (isAnimPlaying) return;
            isAnimPlaying = true;
            if (piggyBankProfile == null) piggyBankProfile = GameManager.Instance.PlayerProfile.PiggyBankProfile;
            
            currentCoinValue = piggyBankProfile.currentCoins;
            SetValueToText(currentCoins, $"{currentCoinValue}");
            
            addSuccessful = piggyBankProfile.IsAddMoreCoinPossible(coinToAdd);
            piggyBankProfile.AddCoinsToPiggyBank(coinToAdd);
            isFullAfterward = piggyBankProfile.IsPiggyBankFull();
            
            if (addSuccessful && isFullAfterward) PiggyAnim(coinToAdd, true);
            else if (addSuccessful) PiggyAnim(coinToAdd);
            else PiggyAnim(true);
        }

        public void HidePiggyAnimation()
        {
            return;
            PiggyAnim_Hide();
        }
        
        void PiggyAnim(int addCoin)
        {
            StopPiggyAnim();
            ResetAllTexts();
            HideTextBoxes();
            
            SetValueToText(changes_TextBox_1, $"+{addCoin}");
            
            piggyAnimSequence = DOTween.Sequence();

            piggyAnimSequence
                .Append(piggyMoveableTransform.DOLocalMoveX(show_X_Pos, 0.3f).SetEase(Ease.OutBack))
                .Join(textBox_1_CanvasGroup.DOFade(1f, 0.3f).SetDelay(0.1f).SetEase(Ease.Linear))
                .Join(DOTween.To(
                        () => currentCoinValue,
                        x => currentCoinValue = x,
                        piggyBankProfile.currentCoins,
                        0.3f)
                    .OnUpdate(delegate
                    {
                        currentCoins.SetText(currentCoinValue.ToString());
                    })
                    .SetDelay(0.5f)
                )
                //.SetDelay(0.5f)
                .AppendInterval(3f)
                .Append(textBox_1_CanvasGroup.DOFade(0f, 0.3f).SetEase(Ease.Linear))
                .Append(piggyMoveableTransform.DOLocalMoveX(hide_X_Pos, 0.3f).SetEase(Ease.Linear));
            
            piggyAnimSequence.OnComplete(delegate
            {
                piggyAnimSequence = null;
                isAnimPlaying = false;
            });
        }

        void PiggyAnim(int addCoin, bool fullAfterward)
        {
            StopPiggyAnim();
            ResetAllTexts();
            HideTextBoxes();
            
            SetValueToText(changes_TextBox_1, $"+{addCoin}");
            SetValueToText_WarningType(changes_TextBox_2, "FULL");
            
            piggyAnimSequence = DOTween.Sequence();
            
            piggyAnimSequence
                .Append(piggyMoveableTransform.DOLocalMoveX(show_X_Pos, 0.3f).SetEase(Ease.OutBack))
                .Join(textBox_1_CanvasGroup.DOFade(1f, 0.3f).SetDelay(0.1f).SetEase(Ease.Linear))
                .Join(textBox_2_CanvasGroup.DOFade(1f, 0.3f).SetDelay(0.1f).SetEase(Ease.Linear))
                .Join(DOTween.To(
                        () => currentCoinValue,
                        x => currentCoinValue = x,
                        piggyBankProfile.currentCoins,
                        0.3f)
                    .OnUpdate(delegate
                    {
                        currentCoins.SetText(currentCoinValue.ToString());
                    })
                    .SetDelay(0.5f)
                )
                .AppendInterval(3f)
                .Append(textBox_1_CanvasGroup.DOFade(0f, 0.3f).SetEase(Ease.Linear))
                .Join(textBox_2_CanvasGroup.DOFade(0f, 0.3f).SetEase(Ease.Linear))
                .Append(piggyMoveableTransform.DOLocalMoveX(hide_X_Pos, 0.3f).SetEase(Ease.Linear));

            piggyAnimSequence.OnComplete(delegate
            {
                piggyAnimSequence = null;
                isAnimPlaying = false;
            });
        }

        void PiggyAnim(bool failed)
        {
            StopPiggyAnim();
            ResetAllTexts();
            HideTextBoxes();
            
            SetValueToText(changes_TextBox_1, "+0");
            SetValueToText_WarningType(changes_TextBox_2, "FULL");
            
            piggyAnimSequence = DOTween.Sequence();
            
            piggyAnimSequence
                .Append(piggyMoveableTransform.DOLocalMoveX(show_X_Pos, 0.3f).SetEase(Ease.OutBack))
                .Join(textBox_1_CanvasGroup.DOFade(1f, 0.3f).SetDelay(0.1f).SetEase(Ease.Linear))
                .Join(textBox_2_CanvasGroup.DOFade(1f, 0.3f).SetDelay(0.1f).SetEase(Ease.Linear))
                //.SetDelay(0.5f)
                .AppendInterval(3f)
                .Append(textBox_1_CanvasGroup.DOFade(0f, 0.3f).SetEase(Ease.Linear))
                .Join(textBox_2_CanvasGroup.DOFade(0f, 0.3f).SetEase(Ease.Linear))
                .Append(piggyMoveableTransform.DOLocalMoveX(hide_X_Pos, 0.3f).SetEase(Ease.Linear));
            
            piggyAnimSequence.OnComplete(delegate
            {
                piggyAnimSequence = null;
                isAnimPlaying = false;
            });
        }

        void PiggyAnim_Hide()
        {
            if (piggyAnimSequence == null) return;
            
            StopPiggyAnim();
            HideTextBoxes();
            
            piggyAnimSequence = DOTween.Sequence();
            
            piggyAnimSequence
                .Append(piggyMoveableTransform.DOLocalMoveX(hide_X_Pos, 0.3f).SetEase(Ease.Linear));
            
            piggyAnimSequence.OnComplete(delegate
            {
                piggyAnimSequence = null;
                isAnimPlaying = false;
            });
        }

        void StopPiggyAnim()
        {
            if (piggyAnimSequence != null)
            {
                piggyAnimSequence.Kill();
                piggyAnimSequence = null;
            }
        }
        
        void SetValueToText(TextMeshProUGUI textBox, string value)
        {
            textBox.SetText(value);
        }
        
        void SetValueToText_WarningType(TextMeshProUGUI textBox, string value)
        {
            textBox.SetText($"<color=red>{value}</color>");
        }
        
        void ResetAllTexts()
        {
            changes_TextBox_1.SetText("");
            changes_TextBox_2.SetText("");
        }

        void HideTextBoxes()
        {
            textBox_1_CanvasGroup.alpha = 0;
            textBox_2_CanvasGroup.alpha = 0;
        }

        void HideSpecificTextBox(CanvasGroup canvasGroup)
        {
            canvasGroup.alpha = 0;
        }
    }
}
