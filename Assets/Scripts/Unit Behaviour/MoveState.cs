using UnityEngine;
using UnityEngine.AI;

namespace dmdspirit
{
    public class MoveState : State
    {
        private Vector3 target;
        private NavMeshAgent agent;
        private float stopDistance;

        public MoveState(Unit unit, Vector3 target, float stopDistance)
        {
            this.target = target;
            agent = unit.Agent;
            this.stopDistance = stopDistance;
            var path = new NavMeshPath();
            if (agent.CalculatePath(target, path) == false)
            {
                // BUG: Does not get handled correctly by UnitBehaviour.
                Finish();
                return;
            }
            
            // NOTE: This should be changed if unit speed can be changed while it is moving (slow effect).
            agent.speed = unit.CurrentJob.movementSpeed;
            agent.SetPath(path);
        }

        public override void Update()
        {
            if (agent.path.status == NavMeshPathStatus.PathComplete)
            {
                Finish();
                return;
            }
            
            // TODO: Check if the target is reachable.
            if (Vector3.Distance(agent.transform.position, target) > stopDistance) return;
            agent.ResetPath();
            Finish();
        }
    }
}