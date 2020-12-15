using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace dmdspirit
{
    public class IdleState : State
    {
        private NavMeshAgent agent;
        private float idleWalkRadius;

        // TODO: Add minimal idle walk radius.
        // TODO: Adjust idle walk speed.
        public IdleState(NavMeshAgent agent, float idleWalkRadius)
        {
            var unit = agent.GetComponent<Unit>();
            if (unit.IsPlayer)
                Debug.Log($"{agent.gameObject.name} started idle state.");
            this.agent = agent;
            this.idleWalkRadius = idleWalkRadius;
        }

        public override void Update()
        {
            var target = agent.transform.position + new Vector3(Random.Range(-1f, 1f) * idleWalkRadius, 0, Random.Range(-1f, 1f) * idleWalkRadius);
            var moveState = new MoveState(target, agent, 0.1f);
            PushState(moveState);
        }
    }
}