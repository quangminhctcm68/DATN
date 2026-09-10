using NabaGame.Core.Runtime.FSM;
using UnityEngine;

namespace StateMachineDemo
{
    public class ZombieAttackAction : ZombieAction
    {
        // public ZombieAttackAction(Zombie zombie, FSMState owner) : base(zombie, owner)
        // {
        // }
        //
        // public override void OnEnter()
        // {
        // }
        //
        // public override void OnUpdate(float dt)
        // {
        //     if (zombie.target.IsVisible)
        //     {
        //         zombie.behaviour.ChangeState(AiState.FreeMove);
        //         return;
        //     }
        //     zombie.animator.Attacking(true, zombie.rawZombie.AttackSpeed);
        //     zombie.AgentMove(Vector3.zero, false);
        //     if (Vector3.Distance(zombie.transform.position, zombie.target.transform.position) >
        //         zombie.rawZombie.RangeAttack)
        //     {
        //         zombie.behaviour.ChangeState(AiState.Running);
        //     }
        // }
        //
        // public override void OnExit()
        // {
        //     zombie.animator.Attacking(false, 1);
        // }
        public ZombieAttackAction(FSMState owner) : base(owner)
        {
        }
    }
}