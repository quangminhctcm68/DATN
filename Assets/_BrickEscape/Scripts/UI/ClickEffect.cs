using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BrickEscape
{
    public class ClickEffect : MonoBehaviour
    { 
        public ParticleSystem _particleSystem;
        WaitForSeconds waitFor;
        public RectTransform rect;
        private void Awake()
        {
            waitFor = new WaitForSeconds(1);
        }
        public void PlayEffect()
        {
            _particleSystem.Play();
            StartCoroutine(DestroyEffect());
        }
        IEnumerator DestroyEffect() 
        {
            yield return waitFor;
            GameManager.Instance.pooling.DespawnClickEffect(gameObject);
        }
    }
}
