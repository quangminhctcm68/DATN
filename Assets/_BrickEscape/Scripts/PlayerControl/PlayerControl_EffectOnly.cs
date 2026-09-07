using System.Collections;
using System.Collections.Generic;
using Lean.Touch;
using UnityEngine;

namespace BrickEscape
{
    public class PlayerControl_EffectOnly : MonoBehaviour
    {
        [SerializeField] private Camera cameraToUse;
        private Pooling objectPool;
        private ParticleSystem tempEffectRef;
        private Vector3 effectSpawnPos;

        public void Start()
        {
            if (cameraToUse == null) cameraToUse = CameraController.Instance.mainCamera;
            if (objectPool == null) objectPool = GameManager.Instance.pooling;
        }
        
        private void OnEnable()
        {
            LeanTouch.OnFingerDown += OnTouch;
        }
        
        private void OnDisable()
        {
            LeanTouch.OnFingerDown -= OnTouch;
        }

        // private void Update()
        // {
        //     ProcessingInput();
        // }

        void ProcessingInput()
        {
            if (Input.GetMouseButtonDown(0))
                PlayOnClickEffect(Input.mousePosition);
        }
        
        void OnTouch(LeanFinger finger)
        {
            PlayOnClickEffect(Input.mousePosition);
        }

        void PlayOnClickEffect(Vector3 pos)
        {
            UIMainManager.Instance.clickEffectPanel.PlayEffect(pos);
        }
    }
}
