using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace dmdspirit
{
    public class IdleState : State
    {
        private NavMeshAgent agent;
        private float idleWalkRadius;
        private Unit unit;

        // TODO: Add minimal idle walk radius.
        // TODO: Adjust idle walk speed.
        public IdleState(NavMeshAgent agent, float idleWalkRadius)
        {
            unit = agent.GetComponent<Unit>();
            if (unit.IsPlayer)
                Debug.Log($"{agent.gameObject.name} started idle state.");
            this.agent = agent;
            this.idleWalkRadius = idleWalkRadius;
        }

        public override void Update()
        {
            if (unit.IsPlayer || GatherSomething() == false)
                WalkSomewhere();
        }

        private bool GatherSomething()
        {
            var notEmptyResources = (from resource in Map.Instance.resources where resource.Value.Count > 0 select resource.Key).ToList();
            if (notEmptyResources.Count == 0) return false;
            ResourceType resourceType = notEmptyResources[Random.Range(0, notEmptyResources.Count)];
            // HACK: Bad idea to GetComponent in update for units.
            var unitBehaviour = agent.GetComponent<UnitBehaviour>();
            var gatherState = new GatherState(agent, unitBehaviour.gatheringDistance, unitBehaviour.gatheringCooldown, unitBehaviour.gatheringAmount, resourceType);
            PushState(gatherState);
            return true;
        }

        private void WalkSomewhere()
        {
            var target = agent.transform.position + new Vector3(Random.Range(-1f, 1f) * idleWalkRadius, 0, Random.Range(-1f, 1f) * idleWalkRadius);
            var moveState = new MoveState(target, agent, 0.1f);
            PushState(moveState);
        }
    }
}