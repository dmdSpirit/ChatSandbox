using UnityEngine;
using UnityEngine.AI;

namespace dmdspirit
{
    public class ChaseState : State
    {
        private NavMeshAgent agent;
        private HitPoints target;
        private float stopDistance;
        private NavMeshPath path;
        private ObjectRadius targetRadius;

        public ChaseState(Unit unit, HitPoints target, float stopDistance)
        {
            this.target = target;
            this.stopDistance = stopDistance;
            agent = unit.Agent;
            agent.speed = unit.CurrentJob.movementSpeed;
            path = new NavMeshPath();
            targetRadius = target.GetComponent<ObjectRadius>();
        }

        public override void Update()
        {
            var targetPosition = targetRadius == null ? target.transform.position : targetRadius.GetClosestPoint(target.transform.position);
            if (target.IsAlive && Vector3.Distance(agent.transform.position, targetPosition) <= stopDistance)
            {
                agent.ResetPath();
                Finish();
                return;
            }

            UpdatePath(targetPosition);
        }

        private void UpdatePath(Vector3 targetPosition)
        {
            if (target.IsAlive == false || agent.CalculatePath(targetPosition, path) == false)
            {
                Finish();
                return;
            }

            agent.SetPath(path);
        }
    }
}