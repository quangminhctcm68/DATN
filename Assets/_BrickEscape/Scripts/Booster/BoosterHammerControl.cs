using System.Collections;
using System.Collections.Generic;
using Lean.Touch;
using UnityEngine;

namespace BrickEscape
{
    public class BoosterHammerControl : MonoBehaviour
    {
        [SerializeField] private Camera cameraToUse;
        [SerializeField] private LayerMask allowedDetectionLayers;
        
        [SerializeField] private BoosterManager boosterManager;
        [SerializeField] private LevelGenerator levelGenerator;
        [SerializeField] private UIMainManager uiMainManager;
        [SerializeField] private CameraController cameraController;
        
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
            if (uiMainManager == null) uiMainManager = UIMainManager.Instance;
            if (cameraController == null) cameraController = CameraController.Instance;
        }
        
        // private void Update()
        // {
        //     CheckAndActivateBlock();
        // }
        
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
        
        #endregion

        #region Control Block Logic
        
        void CheckAndActivateBlock()
        {
            if (uiMainManager.IsGamePause()) return;
            if (cameraController.mobileTouchCamera.IsPinching) return;

            if (Input.GetMouseButtonDown(0))
            {
                //PlayOnClickEffect(Input.mousePosition);
                holdTime = 0;
            }
            
            if (Input.GetMouseButton(0))
            {
                if (holdTime < 1)
                    holdTime += Time.deltaTime;
            }
            
            if (Input.GetMouseButtonUp(0))
            {
                onTapRay = cameraToUse.ScreenPointToRay(Input.mousePosition);
                blockHit = Physics2D.GetRayIntersection(onTapRay, Mathf.Infinity, allowedDetectionLayers);
                if (blockHit.collider != null)
                {
                    MoveableBlock tempBlockRef = levelGenerator.GetBlockByID(blockHit.collider.GetInstanceID());
                    
                    if (boosterManager.HintBoosterIsActive && tempBlockRef == boosterManager.BlockToDisableHintEffect)
                    {
                        boosterManager.DeactivateHintBooster();
                    }
                    
                    if (tempBlockRef != null && tempBlockRef.IsBlockActive && !tempBlockRef.IsBlockMoving)
                    {
                        //spawn hammer effect here
                        if (tempBlockRef.ThisBlockIsRemoveable())
                        {
                            boosterManager.DeactivateHammerBooster(true);
                            CameraController.Instance.HammerLogic.SetTarget(tempBlockRef);
                            CameraController.Instance.HammerLogic.PlayAnimation();
                        }
                        else boosterManager.DeactivateHammerBooster(false);
                    }
                    else
                    {
                        boosterManager.DeactivateHammerBooster(false);
                    }

                    return;
                }
                
                boosterManager.DeactivateHammerBooster(false);
            }
        }
        
        void OnFingerUp(LeanFinger finger)
        {
            if (finger.StartedOverGui || finger.IsOverGui)
            {
                return;
            }
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
                    
                if (tempBlockRef != null && tempBlockRef.IsBlockActive && !tempBlockRef.IsBlockMoving)
                {
                    //spawn hammer effect here
                    if (tempBlockRef.ThisBlockIsRemoveable())
                    {
                        boosterManager.DeactivateHammerBooster(true);
                        CameraController.Instance.HammerLogic.SetTarget(tempBlockRef);
                        CameraController.Instance.HammerLogic.PlayAnimation();
                    }
                    else boosterManager.DeactivateHammerBooster(false);
                }
                else
                {
                    boosterManager.DeactivateHammerBooster(false);
                }

                return;
            }
                
            boosterManager.DeactivateHammerBooster(false);
        }
        
        #endregion
        
        #region Effects
        
        //void PlayOnClickEffect(Vector3 pos)
        //{
        //    UIMainManager.Instance.clickEffectPanel.PlayEffect(pos);
        //}
        
        #endregion
    }
}
