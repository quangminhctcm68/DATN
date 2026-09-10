using NabaGame.Core.Runtime.FSM;
using UnityEngine;

namespace StateMachineDemo
{
    public class ZombieAction : FSMAction
    {

        // protected Zombie zombie;
        //
        // public ZombieAction(Zombie zombie, FSMState owner) : base(owner)
        // {
        //     this.zombie = zombie;
        // }
        public ZombieAction(FSMState owner) : base(owner)
        {

        }
    }
}