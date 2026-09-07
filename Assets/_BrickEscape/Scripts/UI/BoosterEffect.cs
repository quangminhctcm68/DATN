using DG.Tweening;
using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BrickEscape
{
    public class BoosterEffect : MonoBehaviour
    {
        public Animator boosterAnimator;
        public bool isCatchMagic= false;
        public bool isHarmer = false;
        public Transform CatchPoint;
        [SerializeField] private float flyDuration = 0.4f;

        // public void PlayMagicWant()
        // {
        //     List<MoveableBlock> blocks =
        //         GameController.Instance.boosterManager.ActivateMagicWandBooster();
        //
        //     int count = Mathf.Min(3, blocks.Count);
        //
        //     for (int i = 0; i < count; i++)
        //     {
        //         ShootTrail(blocks[i].transform);
        //     }
        // }

        // private void ShootTrail(Transform target)
        // {
        //     ParticleSystem trail =
        //         GameManager.Instance.pooling.SpawnEffect(EffectID.MagicWandTrail);
        //
        //     Transform trailTran = trail.transform;
        //     trailTran.position = CatchPoint.position;
        //
        //     trail.gameObject.SetActive(true);
        //     trail.Play();
        //
        //     // Kill tween cũ nếu object pool reuse
        //     trailTran.DOKill();
        //
        //     trailTran.DOMove(target.position, flyDuration)
        //         .SetEase(Ease.OutQuad)
        //         .OnComplete(() =>
        //         {
        //             PlayExplosion(target.position);
        //             trail.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        //             trail.gameObject.SetActive(false);
        //         });
        // }

        // private void PlayExplosion(Vector3 position)
        // {
        //     ParticleSystem explosion =
        //         GameManager.Instance.pooling.SpawnEffect(EffectID.MagicWandHit);
        //
        //     explosion.transform.position = position;
        //     explosion.gameObject.SetActive(true);
        //     explosion.Play();
        // }
    }
}
