using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BrickEscape
{
    public class EditorTweenManager : MonoBehaviour
    {
        [SerializeField] private List<TweenData> tweenDatas = new List<TweenData>();
        
        [Button]
        private void GetAllAnimations()
        {
            DOTweenAnimation[] tweenAnimations = GetComponentsInChildren<DOTweenAnimation>();
            tweenDatas.Clear();
            
            foreach (DOTweenAnimation tweenAnimation in tweenAnimations)
            {
                TweenData tweenData = new TweenData();
                tweenData.target = tweenAnimation.transform;
                tweenData.ogPosition = tweenAnimation.transform.localPosition;
                tweenData.ogRotation = tweenAnimation.transform.localEulerAngles;
                tweenData.ogScale = tweenAnimation.transform.localScale;
                tweenData.tweenAnimation = tweenAnimation;
                tweenDatas.Add(tweenData);
            }
        }

        public void PlayAllAnimations()
        {
            foreach (TweenData tweenData in tweenDatas)
            {
                if(tweenData.target.gameObject.activeSelf)
                    tweenData.tweenAnimation.DOPlay();
            }
        }

        public void StopAllAnimations()
        {
            foreach (TweenData tweenData in tweenDatas)
            {
                tweenData.tweenAnimation.DORewind();
                tweenData.tweenAnimation.DOPause();
            }
            
            RestoreAllToOriginal();
        }
        
        void RestoreAllToOriginal()
        {
            foreach (TweenData tweenData in tweenDatas)
            {
                tweenData.target.localPosition = tweenData.ogPosition;
                tweenData.target.localEulerAngles = tweenData.ogRotation;
                tweenData.target.localScale = tweenData.ogScale;
            }
        }
    }

    [Serializable]
    public class TweenData
    {
        public Transform target;
        public Vector3 ogPosition;
        public Vector3 ogRotation;
        public Vector3 ogScale;
        public DOTweenAnimation tweenAnimation;
    }
}
