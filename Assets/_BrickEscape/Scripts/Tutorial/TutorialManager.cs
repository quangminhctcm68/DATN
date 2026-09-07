using System.Collections;
using System.Collections.Generic;
using BitBenderGames;
using DG.Tweening;
using JetBrains.Annotations;
using NabaGame.Core.Runtime.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BrickEscape
{
    public class TutorialManager : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer tutorialHand_1;
        [SerializeField] private SpriteRenderer tutorialHand_1_clickVer;
        [SerializeField] private SpriteRenderer tutorialHand_2;
        [SerializeField] private SpriteRenderer tutorialHand_2_clickVer;

        [FoldoutGroup("Tutorial Type 1")] [SerializeField] private bool tutorialType_1_IsActive;
        [FoldoutGroup("Tutorial Type 1")] [SerializeField] private MoveableBlock blockToChoose;
        
        [FoldoutGroup("Tutorial Type 2")] [SerializeField] private bool tutorialType_2_IsActive;
        [FoldoutGroup("Tutorial Type 2")] [SerializeField] private MobileTouchCamera mobileTouchCamera;
        [FoldoutGroup("Tutorial Type 2")] [SerializeField] private Vector3 hand_1_StartPos;
        [FoldoutGroup("Tutorial Type 2")] [SerializeField] private Vector3 hand_2_StartPos;
        [FoldoutGroup("Tutorial Type 2")] [SerializeField] private Vector3 hand_1_EndPos;
        [FoldoutGroup("Tutorial Type 2")] [SerializeField] private Vector3 hand_2_EndPos;

        //private Tweener tutorialType_1Animation;
        private Sequence tutorialType_1Animation;
        private Sequence tutorialType_2Animation;

        private Coroutine waitForPlayerAction;
        
        #region Start, Update, Validate

        public void Init()
        {
            tutorialHand_1.gameObject.SetActive(false);
            tutorialHand_2.gameObject.SetActive(false);
            mobileTouchCamera = CameraController.Instance.mobileTouchCamera;
        }
        
        #endregion
        
        #region Tutorial Logic - Type 1 - Choose Correct Block

        [Button]
        public void StartTutorial_Type1()
        {
            List<MoveableBlock> spawnedBlocks = GameController.Instance.levelGenerator.SpawnedBlocks;

            foreach (var block in spawnedBlocks)
                if (block.IsBlockActive && !block.IsBlockMoving && !block.IsThereObstacleInFront())
                {
                    blockToChoose = block;
                    break;
                }

            if (blockToChoose != null)
            {
                tutorialType_1_IsActive = true;
                HandAnimation_TutorialType_1();
            }
        }

        public void StopTutorial_Type1()
        {
            StopHandAnimation_TutorialType_1();
            tutorialType_1_IsActive = false;
            GameManager.Instance.PlayerProfile.ChangeTutorialType_1Status(true);
        }

        [Button]
        void HandAnimation_TutorialType_1()
        {
            StopHandAnimation_TutorialType_1();

            blockToChoose.SetupBounds();
            Vector3 destination = blockToChoose.GetBlockBoundsCenter();
            ResetAllHands();
            
            //tutorialHand_1.transform.position = destination + new Vector3(0, -2.5f, 0);
            tutorialHand_1.transform.position = destination + new Vector3(0.4575f + 0.6925f, -0.4175f, 0);
            tutorialHand_1.gameObject.SetActive(true);

            tutorialType_1Animation = DOTween.Sequence();
            
            // tutorialType_1Animation = tutorialHand_1.transform.DOMoveY(destination.y - 0.5f, 1f)
            //     .SetEase(Ease.InQuad)
            //     .SetLoops(-1, LoopType.Yoyo);
            
            tutorialType_1Animation
                .AppendInterval(0.25f)
                // .Append(tutorialHand_1.DOFade(0, 0.5f).SetEase(Ease.Linear))
                // .Join(tutorialHand_1_clickVer.DOFade(1, 0.5f).SetEase(Ease.Linear))
                .AppendCallback(delegate{
                    tutorialHand_1.color = ColorUtils.Faded;
                    tutorialHand_1_clickVer.color = Color.white;
                })
                .AppendInterval(0.15f)
                .AppendCallback(delegate{
                    tutorialHand_1_clickVer.color = ColorUtils.Faded;
                    tutorialHand_1.color = Color.white;
                })
                .AppendInterval(0.15f)
                .SetLoops(-1, LoopType.Restart);
        }

        void StopHandAnimation_TutorialType_1()
        {
            if (tutorialType_1Animation != null)
            {
                tutorialType_1Animation.Kill();
                tutorialType_1Animation = null;
                tutorialHand_1.gameObject.SetActive(false);
                tutorialHand_1_clickVer.color = Color.clear;
                tutorialHand_1.color = Color.white;
            }
        }
        
        #endregion
        
        #region Tutorial Logic - Type 2 - Zoom/Shrink Camera
        
        [Button]
        public void StartTutorial_Type2()
        {
            tutorialType_2_IsActive = true;
            HandAnimation_TutorialType_2();
        }
        
        public void StopTutorial_Type2()
        {
            StopHandAnimation_TutorialType_2();
            tutorialType_2_IsActive = false;
        }

        [Button]
        void HandAnimation_TutorialType_2()
        {
            StopHandAnimation_TutorialType_2();

            ResetAllHands();

            Vector3 center =
                CameraController.Instance.GetAllBlocksBound(GameController.Instance.levelGenerator.SpawnedBlockRenderers).center;
            
            //tutorialHand_1.transform.localEulerAngles = new Vector3(0, 0, 45f);
            //tutorialHand_2.transform.localEulerAngles = new Vector3(0, 0, -35f);
            
            tutorialHand_1.transform.position = hand_1_StartPos + center;
            tutorialHand_2.transform.position = hand_2_StartPos + center;

            tutorialHand_2.flipX = true;
            tutorialHand_2_clickVer.flipX = true;
            
            tutorialHand_1.color = ColorUtils.Faded;
            tutorialHand_2.color = ColorUtils.Faded;
            
            tutorialHand_1.gameObject.SetActive(true);
            tutorialHand_2.gameObject.SetActive(true);
            
            tutorialType_2Animation = DOTween.Sequence();

            tutorialType_2Animation
                .Append(tutorialHand_1.DOFade(1, 0.5f).SetEase(Ease.Linear))
                .Join(tutorialHand_2.DOFade(1, 0.5f).SetEase(Ease.Linear))
                .AppendInterval(0.15f)
                .AppendCallback(delegate{
                    tutorialHand_1.color = ColorUtils.Faded;
                    tutorialHand_1_clickVer.color = Color.white;
                    tutorialHand_2.color = ColorUtils.Faded;
                    tutorialHand_2_clickVer.color = Color.white;
                })
                .AppendInterval(0.15f)
                .Append(tutorialHand_1.transform.DOMove(hand_1_EndPos + center, 0.75f))
                .Join(tutorialHand_2.transform.DOMove(hand_2_EndPos + center, 0.75f))
                .AppendInterval(0.15f)
                .AppendCallback(delegate{
                    tutorialHand_1_clickVer.color = ColorUtils.Faded;
                    tutorialHand_1.color = Color.white;
                    tutorialHand_2_clickVer.color = ColorUtils.Faded;
                    tutorialHand_2.color = Color.white;
                })
                .Append(tutorialHand_1.DOFade(0, 0.5f).SetEase(Ease.Linear))
                .Join(tutorialHand_2.DOFade(0, 0.5f).SetEase(Ease.Linear))
                .SetLoops(-1, LoopType.Restart);

            if (waitForPlayerAction != null)
            {
                StopCoroutine(waitForPlayerAction);
            }
            waitForPlayerAction = StartCoroutine(WaitForPlayerAction());
        }
        
        void StopHandAnimation_TutorialType_2()
        {
            if (tutorialType_2Animation != null)
            {
                tutorialType_2Animation.Kill();
                tutorialType_2Animation = null;
                tutorialHand_1.gameObject.SetActive(false);
                tutorialHand_2.gameObject.SetActive(false);
                
                tutorialHand_1_clickVer.color = ColorUtils.Faded;
                tutorialHand_1.color = Color.white;
                tutorialHand_2_clickVer.color = ColorUtils.Faded;
                tutorialHand_2.color = Color.white;
                
                tutorialHand_2.flipX = false;
                tutorialHand_2_clickVer.flipX = false;
            }
        }

        IEnumerator WaitForPlayerAction()
        {
#if UNITY_EDITOR
            yield return new WaitForSeconds(2);
            StopTutorial_Type2();
            GameManager.Instance.PlayerProfile.ChangeTutorialType_2Status(true);
            waitForPlayerAction = null;
            yield break;
#endif

            yield return new WaitUntil(() => mobileTouchCamera.IsPinching);
            StopTutorial_Type2();
            GameManager.Instance.PlayerProfile.ChangeTutorialType_2Status(true);
            waitForPlayerAction = null;
        }
        
#endregion
        
        #region Hand Model

        void ResetAllHands()
        {
            tutorialHand_1.transform.position = Vector3.zero;
            tutorialHand_1.transform.localEulerAngles = Vector3.zero;
            tutorialHand_1.color = Color.white;
            tutorialHand_1.flipX = false;
            
            tutorialHand_2.transform.position = Vector3.zero;
            tutorialHand_2.transform.localEulerAngles = Vector3.zero;
            tutorialHand_2.color = Color.white;
            tutorialHand_2.flipX = false;
        }
        
        #endregion
        
        #region Getters, Setters

        public bool IsTutorialActive => tutorialType_1_IsActive || tutorialType_2_IsActive;
        public bool IsTutorialType_1Active => tutorialType_1_IsActive;
        public bool IsTutorialType_2Active => tutorialType_2_IsActive;
        public bool IsCorrectBlock(MoveableBlock block) => block == blockToChoose;

        #endregion
    }
}
