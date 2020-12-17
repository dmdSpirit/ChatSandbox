using UnityEngine;
using UnityEngine.AI;

namespace dmdspirit
{
    public class MoveState : State
    {
        private Vector3 target;
        private NavMeshAgent agent;
        private float stopDistance;


        // TODO: Additional condition to check if moving should be stopped. Function delegate?
        public MoveState(Vector3 target, NavMeshAgent agent, float stopDistance)
        {
            var unit = agent.GetComponent<Unit>();
            this.target = target;
            this.agent = agent;
            this.stopDistance = stopDistance;
            var path = new NavMeshPath();
            if (agent.CalculatePath(target, path) == false)
            {
                Finish();
                return;
            }

            agent.SetPath(path);
        }

        public override void Update()
        {
            // HACK: Not sure this will work at all.
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