using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace BrickEscape
{
    public class BattlepassAnimation : MonoBehaviour
    {
        [SerializeField] private Transform battlepassMoveableTransform;
        
        [SerializeField] private float show_X_Pos;
        [SerializeField] private float hide_X_Pos;
        
        [SerializeField] private TextMeshProUGUI currentLevel;
        [SerializeField] private TextMeshProUGUI changes_TextBox_1;
        [SerializeField] private TextMeshProUGUI changes_TextBox_2;

        [SerializeField] private CanvasGroup textBox_1_CanvasGroup;
        [SerializeField] private CanvasGroup textBox_2_CanvasGroup;

        [SerializeField] private int expToAdd = 0;
        [SerializeField] private int currentLevelValue = 0;
        
        [SerializeField] private bool addSuccessful = false;
        [SerializeField] private bool isFullAfterward = false;
        [SerializeField] private bool isAnimPlaying = false;
        
        private BattlepassManager battlepassManager;
        private LevelGenerator levelGenerator;
        
        private Sequence battlepassAnimSequence;

        public void PlayBattlepassAnimation()
        {
            return;
            //Check whether battlepass is active here
            if (isAnimPlaying) return;
            if (battlepassManager == null) battlepassManager = GameController.Instance.battlepassManager;
            if (levelGenerator == null) levelGenerator = GameController.Instance.levelGenerator;

            //if (!levelGenerator.IsThreeStarsRating) return;
            
            isAnimPlaying = true;

            currentLevelValue = battlepassManager.CurrentBattlepassLevel;
            expToAdd = levelGenerator.IsCurrentLevelHard ? 2 : 1;
            
            SetValueToText(currentLevel, $"Lv{currentLevelValue}");
            SetValueToText(changes_TextBox_1, $"+{expToAdd}");

            if (battlepassManager.IsThereUnclaimedReward())
            {
                BattlepassAnim(true);
            }
            else
            {
                BattlepassAnim();
            }
        }

        public void HideBattlepassAnimation()
        {
            return;
            BattlepassAnim_Hide();
        }
        
        void BattlepassAnim()
        {
            StopBattlepassAnim();
            //ResetAllTexts();
            HideTextBoxes();
            
            battlepassAnimSequence = DOTween.Sequence();

            battlepassAnimSequence
                .Append(battlepassMoveableTransform.DOLocalMoveX(show_X_Pos, 0.3f).SetEase(Ease.OutBack))
                .Join(textBox_1_CanvasGroup.DOFade(1f, 0.3f).SetDelay(0.1f).SetEase(Ease.Linear))
                .Append(textBox_1_CanvasGroup.DOFade(0f, 0.3f).SetDelay(3f).SetEase(Ease.Linear))
                .Append(battlepassMoveableTransform.DOLocalMoveX(hide_X_Pos, 0.3f).SetEase(Ease.Linear));
            
            battlepassAnimSequence.OnComplete(delegate
            {
                battlepassAnimSequence = null;
                isAnimPlaying = false;
            });
        }
        
        void BattlepassAnim(bool newRewardAvailable)
        {
            StopBattlepassAnim();
            //ResetAllTexts();
            HideTextBoxes();
            
            battlepassAnimSequence = DOTween.Sequence();
            
            battlepassAnimSequence
                .Append(battlepassMoveableTransform.DOLocalMoveX(show_X_Pos, 0.3f).SetEase(Ease.OutBack))
                .Join(textBox_1_CanvasGroup.DOFade(1f, 0.3f).SetDelay(0.1f).SetEase(Ease.Linear))
                .Join(textBox_2_CanvasGroup.DOFade(1f, 0.3f).SetDelay(0.1f).SetEase(Ease.Linear))
                .AppendInterval(3f)
                .Append(textBox_1_CanvasGroup.DOFade(0f, 0.3f).SetEase(Ease.Linear))
                .Join(textBox_2_CanvasGroup.DOFade(0f, 0.3f).SetEase(Ease.Linear))
                .Append(battlepassMoveableTransform.DOLocalMoveX(hide_X_Pos, 0.3f).SetEase(Ease.Linear));

            battlepassAnimSequence.OnComplete(delegate
            {
                battlepassAnimSequence = null;
                isAnimPlaying = false;
            });
        }

        void BattlepassAnim_Hide()
        {
            if (battlepassAnimSequence == null) return;
            
            StopBattlepassAnim();
            HideTextBoxes();
            
            battlepassAnimSequence = DOTween.Sequence();
            
            battlepassAnimSequence
                .Append(battlepassMoveableTransform.DOLocalMoveX(hide_X_Pos, 0.3f).SetEase(Ease.Linear));
            
            battlepassAnimSequence.OnComplete(delegate
            {
                battlepassAnimSequence = null;
                isAnimPlaying = false;
            });
        }

        void StopBattlepassAnim()
        {
            if (battlepassAnimSequence != null)
            {
                battlepassAnimSequence.Kill();
                battlepassAnimSequence = null;
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
