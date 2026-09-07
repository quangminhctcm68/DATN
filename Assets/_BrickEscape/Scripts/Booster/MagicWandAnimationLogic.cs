using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace BrickEscape
{
    public class MagicWandAnimationLogic : MonoBehaviour
    {
        [SerializeField] private GameObject magicWandModel;
        [SerializeField] private Animator magicWandAnimator;
        [SerializeField] private List<MoveableBlock> targets = new List<MoveableBlock>();
        [SerializeField] private string animatorParameter = "BoosterAnimationStart";
        [SerializeField] private float trailSpeed = 15f;
        
        private Vector3 middlePoint;
        private float randomizedXOffset = 0f;
        
        [SerializeField] private int currentFinishedAnimation = 0;
        [SerializeField] private int expectedFinishedAnimation = 0;

        private Coroutine hammerCoroutine;
        private Tweener wandScaleAnimation;

        private void OnValidate()
        {
            magicWandAnimator = GetComponent<Animator>();
        }

        public void SetTarget(List<MoveableBlock> blocks)
        {
            targets = blocks;
        }

        public void PlayAnimation()
        {
            CameraController.Instance.ChangeCameraControlStatus(false);
            
            StartCoroutine(WandRoutine());

            IEnumerator WandRoutine()
            {
                CameraController.Instance.MoveBackToCenter(true);
                yield return new WaitUntil(() => !CameraController.Instance.IsCameraAnimationRunning);
                
                Vector3 worldPos =
                    CameraController.Instance.mainCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.2f, 8));
                magicWandAnimator.transform.position = worldPos;
                
                magicWandAnimator.SetBool(animatorParameter, true);
                ShowWand();
            }
        }

        public void FinishAnimation()
        {
            magicWandAnimator.SetBool(animatorParameter, false);
            transform.localScale = Vector3.zero;
        }

        public void StartHittingTarget()
        {
            currentFinishedAnimation = 0;
            
            float waitTime = 0f;
            expectedFinishedAnimation = targets.Count;
            
            foreach (var target in targets)
            {
                ParticleSystem tempTrail = GameManager.Instance.pooling.SpawnEffect(EffectID.MagicWandTrail);
                tempTrail.transform.position = transform.position;

                Vector3[] path =
                {
                    transform.position,
                    GetMiddlePoint_RandomizedX(transform.position, target.BlockCenterPoint.position),
                    target.BlockCenterPoint.position
                };
                
                tempTrail.transform.DOPath(path, trailSpeed, PathType.CatmullRom).SetSpeedBased(true).SetDelay(waitTime)
                    .OnStart(delegate
                    {
                        tempTrail.Play();
                    })
                    .OnComplete(delegate
                    {
                        target.RemoveThisBlockMagically();
                        //GameManager.Instance.pooling.DespawnEffect(EffectID.MagicWandTrail, tempTrail);
                    });
                waitTime += 0.2f;
            }
            AudioManager.Instance.PlaySFX(SFXID.Booster_MagicWand);

            //WaitTillBlockRemoved();
        }

        Vector3 GetMiddlePoint_RandomizedX(Vector3 start, Vector3 end)
        {
            randomizedXOffset = Random.Range(-2f, 2f);
            Vector3 result = (start + end) / 2;
            result.x += randomizedXOffset;
            return result;
        }

        public void ReportBlockRemovalFinish()
        {
            currentFinishedAnimation++;
            if (currentFinishedAnimation >= expectedFinishedAnimation)
            {
                if (!GameController.Instance.levelGenerator.IsLevelOver)
                {
                    CameraController.Instance.ChangeCameraControlStatus(true);
                    GameController.Instance.ChangePlayerControlState(true);
                }

                GameController.Instance.boosterManager.ChangeMagicWandBoosterStatus(false);
            }
        }
        
        void ShowWand()
        {
            if (wandScaleAnimation != null)
            {
                wandScaleAnimation.Kill();
                wandScaleAnimation = null;
            }
            
            transform.localScale = Vector3.zero;
            wandScaleAnimation = transform.DOScale(Vector3.one, 0.2f)
                .OnComplete(delegate
                {
                    wandScaleAnimation = null;
                    
                });
        }
        
        public void ShowModel()
        {
            gameObject.SetActive(true);
        }
        
        public void HideModel()
        {
            gameObject.SetActive(false);
        }
    }
}
