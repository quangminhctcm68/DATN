using NabaGame.Core.Runtime.FSM;
using UnityEngine;
using UnityEngine.AI;

namespace StateMachineDemo
{

    public class ZombieFreeMoveAction : ZombieAction
    {
        // [SerializeField] private float minSpawnDistance = 10f;
        // [SerializeField] private float maxSpawnDistance = 20f;
        // [SerializeField] private float navMeshSampleRadius = 5f;
        // private Vector3 destination;
        //
        // public ZombieFreeMoveAction(Zombie zombie, FSMState owner) : base(zombie, owner)
        // {
        // }
        //
        // public override void OnEnter()
        // {
        //     destination = RandomPosition();
        // }
        //
        // public override void OnUpdate(float dt)
        // {
        //     if (!zombie.target.IsVisible)
        //     {
        //         zombie.behaviour.ChangeState(AiState.Running);
        //         return;
        //     }
        //     zombie.animator.Running(true,zombie.rawZombie.Speed);
        //     zombie.AgentMove(destination, true);
        // }
        //
        // private Vector3 RandomPosition()
        // {
        //     for (int i = 0; i < 30; i++)
        //     {
        //         // Random góc và khoảng cách quanh player
        //         float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        //         float distance = Random.Range(minSpawnDistance, maxSpawnDistance);
        //         Vector3 candidate = zombie.transform.position + new Vector3(
        //             Mathf.Cos(angle) * distance, 0f, Mathf.Sin(angle) * distance);
        //
        //         // Kiểm tra nằm trên NavMesh
        //         if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, navMeshSampleRadius, NavMesh.AllAreas))
        //             return hit.position;
        //     }
        //
        //     // Fallback: trả về vị trí player + offset cố định nếu không tìm được
        //     return zombie.transform.position + zombie.transform.forward * minSpawnDistance;
        // }
        public ZombieFreeMoveAction(FSMState owner) : base(owner)
        {
        }
    }
}