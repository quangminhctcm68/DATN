using DG.Tweening;
using NabaGame.Core.Runtime.FSM;
using UnityEngine;

namespace StateMachineDemo
{

    public class ZombieDieAction : ZombieAction
    {
        // public ZombieDieAction(Zombie zombie, FSMState owner) : base(zombie, owner)
        // {
        // }
        // public override void OnEnter()
        // {
        //     zombie.animator.Die();
        //     zombie.AgentMove(Vector3.zero, false);
        //     var eff = GameManager.Instance.pooling.InstantiateEffectGoldCollect();
        //     eff.transform.position = zombie.transform.position;
        //     eff.PlayEffect(zombie.rawZombie.Credit);
        //     DOVirtual.DelayedCall(5, () => { zombie.OnDestroyZombie(); });
        // }
        public ZombieDieAction(FSMState owner) : base(owner)
        {
        }
    }
}