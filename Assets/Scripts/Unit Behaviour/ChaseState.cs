using UnityEngine;
using UnityEngine.AI;

namespace dmdspirit
{
    public class ChaseState : State
    {
        private NavMeshAgent agent;
        private Unit unit;
        private Unit target;
        private float stopDistance;
        private NavMeshPath path;

        public ChaseState(Unit unit, Unit target, float stopDistance)
        {
            this.unit = unit;
            this.target = target;
            this.stopDistance = stopDistance;
            agent = unit.GetComponent<NavMeshAgent>();
            path = new NavMeshPath();
            UpdatePath();
        }

        public override void Update()
        {
            if (target.IsAlive && Vector3.Distance(unit.transform.position, target.transform.position) <= stopDistance)
            {
                agent.ResetPath();
                Finish();
            }
            else
                UpdatePath();
        }

        private void UpdatePath()
        {
            if (target.IsAlive == false || agent.CalculatePath(target.transform.position, path) == false)
            {
                Finish();
                return;
            }

            agent.SetPath(path);
        }
    }
}