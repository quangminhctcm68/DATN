using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MoreMountains.NiceVibrations;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BrickEscape
{
    public class MoveableBlock : BlockBase
    {
        [FoldoutGroup("Block Info")] [SerializeField] private bool isBlockActive;
        [FoldoutGroup("Block Info")] [SerializeField] private bool isBlockMoving;
        [FoldoutGroup("Block Info")] [SerializeField] private bool allowCheckingForCollision;
        [FoldoutGroup("Block Info")] [SerializeField] private bool frontIsBlocked;
        
        [FoldoutGroup("Block Collision")] [SerializeField] private LayerMask collisionLayer;
        
        [FoldoutGroup("Block Movement Info")] [SerializeField] private Vector3 ogPos;
        [FoldoutGroup("Block Movement Info")] [SerializeField] private Vector3 moveDirection;
        [FoldoutGroup("Block Movement Info")] [SerializeField] private float speed = 1f;
        
        [FoldoutGroup("Block Info")] [SerializeField] private LevelGenerator levelGenerator;

        private Color tempColor;
        
        private Tweener blockMovementAnimation;
        private Tweener blockFlashAnimation;
        private Sequence blockScaleAnimation;
        private Sequence blockClickAnimation;
        private Sequence pushAnimation;
        
        #region Start, Update, Validate
        
        [Button]
        public override void Init()
        {
            ResetArrowVisibility();
            ChangeCollisionStatus(true);
            ogPos = transform.position;
            isBlockActive = true;
            isBlockMoving = false;
            allowCheckingForCollision = false;
            ChangeTrailState(false);
            if (levelGenerator == null) levelGenerator = GameController.Instance.levelGenerator;
        }

        [Button]
        public void SetupMoveDirection()
        {
            moveDirection = GetMovementDirection();
        }

        public void HideThisBlock()
        {
            ChangeCollisionStatus(false);
        }

        #endregion
        
        #region Player Interaction Logic

        [Button]
        public override void OnClick()
        {
            if (isBlockMoving) return;
            if(GameManager.Instance.PlayerProfile.VibrationSetting)
                MMVibrationManager.Haptic(HapticTypes.LightImpact);
            StartMoving();
        }
        
        #endregion
        
        #region Block Movement Logic

        void StartMoving()
        {
            StopBlockMovement();
            CancelPushLogic();
            if (IsThereObstacleInFront())
            {
                IsFrontBlocked(true);
            }
            else
            {
                IsFrontBlocked(false);
                RestoreBlockOgColor();
            }
            //IsFrontBlocked(IsThereObstacleInFront());
            //if (currentColorIsNotOg) RestoreBlockOgColor();
            StartBlockClickAnimation();
            BlockStartMoving();
        }

        void BlockStartMoving()
        {
            isBlockMoving = true;
            allowCheckingForCollision = true;
            blockRb.WakeUp();
            ChangeTrailState(blockMovementDirection, true);
            AudioManager.Instance.PlayRandomBlockEscapeSound();
            //AudioManager.Instance.PlayRandomMoveSound();
            
            switch (blockMovementDirection)
            {
                case BlockMovement.Down or BlockMovement.Up:
                    blockMovementAnimation = transform.DOMoveY(moveDirection.y * 1000f, speed)
                        .SetEase(Ease.InQuad)
                        .SetSpeedBased(true)
                        .SetUpdate(UpdateType.Late)
                        .SetDelay(0.075f)
                        .OnComplete(delegate
                        {
                            blockMovementAnimation = null;
                            ChangeTrailState(false);
                        });
                    break;
                case BlockMovement.Left or BlockMovement.Right:
                    blockMovementAnimation = transform.DOMoveX(moveDirection.x * 1000f, speed)
                        .SetEase(Ease.InQuad)
                        .SetSpeedBased(true)
                        .SetUpdate(UpdateType.Late)
                        .SetDelay(0.075f)
                        .OnComplete(delegate
                        {
                            blockMovementAnimation = null;
                            ChangeTrailState(false);
                        });
                    break;
            }
            //blockRb.velocity = moveDirection * speed;
        }

        void BlockReturnToOldPosition()
        {
            isBlockMoving = true;
            PlayWrongMoveEffect();
            
            switch (blockMovementDirection)
            {
                case BlockMovement.Down or BlockMovement.Up:
                    blockMovementAnimation = blockRb.DOMoveY(ogPos.y, speed * 0.25f)
                        .SetSpeedBased(true)
                        .OnComplete(delegate
                        {
                            isBlockMoving = false;
                            blockMovementAnimation = null;
                            blockRb.Sleep();
                            ChangeTrailState(false);
                        });
                    break;
                case BlockMovement.Left or BlockMovement.Right:
                    blockMovementAnimation = blockRb.DOMoveX(ogPos.x, speed * 0.25f)
                        .SetSpeedBased(true)
                        .OnComplete(delegate
                        {
                            isBlockMoving = false;
                            blockMovementAnimation = null;
                            blockRb.Sleep();
                            ChangeTrailState(false);
                        });
                    break;
            }
            
        }

        Vector3 GetMovementDirection()
        {
            switch (blockMovementDirection)
            {
                case BlockMovement.Up:
                    return Vector3.up;
                case BlockMovement.Down:
                    return Vector3.down;
                case BlockMovement.Left:
                    return Vector3.left;
                case BlockMovement.Right:
                    return Vector3.right;
            }
            
            return Vector3.zero;
        }

        void IsFrontBlocked(bool status)
        {
            frontIsBlocked = status;
        }

        public void DespawnBlock()
        {
            if (!isBlockActive || !isBlockMoving || frontIsBlocked) return;
            
            StopBlockMovement();
            isBlockMoving = false;
            allowCheckingForCollision = false;
            isBlockActive = false;
            ChangeTrailState(false);
            //AudioManager.Instance.PlaySFX(SFXID.Block_Escape);
            GameManager.Instance.PlayerProfile.QuestProfile.AddDailyClearBlockCount(1);
            blockTrail.StopTrail();
            TurnOffAllTrailExtraEffect();
            GameManager.Instance.pooling.DespawnBlock(this, blockID);
            StopWaitingForPushing(true);
            levelGenerator.ReportBlockGotOut();
        }

        public void StopBlockMovement()
        {
            if (blockMovementAnimation != null)
            {
                blockMovementAnimation.Kill();
                blockMovementAnimation = null;
                isBlockMoving = false;
            }
            allowCheckingForCollision = false;
            //blockRb.velocity = Vector2.zero;
        }
        
        #endregion
        
        #region Collision

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag(UnityConstants.Tags.MoveableBlock) && allowCheckingForCollision)
            {
                MoveableBlock otherBlock = levelGenerator.GetBlockByID(other.GetInstanceID());
                if (otherBlock != null && !otherBlock.isBlockMoving)
                {
                    StopBlockMovement();
                    if (GameManager.Instance.PlayerProfile.VibrationSetting)
                        MMVibrationManager.Haptic(HapticTypes.HeavyImpact);
                    AudioManager.Instance.PlaySFX(SFXID.WrongMove);
                    BlockReturnToOldPosition();
                    InitiatePushingOtherBlock(2);
                    levelGenerator.ReportWrongMove();
                }
            }
        }

        #endregion
        
        #region Check Block Front
        
        private RaycastHit2D[] results = new RaycastHit2D[10];
        private MoveableBlock tempBlockRef;
        
        [Button]
        public bool IsThereObstacleInFront()
        {
            SetupBounds();
            ChangeCollisionStatus(false);
            
            foreach (var joint in blockJoints)
            {
                int temp = Physics2D.RaycastNonAlloc(joint.transform.position, moveDirection, results, Mathf.Infinity,
                    collisionLayer);
                if (temp > 0)
                {
                    for (int i = 0; i < temp; i++)
                    {
                        tempBlockRef = levelGenerator.GetBlockByID(results[i].collider.GetInstanceID());
                        if (tempBlockRef != null && tempBlockRef != this && !tempBlockRef.IsBlockMoving)
                        {
                            ChangeCollisionStatus(true);
                            return true;
                        }
                    }
                }
            }

            ChangeCollisionStatus(true);
            return false;
        }

        // [SerializeField] private float castDistance;
        // bool hitSomething = false;
        // private RaycastHit2D tempHit;
        // [SerializeField] private float hitDistance;
        // private void FixedUpdate()
        // {
        //     DebugCast();
        // }
        //
        // private void DebugCast()
        // {
        //     tempHit = Physics2D.BoxCast(
        //         blockBounds.center,
        //         blockBounds.size,
        //         0f,
        //         moveDirection,
        //         hitDistance,
        //         collisionLayer);
        //
        //     if (tempHit.collider != null)
        //     {
        //         hitSomething = true;
        //     }
        //     else
        //     {
        //         hitSomething = false;
        //     }
        // }
        //
        // private void OnDrawGizmos()
        // {
        //     Gizmos.color = hitSomething ? Color.red : Color.green;
        //     
        //     Vector2 origin = blockBounds.center;
        //     Vector2 endPosition = origin + (Vector2)moveDirection * hitDistance;
        //     Vector2 boxCastSize = blockBounds.size;
        //     
        //     //Gizmos.DrawWireCube(boundsOffset + (Vector2)transform.position + (Vector2)moveDirection * 0.1f, blockBounds.size * 0.95f);
        //     Gizmos.DrawWireCube(origin, boxCastSize);
        //     Gizmos.DrawWireCube(endPosition, boxCastSize);
        //     Gizmos.DrawLine(origin, endPosition);
        // }

        #endregion
        
        #region Block Hint Effect

        public void PlayHintEffect()
        {
            StopColorFlashingEffect(true);
            
            // blockFlashAnimation = blockRenderer.DOColor(Color.gray, 0.5f)
            //     .SetLoops(-1, LoopType.Yoyo)
            //     .SetEase(Ease.Linear);

            blockFlashAnimation = DOVirtual.Color(blockRenderers[0].color, Color.gray, 0.5f, (x) =>
                {
                    foreach (var blockRenderer in blockRenderers)
                    {
                        blockRenderer.color = x;
                    }
                }).SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.Linear);
        }

        public void StopColorFlashingEffect(bool restoreColor = false)
        {
            if (blockFlashAnimation != null)
            {
                blockFlashAnimation.Rewind();
                blockFlashAnimation = null;
                if (restoreColor)
                {
                    //blockRenderer.color = blockOgColor;
                    foreach (var blockRenderer in blockRenderers)
                    {
                        blockRenderer.color = blockOgColor;
                    }
                }
            }
        }
        
        #endregion
        
        #region Hammered Block Effect

        public bool ThisBlockIsRemoveable()
        {
            if (!isBlockActive) return false;
            return true;
        }
        
        public bool RemoveThisBlock()
        {
            if (!isBlockActive) return false;
            
            // isBlockMoving = false;
            // allowCheckingForCollision = false;
            // isBlockActive = false;
            // ChangeCollisionStatus(false);
            // levelGenerator.ReportBlockGotOut();
            // GameManager.Instance.pooling.DespawnBlock(this, blockID);
            ChangeTrailState(false);
            MakeBlockDisappear(true, true, CameraController.Instance.HammerLogic);
            return true;
        }
        
        #endregion
        
        #region Magic Wand Block Effect
        
        public bool RemoveThisBlockMagically()
        {
            if (!isBlockActive) return false;

            ChangeTrailState(false);
            MakeBlockDisappear(true, true, CameraController.Instance.MagicWandLogic);
            return true;
        }
        
        #endregion
        
        #region Block Wrong Move Effect

        private bool currentColorIsNotOg = false;
        
        public void PlayWrongMoveEffect()
        {
            StopColorFlashingEffect();
            
            Color colorStart = currentColorIsNotOg ? Color.gray : blockOgColor;
            Color colorTween = currentColorIsNotOg ? blockOgColor : Color.gray;
            
            foreach (var blockRenderer in blockRenderers)
            {
                blockRenderer.color = colorStart;
            }
            
            //blockRenderer.color = currentColorIsNotOg ? Color.gray : blockOgColor;
            
            // blockFlashAnimation = blockRenderer.DOColor(currentColorIsNotOg? blockOgColor : Color.gray, 0.5f)
            //     .SetLoops(currentColorIsNotOg? 8 : 7, LoopType.Yoyo)
            //     .SetEase(Ease.Linear)
            //     .OnComplete(delegate
            //     {
            //         blockFlashAnimation = null;
            //     });
            
            blockFlashAnimation = DOVirtual.Color(colorStart, colorTween, 0.5f, (x) =>
            {
                foreach (var blockRenderer in blockRenderers)
                {
                    blockRenderer.color = x;
                }
            }).SetLoops(currentColorIsNotOg? 8 : 7, LoopType.Yoyo)
            .SetEase(Ease.Linear)
            .OnComplete(delegate
            {
                blockFlashAnimation = null;
            });

            currentColorIsNotOg = !currentColorIsNotOg;
        }

        void RestoreBlockOgColor()
        {
            StopColorFlashingEffect();
            
            // blockFlashAnimation = blockRenderer.DOColor(blockOgColor, 0.5f)
            //     .SetEase(Ease.Linear)
            //     .OnComplete(delegate
            //     {
            //         blockFlashAnimation = null;
            //     });
            
            blockFlashAnimation = DOVirtual.Color(blockRenderers[0].color, blockOgColor, 0.5f, (x) =>
            {
                foreach (var blockRenderer in blockRenderers)
                {
                    blockRenderer.color = x;
                }
            }).SetEase(Ease.Linear)
            .OnComplete(delegate
            {
                blockFlashAnimation = null;
            });

            currentColorIsNotOg = true;
        }
        
        #endregion
        
        #region Appear/Disappear Animation

        public void MakeBlockAppear()
        {
            StopBlockScaleAnimation();
            ChangeCollisionStatus(false);
            
            blockSpriteCenter.transform.localScale = blockRendererOgScale;
            chosenBlockJoint.RestoreArrowAndArrowBodyColor();
            ChangeCollisionStatus(true);
        }
        
        public void MakeBlockAppear(bool withAnimation)
        {
            StopBlockScaleAnimation();
            ChangeCollisionStatus(false);

            blockScaleAnimation = DOTween.Sequence();

            blockScaleAnimation.Append(
                blockSpriteCenter.transform.DOScale(blockRendererOgScale, 0.5f)
                    .SetEase(Ease.OutBack));

            // blockScaleAnimation.Append(
            //     currentArrowRenderer.DOFade(1f, 0.5f)
            //     .SetEase(Ease.Linear));
            blockScaleAnimation.Join(
                currentArrowBodyRenderer.DOFade(arrowOgColor.a, 0.5f)
                .SetEase(Ease.Linear));

            blockScaleAnimation.OnComplete(delegate
            {
                ChangeCollisionStatus(true);
                blockScaleAnimation = null;
            });
        }

        public void MakeBlockDisappear()
        {
            StopBlockScaleAnimation();
            ChangeCollisionStatus(false);
            
            blockSpriteCenter.transform.localScale = Vector3.zero;
            if (chosenBlockJoint != null) chosenBlockJoint.FadeArrowAndArrowBody();
        }
        
        public void MakeBlockDisappear(bool withAnimation, bool alsoRemoveBlock, HammerAnimationLogic logic)
        {
            StopBlockScaleAnimation();
            ChangeCollisionStatus(false);
            CancelPushLogic();
            
            blockScaleAnimation = DOTween.Sequence();
            
            blockScaleAnimation.Append(
                blockSpriteCenter.transform.DOScale(Vector3.zero, 0.5f)
                    .SetEase(Ease.InBack));
                
            // blockScaleAnimation.Join(
            //     currentArrowRenderer.DOFade(0f, 0.5f)
            //         .SetEase(Ease.Linear));
            blockScaleAnimation.Join(
                currentArrowBodyRenderer.DOFade(0f, 0.5f)
                    .SetEase(Ease.Linear));
            
            blockScaleAnimation.OnComplete(delegate
            {
                isBlockMoving = false;
                allowCheckingForCollision = false;
                isBlockActive = false;
                levelGenerator.ReportBlockGotOut();
                blockScaleAnimation = null;
                blockTrail.StopTrail();
                TurnOffAllTrailExtraEffect();
                GameManager.Instance.pooling.DespawnBlock(this, blockID);
                StopWaitingForPushing(true);
                logic.ReportBlockRemovalFinish();
            });
        }
        
        public void MakeBlockDisappear(bool withAnimation, bool alsoRemoveBlock, MagicWandAnimationLogic logic)
        {
            StopBlockScaleAnimation();
            ChangeCollisionStatus(false);
            CancelPushLogic();
            
            blockScaleAnimation = DOTween.Sequence();
            
            blockScaleAnimation.Append(
                blockSpriteCenter.transform.DOScale(Vector3.zero, 0.5f)
                    .SetEase(Ease.InBack));
                
            // blockScaleAnimation.Join(
            //     currentArrowRenderer.DOFade(0f, 0.5f)
            //         .SetEase(Ease.Linear));
            blockScaleAnimation.Join(
                currentArrowBodyRenderer.DOFade(0f, 0.5f)
                    .SetEase(Ease.Linear));
            
            blockScaleAnimation.OnComplete(delegate
            {
                isBlockMoving = false;
                allowCheckingForCollision = false;
                isBlockActive = false;
                levelGenerator.ReportBlockGotOut();
                blockScaleAnimation = null;
                blockTrail.StopTrail();
                TurnOffAllTrailExtraEffect();
                GameManager.Instance.pooling.DespawnBlock(this, blockID);
                StopWaitingForPushing(true);
                logic.ReportBlockRemovalFinish();
            });
        }

        public void StopBlockScaleAnimation()
        {
            if (blockScaleAnimation != null)
            {
                blockScaleAnimation.Kill();
                blockScaleAnimation = null;
            }
        }
        
        #endregion
        
        #region Trail Effect

        public override void SetColor(int colorID)
        {
            base.SetColor(colorID);
            tempColor = GameManager.Instance.dataCollection.GetColorByID(colorID);
            tempColor.a = 0.5f;
            //SetTrailColor(tempColor);
        }
        
        [Button]
        public void ChangeTrailState(bool status, bool reverseVer = false)
        {
            // foreach(var joint in blockJoints)
            //     joint.ChangeTrailState(status);
            //DisableAllTrails();
            if (status)
            {
                if (!reverseVer)
                    blockTrail.SetLinePositions(GetTrail(blockMovementDirection));
                else
                    blockTrail.SetLinePositions(GetReversedTrail(blockMovementDirection));
                
                
                blockTrail.gameObject.SetActive(true);
                blockTrail.StartTrail();
                blockTrail.SetColorToTrail(blockOgColor);
            }
            else
            {
                blockTrail.StopTrail();
                TurnOffAllTrailExtraEffect();
                blockTrail.gameObject.SetActive(false);
            }
        }
        
        [Button]
        public void ChangeTrailState(BlockMovement movement, bool status, bool reverseVer = false)
        {
            // foreach(var joint in blockJoints)
            //     joint.ChangeTrailState(status);
            //DisableAllTrails();
            if (status)
            {
                TurnOffAllTrailExtraEffect();
                if (!reverseVer)
                {
                    blockTrail.SetLinePositions(GetTrail(movement));
                    TurnOnExtraTrailEffect(movement);
                }
                else
                {
                    blockTrail.SetLinePositions(GetReversedTrail(movement));
                    TurnOnExtraTrailEffect(movement);
                }
                
                
                blockTrail.gameObject.SetActive(true);
                blockTrail.StartTrail();
                blockTrail.SetColorToTrail(blockOgColor);
            }
            else
            {
                blockTrail.StopTrail();
                TurnOffAllTrailExtraEffect();
                blockTrail.gameObject.SetActive(false);
            }
        }

        public void SetTrailColor(Color color)
        {
            // foreach(var joint in blockJoints)
            //     joint.SetTrailColor(color);
            // foreach (var movementStyle in blockTrails.Keys)
            //     blockTrails[movementStyle].SetColorToTrail(color);
            blockTrail.SetColorToTrail(color);
        }

        void DisableAllTrails()
        {
            // foreach (var movementStyle in blockTrails.Keys)
            //     blockTrails[movementStyle].enabled = false;
            blockTrail.gameObject.SetActive(false);
        }

        Vector3[] GetTrail(BlockMovement blockMovement)
        {
            return GetTrailData(blockMovement);
        }

        Vector3[] GetReversedTrail(BlockMovement blockMovement)
        {
            switch (blockMovement)
            {
                case BlockMovement.Up:
                    return GetTrailData(BlockMovement.Down);
                case BlockMovement.Down:
                    return GetTrailData(BlockMovement.Up);
                case BlockMovement.Left:
                    return GetTrailData(BlockMovement.Right);
                case BlockMovement.Right:
                    return GetTrailData(BlockMovement.Left);
                default:
                    return null;
            }
        }

        Vector3[] GetTrailData(BlockMovement blockMovement)
        {
            foreach (var data in trailDataList)
            {
                if (data.movementDirection == blockMovement) return data.directionData;
            }

            return null;
        }

        void TurnOnExtraTrailEffect(BlockMovement blockMovement)
        {
            switch (blockMovement)
            {
                case BlockMovement.Up:
                    trailExtraEffectList[0].gameObject.SetActive(true);
                    break;
                case BlockMovement.Down:
                    trailExtraEffectList[1].gameObject.SetActive(true);
                    break;
                case BlockMovement.Left:
                    trailExtraEffectList[2].gameObject.SetActive(true);
                    break;
                case BlockMovement.Right:
                    trailExtraEffectList[3].gameObject.SetActive(true);
                    break;
            }
        }
        
        void TurnOffAllTrailExtraEffect()
        {
            foreach (var effect in trailExtraEffectList)
            {
                effect.gameObject.SetActive(false);
            }
        }
        
        #endregion
        
        #region Block Click Animation

        void StartBlockClickAnimation()
        {
            StopBlockClickAnimation();
            blockClickAnimation = DOTween.Sequence();
            
            blockClickAnimation
                // .Append(blockCenterPoint.DOScale(0.6f, 0.1f).SetEase(Ease.OutQuad))
                // .Join(chosenBlockJoint.transform.DOScale(0.5f, 0.1f).SetEase(Ease.OutQuad))
                // .Append(blockCenterPoint.DOScale(1.3f, 0.05f).SetEase(Ease.OutBack))
                // .Join(chosenBlockJoint.transform.DOScale(1.3f, 0.05f).SetEase(Ease.OutBack))
                // .Append(blockCenterPoint.DOScale(1f, 0.025f).SetEase(Ease.Linear))
                // .Join(chosenBlockJoint.transform.DOScale(1f, 0.025f).SetEase(Ease.Linear));
            
                .Append(blockCenterPoint.DOPunchScale(-Vector3.one * 0.3f, 0.075f, 2, 0))
                .Join(chosenBlockJoint.transform.DOPunchScale(-Vector3.one * 0.3f, 0.075f, 2, 0));

            blockClickAnimation.OnComplete(delegate
            {
                blockClickAnimation = null;
            });
        }

        void StopBlockClickAnimation()
        {
            if (blockClickAnimation != null)
            {
                blockClickAnimation.Rewind();
                blockClickAnimation.Kill();
                blockClickAnimation = null;
                blockCenterPoint.localScale = Vector3.one;
            }
        }
        
        #endregion
        
        #region Block Push Effect

        private RaycastHit2D[] detectedBlocksInFront = new RaycastHit2D[10];
        private MoveableBlock tempBlockToPushRef;
        private List<MoveableBlock> blocksToPush = new List<MoveableBlock>();
        private bool finishedPushing = false;
        private MoveableBlock blockPushStarter;
        private Vector3 pushDirection;
        private CancellationTokenSource cancelToken = new CancellationTokenSource();
        public bool FinishedPushing => finishedPushing;
        public void ResetPushStatus(){ finishedPushing = false; }

        public void CancelPushLogic()
        {
            StopWaitingForPushing();
            StopPushedAnimation();

            if (blocksToPush.Count > 0)
            {
                foreach (var block in blocksToPush)
                    block.CancelPushLogic();
            }
        }
        
        public void InitiatePushingOtherBlock(int remainingPushCount)
        {
            finishedPushing = true;
            if (remainingPushCount > 0)
            {
                blocksToPush.Clear();
                FindBlockToPushInFront();
                if (blocksToPush.Count > 0)
                    foreach (var block in blocksToPush)
                    {
                        block.ResetPushStatus();
                        block.PushOtherBlock(this, remainingPushCount - 1, moveDirection);
                    }
            }
        }
        
        public void PushOtherBlock(MoveableBlock pushBlockStarter, int remainingPushCount, Vector3 pushDirectionForAnimation)
        {
            blockPushStarter = pushBlockStarter;
            pushDirection = pushDirectionForAnimation;
            if (remainingPushCount > 0)
            {
                blocksToPush.Clear();
                FindBlockToPushInFront(true);
                if (blocksToPush.Count > 0)
                    foreach (var block in blocksToPush)
                        block.PushOtherBlock(this, remainingPushCount - 1, pushDirectionForAnimation);
            }
            else
            {
                blocksToPush.Clear();
            }

            StopWaitingForPushing();
            WaitForPush();
        }

        async void WaitForPush()
        {
            await UniTask.WaitUntil(() => blockPushStarter.FinishedPushing, cancellationToken: cancelToken.Token).SuppressCancellationThrow();
            PushedAnimation();
        }
        

        void PushedAnimation()
        {
            StopPushedAnimation();
            pushAnimation = DOTween.Sequence();
            
            Vector3 chosenBlockOgPos = chosenBlockJoint.transform.localPosition;
            pushAnimation
                .Append(
                    blockCenterPoint.DOLocalMove(pushDirection * 0.1f, 0.1f)
                        .SetRelative()
                        .SetEase(Ease.OutQuad)
                )
                .Join(chosenBlockJoint.transform.DOLocalMove(pushDirection * 0.1f, 0.1f)
                    .SetRelative()
                    .SetEase(Ease.OutQuad)
                )
                .AppendCallback(delegate
                {
                    finishedPushing = true;
                })
                .Append(
                    blockCenterPoint.DOLocalMove(blockCenterPointOGPos, 0.1f)
                        .SetEase(Ease.InBack)
                )
                .Join(
                    chosenBlockJoint.transform.DOLocalMove(chosenBlockOgPos, 0.1f)
                        .SetEase(Ease.InBack)
                    );
            
            pushAnimation
                .OnComplete(delegate
                {
                    pushAnimation = null;
                });
        }

        void StopPushedAnimation()
        {
            if (pushAnimation != null)
            {
                pushAnimation.Rewind();
                pushAnimation.Kill();
                pushAnimation = null;
            }
        }

        void StopWaitingForPushing()
        {
            if (cancelToken != null)
            {
                cancelToken.Cancel();
                //cancelToken.Dispose();
                cancelToken = new CancellationTokenSource();
            }
        }
        
        void StopWaitingForPushing(bool withDisposal)
        {
            if (cancelToken != null)
            {
                cancelToken.Cancel();
                cancelToken.Dispose();
                cancelToken = new CancellationTokenSource();
            }
        }
        
        bool FindBlockToPushInFront()
        {
            foreach (var joint in blockJoints)
            {
                int temp = Physics2D.RaycastNonAlloc(joint.transform.position, moveDirection, detectedBlocksInFront, 1,
                    collisionLayer);
                if (temp > 0)
                {
                    for (int i = 0; i < temp; i++)
                    {
                        tempBlockToPushRef = levelGenerator.GetBlockByID(detectedBlocksInFront[i].collider.GetInstanceID());
                        if (tempBlockToPushRef != null && tempBlockToPushRef != this && !tempBlockToPushRef.IsBlockMoving && !blocksToPush.Contains(tempBlockToPushRef))
                        {
                            blocksToPush.Add(tempBlockToPushRef);
                        }
                    }
                }
            }
            
            return false;
        }
        
        bool FindBlockToPushInFront(bool notStarterVer)
        {
            ChangeCollisionStatus(false);
            
            foreach (var joint in blockJoints)
            {
                int temp = Physics2D.RaycastNonAlloc(joint.transform.position, blockPushStarter.MoveDirection, detectedBlocksInFront, 1,
                    collisionLayer);
                if (temp > 0)
                {
                    for (int i = 0; i < temp; i++)
                    {
                        tempBlockToPushRef = levelGenerator.GetBlockByID(detectedBlocksInFront[i].collider.GetInstanceID());
                        if (tempBlockToPushRef != null && tempBlockToPushRef != this && !tempBlockToPushRef.IsBlockMoving && !blocksToPush.Contains(tempBlockToPushRef))
                        {
                            blocksToPush.Add(tempBlockToPushRef);
                        }
                    }
                }
            }

            ChangeCollisionStatus(true);
            return false;
        }
        
        #endregion
        
        #region Getters, Setters
        
        public Vector3 MoveDirection => moveDirection;
        public bool IsBlockMoving => isBlockMoving;
        public bool IsBlockActive => isBlockActive;
        public bool IsBlockScaleAnimationRunning => blockScaleAnimation != null;

        public void ChangeBlockActiveStatus(bool status)
        {
            isBlockActive = status;
        }

        public void StopAllAnimations()
        {
            StopBlockMovement();
            StopColorFlashingEffect(true);
            StopBlockScaleAnimation();
            StopBlockClickAnimation();
            StopWaitingForPushing(true);
            StopPushedAnimation();
        }
        
        #endregion
    }
}
