using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace StateMachineDemo
{
    public class ZombieBehaviour : AIBehaviour<AiState>
    {
        // private Zombie zombie;
        // private ZombieTargetFinderAction targetFinderAction;
        //
        // public ZombieBehaviour(Zombie zombie, string fsmName) : base(fsmName)
        // {
        //     targetFinderAction = new ZombieTargetFinderAction(zombie);
        //     stateUsing.Add(AiState.Idle);
        //     stateUsing.Add(AiState.Running);
        //     stateUsing.Add(AiState.Attacking);
        //     stateUsing.Add(AiState.Die);
        //     stateUsing.Add(AiState.FreeMove);
        //     this.zombie = zombie;
        //     AddState();
        //     AddTransition();
        // }
        //
        // public override void Update(float dt)
        // {
        //     base.Update(dt);
        //     targetFinderAction.Update(dt);
        // }
        //
        // public override void AddState()
        // {
        //     for (int i = 0; i < stateUsing.Count; i++)
        //     {
        //         fsmState.Add(stateUsing[i], fsm.AddState(i));
        //     }
        //     var action = new ZombieIdleAction(zombie, fsmState[AiState.Idle]);
        //     var action1 = new ZombieRunningAction(zombie, fsmState[AiState.Running]);
        //     var action2 = new ZombieAttackAction(zombie, fsmState[AiState.Attacking]);
        //     var action3 = new ZombieDieAction(zombie, fsmState[AiState.Die]);
        //     var action4 = new ZombieFreeMoveAction(zombie, fsmState[AiState.FreeMove]);
        // }
        //
        // public override void StartCalculating()
        // {
        //     base.StartCalculating();
        //     fsm.Start((int)AiState.Idle);
        // }
        public ZombieBehaviour(string fsmName) : base(fsmName)
        {

        }

        public override void AddState()
        {
        }
    }
}