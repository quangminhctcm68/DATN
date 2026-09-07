using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace BrickEscape
{
    public class HammerAnimationLogic : MonoBehaviour
    {
        [SerializeField] private GameObject modelHolder;
        [SerializeField] private Animator hammerAnimator;
        [SerializeField] private MoveableBlock target;
        [SerializeField] private string animatorParameter = "BoosterAnimationStart";

        [SerializeField] private int currentFinishedAnimation = 0;
        [SerializeField] private int expectedFinishedAnimation = 1;

        private Tweener hammerScaleAnimation;
        private Coroutine hammerCoroutine;

        private void OnValidate()
        {
            hammerAnimator = GetComponent<Animator>();
        }

        public void SetTarget(MoveableBlock block)
        {
            target = block;
        }

        public void PlayAnimation()
        {
            CameraController.Instance.ChangeCameraControlStatus(false);
            StartCoroutine(HammerRoutine());
            IEnumerator HammerRoutine()
            {
                CameraController.Instance.LookAtThisBlock(target, true);
                yield return new WaitUntil(() => !CameraController.Instance.IsCameraAnimationRunning);
                Vector3 destination = target.BlockCenterPoint.position;
                destination.z = -1;
                modelHolder.transform.position = destination;
                hammerAnimator.SetBool(animatorParameter, true);
                ShowHammer();
            }
        }

        public void FinishAnimation()
        {
            hammerAnimator.SetBool(animatorParameter, false);
            modelHolder.transform.localScale = Vector3.zero;
        }

        public void HitTarget()
        {
            SpawnEffect();
            target.RemoveThisBlock();
            currentFinishedAnimation = 0;
            //WaitTillBlockRemoved();
        }

        public void ReportBlockRemovalFinish()
        {
            currentFinishedAnimation++;
            if (currentFinishedAnimation >= expectedFinishedAnimation)
            {
                CameraController.Instance.ChangeCameraControlStatus(true);
                GameController.Instance.ChangePlayerControlState(true);
                GameController.Instance.boosterManager.ChangeHammerBoosterStatus(false);
            }
        }

        void ShowHammer()
        {
            if (hammerScaleAnimation != null)
            {
                hammerScaleAnimation.Kill();
                hammerScaleAnimation = null;
            }
            
            modelHolder.transform.localScale = Vector3.zero;
            hammerScaleAnimation = modelHolder.transform.DOScale(Vector3.one, 0.2f)
                .OnComplete(delegate
                {
                    hammerScaleAnimation = null;
                    //HideModel();
                });
        }

        void SpawnEffect()
        {
            Vector3 destination = target.BlockCenterPoint.position;
            destination.z = 0;
            ParticleSystem tempEffectRef =
                GameManager.Instance.pooling.SpawnEffect(EffectID.HammerHit);
            tempEffectRef.transform.position = destination;
            tempEffectRef.transform.localScale = Vector3.one * 2f;
            tempEffectRef.Play();
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
