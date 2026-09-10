using NabaGame.Core.Runtime.FSM;
using UnityEngine;
using UnityEngine.AI;

namespace StateMachineDemo
{
    public class ZombieRunningAction : ZombieAction
    {
        // public ZombieRunningAction(Zombie zombie, FSMState owner) : base(zombie, owner)
        // {
        // }
        //
        // public override void OnEnter()
        // {
        //     zombie.agent.enabled = true;
        // }
        //
        // public override void OnUpdate(float dt)
        // {
        //     if (!zombie.agent.isOnNavMesh) return;
        //     if (zombie.target.IsVisible)
        //     {
        //         zombie.behaviour.ChangeState(AiState.FreeMove);
        //         return;
        //     }
        //     zombie.animator.Running(true,zombie.rawZombie.Speed);
        //     zombie.AgentMove(zombie.target.transform.position, true);
        //     if (Vector3.Distance(zombie.transform.position, zombie.target.transform.position) <
        //         zombie.rawZombie.RangeAttack)
        //     {
        //         zombie.behaviour.ChangeState(AiState.Attacking);
        //     }
        // }
        //
        // public override void OnExit()
        // {
        //     zombie.animator.Running(false,1);
        // }
        public ZombieRunningAction(FSMState owner) : base(owner)
        {
        }
    }
}