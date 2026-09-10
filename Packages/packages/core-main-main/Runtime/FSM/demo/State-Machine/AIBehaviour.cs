using System;
using System.Collections.Generic;
using System.Linq;
using NabaGame.Core.Runtime.FSM;

namespace StateMachineDemo
{
    public abstract class AIBehaviour<TState> : IDisposable where TState : Enum
    {
        protected FSM fsm;
        protected bool isCalculating = false;
        protected List<TState> stateUsing = new List<TState>();
        protected Dictionary<TState, FSMState> fsmState;

        public TState CurrentAiState;

        public abstract void AddState();

        public AIBehaviour(string fsmName)
        {
            stateUsing = new List<TState>();
            fsmState = new Dictionary<TState, FSMState>();
            fsm = new FSM(fsmName);
        }

        public virtual void AddTransition()
        {
            for (int i = 0; i < fsmState.Values.Count; i++)
            {
                for (int j = 0; j < fsmState.Values.Count; j++)
                {
                    if (i == j) continue;
                    fsmState.Values.ElementAt(i)
                        .AddTransition(Convert.ToInt32(stateUsing[j]), fsmState.Values.ElementAt(j));
                }
            }
        }

        public virtual void Update(float dt)
        {
            if (isCalculating)
            {
                fsm.Update(dt);
            }
        }

        public virtual void ChangeState(TState newState)
        {
            if (EqualityComparer<TState>.Default.Equals(CurrentAiState, newState)) return;
            CurrentAiState = newState;
            fsm.ChangeToState(fsmState[CurrentAiState]);
        }

        public AiState GetCurrentState()
        {
            return (AiState)fsm.GetCurrentState();
        }

        public virtual void StartCalculating()
        {
            isCalculating = true;
        }

        public virtual void Stop()
        {
            isCalculating = false;
            fsm.Destroy();
        }

        public virtual void Dispose()
        {
            fsm = null;
        }

        public void Pause(bool isPause)
        {
            isCalculating = !isPause;
        }
    }

    public enum AiState
    {
        Idle = 0,
        Running = 1,
        Attacking = 2,
        Die = 3,
        FreeMove = 4
    }
}