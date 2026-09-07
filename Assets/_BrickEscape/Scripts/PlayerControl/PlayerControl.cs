using System;
using System.Collections;
using System.Collections.Generic;
using Lean.Touch;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BrickEscape
{
    public class PlayerControl : MonoBehaviour
    {
        [SerializeField] private Camera cameraToUse;
        [SerializeField] private LayerMask allowedDetectionLayers;
        
        [SerializeField] private BoosterManager boosterManager;
        [SerializeField] private LevelGenerator levelGenerator;
        [SerializeField] private CameraController cameraController;
        [SerializeField] private TutorialManager tutorialManager;
        [SerializeField] private UIMainManager uiMainManager;

        private bool isHolding = false;
        private float holdTime = 0;
        
        private Vector3 effectSpawnPos;
        
        private Pooling objectPool;
        private ParticleSystem tempEffectRef;
        
        private Ray onTapRay;
        private RaycastHit2D blockHit;
        
        #region Start, Update, Validate
        public void Init()
        {
            if (cameraToUse == null) cameraToUse = CameraController.Instance.MainCamera;
            if (objectPool == null) objectPool = GameManager.Instance.pooling;
            if (levelGenerator == null) levelGenerator = GameController.Instance.levelGenerator;
            if (boosterManager == null) boosterManager = GameController.Instance.boosterManager;
            if (cameraController == null) cameraController = CameraController.Instance;
            if (tutorialManager == null) tutorialManager = GameController.Instance.tutorialManager;
            if (uiMainManager == null) uiMainManager = UIMainManager.Instance;
        }
        
        private void OnEnable()
        {
            LeanTouch.OnFingerUp += OnFingerUp;
            //LeanTouch.OnFingerDown += OnFingerDown;
            //LeanTouch.OnFingerUpdate += OnFingerUpdate;
        }

        private void OnDisable()
        {
            LeanTouch.OnFingerUp -= OnFingerUp;
            //LeanTouch.OnFingerDown -= OnFingerDown;
            //LeanTouch.OnFingerUpdate -= OnFingerUpdate;
        }
        
        // private void Update()
        // {
        //     CheckAndActivateBlock();
        // }

        private void OnFingerUpdate(LeanFinger obj)
        {
            if (isHolding && holdTime < 0.25)
                holdTime += Time.deltaTime;
        }
        
        #endregion

        #region Control Block Logic
        
        void CheckAndActivateBlock()
        {
            if (Input.GetMouseButtonDown(0))
            {
                //PlayOnClickEffect(Input.mousePosition);
                holdTime = 0;
            }
            
            if (Input.GetMouseButton(0))
            {
                if (holdTime < 0.25)
                    holdTime += Time.deltaTime;
            }
            
            if (Input.GetMouseButtonUp(0))
            {
                //if (holdTime >= 0.25) return;
                if (uiMainManager.IsGamePause()) return;
                if (cameraController.mobileTouchCamera.IsPinching) return;

                
                onTapRay = cameraToUse.ScreenPointToRay(Input.mousePosition);
                blockHit = Physics2D.GetRayIntersection(onTapRay, Mathf.Infinity, allowedDetectionLayers);
                if (blockHit.collider != null)
                {
                    MoveableBlock tempBlockRef = levelGenerator.GetBlockByID(blockHit.collider.GetInstanceID());
                    
                    if (boosterManager.HintBoosterIsActive && tempBlockRef == boosterManager.BlockToDisableHintEffect)
                    {
                        boosterManager.DeactivateHintBooster();
                    }
                    
                    if (tempBlockRef != null)
                    {
                        if (tutorialManager.IsTutorialActive)
                        {
                            if (tutorialManager.IsTutorialType_1Active && tutorialManager.IsCorrectBlock(tempBlockRef))
                            {
                                tempBlockRef.OnClick();
                                tutorialManager.StopTutorial_Type1();
                            }
                        }
                        else
                        {
                            tempBlockRef.OnClick();
                        }
                    }
                }
            }
        }

        void OnFingerDown(LeanFinger finger)
        {
            isHolding = true;
            //PlayOnClickEffect(Input.mousePosition);
            holdTime = 0;
        }

        void OnFingerHold(LeanFinger finger)
        {
            if (holdTime < 0.45)
                holdTime += Time.deltaTime;
        }

        void OnFingerUp(LeanFinger finger)
        {
            //isHolding = false;
            if (finger.StartedOverGui || finger.IsOverGui)
            {
                return;
            }
            //if (holdTime >= 0.45) return;
            if (uiMainManager.IsGamePause()) return;
            if (cameraController.mobileTouchCamera.IsPinching || cameraController.mobileTouchCamera.IsDragging) return;

                
            onTapRay = cameraToUse.ScreenPointToRay(Input.mousePosition);
            blockHit = Physics2D.GetRayIntersection(onTapRay, Mathf.Infinity, allowedDetectionLayers);
            if (blockHit.collider != null)
            {
                MoveableBlock tempBlockRef = levelGenerator.GetBlockByID(blockHit.collider.GetInstanceID());
                    
                if (boosterManager.HintBoosterIsActive && tempBlockRef == boosterManager.BlockToDisableHintEffect)
                {
                    boosterManager.DeactivateHintBooster();
                }
                    
                if (tempBlockRef != null)
                {
                    if (tutorialManager.IsTutorialActive)
                    {
                        if (tutorialManager.IsTutorialType_1Active && tutorialManager.IsCorrectBlock(tempBlockRef))
                        {
                            tempBlockRef.OnClick();
                            tutorialManager.StopTutorial_Type1();
                        }
                    }
                    else
                    {
                        tempBlockRef.OnClick();
                    }
                }
            }
        }
        
        #endregion
        
        #region Effects
        
        //void PlayOnClickEffect(Vector3 pos)
        //{
        //   UIMainManager.Instance.clickEffectPanel.PlayEffect(pos);
        //}
        
        #endregion
    }
}
