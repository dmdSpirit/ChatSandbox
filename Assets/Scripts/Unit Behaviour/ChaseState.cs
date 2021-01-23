﻿using UnityEngine;
using UnityEngine.AI;

namespace dmdspirit
{
    public class ChaseState : State
    {
        private NavMeshAgent agent;
        private Unit target;
        private float stopDistance;
        private NavMeshPath path;

        public ChaseState(Unit unit, Unit target, float stopDistance)
        {
            this.target = target;
            this.stopDistance = stopDistance;
            agent = unit.Agent;
            agent.speed = unit.CurrentJob.movementSpeed;
            path = new NavMeshPath();
            UpdatePath();
        }

        public override void Update()
        {
            if (target.IsAlive() && Vector3.Distance(agent.transform.position, target.transform.position) <= stopDistance)
            {
                agent.ResetPath();
                Finish();
                return;
            }

            UpdatePath();
        }

        private void UpdatePath()
        {
            if (target.IsAlive() == false || agent.CalculatePath(target.transform.position, path) == false)
            {
                Finish();
                return;
            }

            agent.SetPath(path);
        }
    }
}