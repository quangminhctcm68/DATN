using BitBenderGames;
using DG.Tweening;
using NabaGame.Core.Runtime.EventManager;
using NabaGame.Core.Runtime.Singleton;
using NabaGame.Core.Runtime.Utils;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BrickEscape
{
    public class CameraController : Singleton<CameraController>
    {
        public Camera mainCamera;
        public MobileTouchCamera mobileTouchCamera;
        public TouchInputController touchInputController;

        [SerializeField] private Vector3 defaultZoomValue;
        [SerializeField] private Vector3 boundCenter;

        [SerializeField] private Bounds bound;
        [SerializeField] private float cameraMinZoom;
        [SerializeField] private float cameraMaxZoom;

        [SerializeField] private HammerAnimationLogic hammerLogic;
        [SerializeField] private MagicWandAnimationLogic magicWandLogic;
        [SerializeField] private ParticleSystem hintEffect;

        [SerializeField] private MoveableBlock blockToLookAt;

        private bool cameraAnimationIsRunning;
        
        private Sequence cameraAnimation;

        #region Start, Update, Validate

        public override void Init()
        {

        }

        public void OnEnable()
        {
            EventManager.Instance.AddListener<ThemeChange>(OnChangeTheme);

        }

        public void OnDisable()
        {
            EventManager.Instance.RemoveListener<ThemeChange>(OnChangeTheme);
        }


        private void OnValidate()
        {
            mobileTouchCamera = GetComponentInChildren<MobileTouchCamera>();
            touchInputController = GetComponentInChildren<TouchInputController>();
        }

        #endregion

        #region Getters, Setters

        public Camera MainCamera => mainCamera;
        public bool IsCameraAnimationRunning => cameraAnimationIsRunning;
        public HammerAnimationLogic HammerLogic => hammerLogic;
        public MagicWandAnimationLogic MagicWandLogic => magicWandLogic;
        public ParticleSystem HintEffect => hintEffect;

        #endregion

        #region Camera Logic

        public Bounds GetAllBlocksBound(List<SpriteRenderer> blockSprites)
        {
            if (blockSprites == null || blockSprites.Count == 0)
                return new Bounds(Vector3.zero, Vector3.zero);

            Bounds bounds = blockSprites[0].bounds;

            for (int i = 1; i < blockSprites.Count; i++)
                bounds.Encapsulate(blockSprites[i].bounds); // mở rộng bounds để bao luôn block tiếp theo

            return bounds;
        }

        public void FitCameraToBounds(Camera cam, Bounds bounds, float padding = 40f)
        {
            // nửa chiều cao & nửa chiều rộng của vùng cần nhìn
            float halfHeight = bounds.extents.y + padding;
            float halfWidth = bounds.extents.x + padding;

            // với camera orthographic:
            // float sizeByHeight = halfHeight;
            // float sizeByWidth = halfWidth / cam.aspect;
            //
            // float cameraSize = Mathf.Max(sizeByHeight, sizeByWidth);
            // cam.orthographicSize = cameraSize;
            float boundsWidth = bounds.size.x;
            float boundsHeight = bounds.size.y;
            
            float sizeByHeight = (boundsHeight / 2f) * (1f + 0.5f);
            float sizeByWidth = ((boundsWidth / 2f) / cam.aspect) * (1f + 0.5f);

            // Chọn giá trị lớn hơn để đảm bảo không bị cắt cạnh nào
            float finalSize = Mathf.Max(sizeByHeight, sizeByWidth);
    
            // Gán cho camera
            //cam.orthographicSize = finalSize;

            Vector2 minBound = new Vector2(
                bounds.min.x - padding - (bounds.min.x - padding) * 0.3f,
                bounds.min.y - padding - (bounds.min.y - padding) * 0.0005f
            );
            Vector2 maxBound = new Vector2(
                bounds.max.x + padding - (bounds.max.x + padding) * 0.15f,
                bounds.max.y + padding + (bounds.max.y + padding) * 0.05f
            );

            SetCameraLimit(maxBound, minBound);
            SetCameraZoomLimit(cam, bounds);

            // đưa tâm camera vào giữa mê cung (giữ nguyên Z)
            Vector3 pos = bounds.center;
            pos.z = cam.transform.position.z;
            cam.transform.position = pos - Vector3Utils.OneY;

            bound = bounds;
            boundCenter = pos;

            if (cameraMinZoom > finalSize)
                ZoomCameraAnimation(cameraMinZoom, finalSize < cameraMaxZoom? cameraMaxZoom : finalSize);
            else
                cam.orthographicSize = finalSize;
        }

        [Button]
        public void SetCameraLimit(Vector2 boundMax, Vector2 boundMin)
        {
            mobileTouchCamera.BoundaryMax = boundMax;
            mobileTouchCamera.BoundaryMin = boundMin;

            // mobileTouchCamera.BoundaryMax = new Vector2(8, 15);
            // mobileTouchCamera.BoundaryMin = new Vector2(-8, -15);
        }

        void SetCameraZoomLimit(Camera cam, Bounds bounds)
        {
            float aspect = cam.aspect;
            
            float boundsWidth = bounds.size.x;
            float boundsHeight = bounds.size.y;
            
            float sizeToFitHeight = boundsHeight / 2f;
            float sizeToFitWidth = boundsWidth / (2f * aspect);
            
            float maxZoom = Mathf.Max(sizeToFitHeight, sizeToFitWidth) * 1.2f;
            
            mobileTouchCamera.CamZoomMax = maxZoom < 10? maxZoom + 10 : maxZoom;
            mobileTouchCamera.CamZoomMin = 5f;//Mathf.Clamp(maxZoom - 20f, 5f, maxZoom - 20f);

            cameraMinZoom = maxZoom < 10? maxZoom + 10 : maxZoom;
            cameraMaxZoom = 5f;//Mathf.Clamp(maxZoom - 20f, 5f, maxZoom - 20f);
        }
        
        public void SetupCameraLimitsAndZoom(List<Vector3> spawnedPoints)
        {
            // 1. Tính toán Bounds
            float minX = float.MaxValue, maxX = float.MinValue;
            float minY = float.MaxValue, maxY = float.MinValue;

            foreach (Vector3 p in spawnedPoints) {
                minX = Mathf.Min(minX, p.x);
                maxX = Mathf.Max(maxX, p.x);
                minY = Mathf.Min(minY, p.y);
                maxY = Mathf.Max(maxY, p.y);
            }

            // 2. Set Center
            Vector3 centerPos = new Vector3((minX + maxX) / 2f, (minY + maxY) / 2f, Camera.main.transform.position.z);
            centerPos.y -= 1f;
            Camera.main.transform.position = centerPos;
            boundCenter = centerPos;

            // 3. Set Zoom
            float paddingForZoom = 7f; // Tuỳ chỉnh khoảng trống viền
            float defaultZoom = CalculateDefaultZoom(minX, maxX, minY, maxY, paddingForZoom);
    
            // Gán vào Mobile Touch Camera (Tùy theo cấu trúc package bạn gọi biến tương ứng)
            mobileTouchCamera.CamZoom = defaultZoom;
            mobileTouchCamera.CamZoomMin = defaultZoom * 0.3f;
            mobileTouchCamera.CamZoomMax = defaultZoom * 1.2f;

            // 4. Set Limits (Boundary)
            float panPadding = 10f; // Cho phép kéo lệch ra ngoài rìa bao nhiêu đơn vị
            float panPadding_Y = 12f;
            Vector2 limitMin = new Vector2(minX - panPadding, minY - panPadding - panPadding_Y);
            Vector2 limitMax = new Vector2(maxX + panPadding, maxY + panPadding + panPadding_Y);
    
            // Gán vào giới hạn của Mobile Touch Camera
            mobileTouchCamera.BoundaryMin = limitMin;
            mobileTouchCamera.BoundaryMax = limitMax;

            cameraMinZoom = defaultZoom * 0.3f;
            cameraMaxZoom = defaultZoom * 1.2f;
            
            mobileTouchCamera.ComputeCamBoundaries();
            
            if (cameraMinZoom > defaultZoom)
                ZoomCameraAnimation(cameraMinZoom, defaultZoom < cameraMaxZoom? cameraMaxZoom : defaultZoom);
            else
                mainCamera.orthographicSize = defaultZoom;
        }
        
        public float CalculateDefaultZoom(float minX, float maxX, float minY, float maxY, float padding)
        {
            float width = maxX - minX;
            float height = maxY - minY;
            float aspect = (float)Screen.width / Screen.height;

            float sizeY = height / 2f;
            float sizeX = (width / 2f) / aspect;

            // Lấy giá trị lớn hơn để đảm bảo không bị cắt viền
            float defaultZoom = Mathf.Max(sizeY, sizeX) + padding;
            return defaultZoom;
        }

        #endregion
        
        #region Manual Camera Control

        [Button]
        public void MoveBackToCenter()
        {
            MoveCamera(boundCenter);
        }

        [Button]
        public void MoveBackToCenter(bool zoomBackToDefault)
        {
            MoveCamera(boundCenter, true);
        }

        [Button]
        public void LookAtThisBlock(MoveableBlock blockToLookAt)
        {
            this.blockToLookAt = blockToLookAt;
            
            Vector2 blockCenter = blockToLookAt.GetBlockBoundsCenter();
            Vector3 whereToLook = new Vector3(blockCenter.x, blockCenter.y, mainCamera.transform.position.z);
            MoveCamera(whereToLook);
        }
        
        [Button]
        public void LookAtThisBlock(MoveableBlock blockToLookAt, bool zoomBackToDefault)
        {
            this.blockToLookAt = blockToLookAt;
            
            Vector2 blockCenter = blockToLookAt.GetBlockBoundsCenter();
            Vector3 whereToLook = new Vector3(blockCenter.x, blockCenter.y, mainCamera.transform.position.z);
            MoveCamera(whereToLook, true);
        }
        
        #endregion
        
        #region Camera Animation

        void MoveCamera(Vector3 newPos)
        {
            StopCameraAnimation();
            cameraAnimationIsRunning = true;
            
            cameraAnimation = DOTween.Sequence();

            cameraAnimation.Append(
                mainCamera.transform.DOMove(newPos, 0.75f)
            );

            cameraAnimation.OnComplete(delegate
            {
                cameraAnimation = null;
                cameraAnimationIsRunning = false;
            });
        }

        void MoveCamera(Vector3 newPos, bool zoomBackToDefault)
        {
            StopCameraAnimation();
            
            cameraAnimation = DOTween.Sequence();
            cameraAnimationIsRunning = true;

            float temp = mainCamera.orthographicSize;
            cameraAnimation
                .Append(
                    mainCamera.transform.DOMove(newPos, 0.75f)
                )
                .Join(
                    DOTween.To(
                        () => mainCamera.orthographicSize,
                        x => mainCamera.orthographicSize = x,
                        cameraMaxZoom,
                        0.75f
                        )
                );

            cameraAnimation.OnComplete(delegate
            {
                cameraAnimation = null;
                cameraAnimationIsRunning = false;
            });
        }
        
        void MoveCamera(bool zoomBackToDefault)
        {
            StopCameraAnimation();
            
            cameraAnimation = DOTween.Sequence();
            cameraAnimationIsRunning = true;

            float temp = mainCamera.orthographicSize;
            cameraAnimation
                .Append(
                    DOTween.To(
                        () => mainCamera.orthographicSize,
                        x => mainCamera.orthographicSize = x,
                        cameraMaxZoom,
                        0.3f
                    )
                );

            cameraAnimation.OnComplete(delegate
            {
                cameraAnimation = null;
                cameraAnimationIsRunning = false;
            });
        }

        void ZoomCameraAnimation(float startZoom, float endZoom)
        {
            StopCameraAnimation();
            
            cameraAnimation = DOTween.Sequence();
            cameraAnimationIsRunning = true;
            
            cameraAnimation
                .AppendCallback(delegate{mainCamera.orthographicSize = startZoom;})
                .Append(
                    DOTween.To(
                        () => mainCamera.orthographicSize,
                        x => mainCamera.orthographicSize = x,
                        endZoom,
                        0.5f
                    )
                );

            cameraAnimation.OnComplete(delegate
            {
                cameraAnimation = null;
                cameraAnimationIsRunning = false;
            });
        }

        void StopCameraAnimation()
        {
            if (cameraAnimation != null)
            {
                cameraAnimation.Kill();
                cameraAnimation = null;
            }
        }

        public void ChangeCameraControlStatus(bool status)
        {
            touchInputController.enabled = status;
        }

        #endregion


        private Tween _colorTween;

        public void SetColor(string hex)
        {
            if (mainCamera == null) return;

            if (ColorUtility.TryParseHtmlString(hex, out Color color))
            {
                mainCamera.clearFlags = CameraClearFlags.SolidColor;
                mainCamera.backgroundColor = color;
            }
            else
            {
                Debug.LogError($"Invalid color code: {hex}");
            }
        }



        public void OnChangeTheme(ThemeChange e)
        {
            ChangeTheme(e.currentTheme);
        }


        [Button]
        public void ChangeTheme(bool isDark) 
        {
            if (isDark) 
            {
                FadeToColor("#2D2E43", 0.25f);
            }
            else 
            {
                FadeToColor("#EAE6DE", 0.25f);
            }
        
        }
        public void FadeToColor(string hex, float duration)
        {
            if (mainCamera == null) return;

            if (ColorUtility.TryParseHtmlString(hex, out Color color))
            {
                mainCamera.clearFlags = CameraClearFlags.SolidColor;

                _colorTween?.Kill();

                _colorTween = DOTween.To(
                    () => mainCamera.backgroundColor,
                    x => mainCamera.backgroundColor = x,
                    color,
                    duration
                );
            }
            else
            {
                Debug.LogError($"Invalid color code: {hex}");
            }
        }






    }
}
