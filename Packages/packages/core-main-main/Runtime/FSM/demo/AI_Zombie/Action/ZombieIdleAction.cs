using DG.Tweening;
using NabaGame.Core.Runtime.FSM;
using UnityEngine;

namespace StateMachineDemo
{
    public class ZombieIdleAction : ZombieAction
    {
        // private float timer;
        //
        // public ZombieIdleAction(Zombie zombie, FSMState owner) : base(zombie, owner)
        // {
        // }
        //
        // public override void OnEnter()
        // {
        //     zombie.collider.enabled = false;
        //     timer = 0;
        //     zombie.AgentMove(Vector3.zero, false);
        //     zombie.collider.enabled = true;
        //     zombie.agent.enabled = true;
        // }
        //
        // public override void OnUpdate(float dt)
        // {
        //     timer += dt;
        //     if (timer >= 3)
        //     {
        //         zombie.behaviour.ChangeState(AiState.Running);
        //     }
        // }
        public ZombieIdleAction(FSMState owner) : base(owner)
        {
        }
    }
}