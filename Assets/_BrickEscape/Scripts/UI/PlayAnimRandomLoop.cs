using Spine;
using Spine.Unity;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Event = Spine.Event;
namespace BrickEscape
{
    public class PlayAnimRandomLoop : MonoBehaviour
    {
        [SerializeField] private SkeletonGraphic skeletonGraphic;
        [SerializeField] private List<string> anims;

        [Header("Coin Config")]
        [SerializeField] private int coinAmount = 10;

        private int _lastIndex = -1;

        public ParticleSystem ChargeEffect;
        public ParticleSystem CoinEffect;
        public bool isActive;
        private void Start()
        {
            skeletonGraphic.Initialize(true);
            PlayRandom();
        }

        void PlayRandom()
        {
            int index;

            do
            {
                index = Random.Range(0, anims.Count);
            }
            while (index == _lastIndex);

            _lastIndex = index;

            string animName = anims[index];

            var entry = skeletonGraphic.AnimationState.SetAnimation(0, animName, false);

            // luôn listen complete
            entry.Complete += OnAnimationComplete;

            // 🔥 CHỈ bind event nếu là CastCoin
            if (animName == "MoonRace_Cat_ CastCoin")
            {
                entry.Event += OnCastCoinEvent;
            }
        }

        // =========================
        // EVENT: CAST COIN
        // =========================
        private void OnCastCoinEvent(TrackEntry entry, Event e)
        {
            switch (e.Data.Name)
            {
                case "charge":
                    OnCharge();
                    break;

                case "boom":
                    OnBoom();
                    break;
            }
        }

        void OnCharge()
        {
            if (!isActive) return; 
            Debug.Log("⚡ Charge");
            ChargeEffect.Play();
            // TODO: bật VFX charge
            // VFXManager.Instance.PlayCharge(...)
        }

        void OnBoom()
        {
            Debug.Log("💥 Boom");
            if (!isActive) return;
            CoinEffect.Play();

        }



        // =========================
        // COMPLETE
        // =========================
        void OnAnimationComplete(TrackEntry entry)
        {
            entry.Complete -= OnAnimationComplete;

            // 🔥 clean luôn event nếu có
            entry.Event -= OnCastCoinEvent;

            PlayRandom();
        }
    }
}