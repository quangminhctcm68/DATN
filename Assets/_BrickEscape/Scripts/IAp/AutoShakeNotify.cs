using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BrickEscape
{
    public class AutoShakeNotify : MonoBehaviour
    {


        [SerializeField] private float _delay = 2f;
        [SerializeField] private bool _loop = true;

        private Coroutine _shakeRoutine;

        private void OnEnable()
        {
            StartShakeLoop();
        }

        private void OnDisable()
        {
            StopShakeLoop();
        }

        void StartShakeLoop()
        {
            StopShakeLoop();
            _shakeRoutine = StartCoroutine(ShakeRoutine());
        }

        void StopShakeLoop()
        {
            if (_shakeRoutine != null)
            {
                StopCoroutine(_shakeRoutine);
                _shakeRoutine = null;
            }
        }

        IEnumerator ShakeRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(_delay);

                if (!gameObject.activeInHierarchy) yield break;

                Shake(transform);

                if (!_loop) yield break;
            }
        }

        public void Shake(Transform trans)
        {
            trans.DOKill();

            Sequence seq = DOTween.Sequence();

            seq.Append(trans.DORotate(new Vector3(0, 0, 20), 0.08f).SetRelative())
               .Append(trans.DORotate(new Vector3(0, 0, -40), 0.08f).SetRelative())
               .Append(trans.DORotate(new Vector3(0, 0, 30), 0.08f).SetRelative())
               .Append(trans.DORotate(new Vector3(0, 0, -30), 0.08f).SetRelative())
               .Append(trans.DORotate(new Vector3(0, 0, 20), 0.05f).SetRelative());
        }
    }
}

