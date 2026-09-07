using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BrickEscape
{
    public class AutoReturnEffectToPool : MonoBehaviour
    {
        [SerializeField] private EffectID effectID;
        [SerializeField] private ParticleSystem _particleSystem;
        private void OnValidate()
        {
            _particleSystem = GetComponent<ParticleSystem>();
        }

        public void OnParticleSystemStopped()
        {
            GameManager.Instance.pooling.DespawnEffect(effectID, _particleSystem);
        }
    }
}
