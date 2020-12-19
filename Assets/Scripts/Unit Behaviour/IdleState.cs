using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace dmdspirit
{
    public class IdleState : State
    {
        private Unit unit;

        private Vector2 IdleWalkRadius => unit.CurrentJob.idleWalkRadius;

        // TODO: Add minimal idle walk radius.
        // TODO: Adjust idle walk speed.
        public IdleState(Unit unit)
        {
            this.unit = unit;
        }

        public override void Update()
        {
            if (unit.IsPlayer || GatherSomething() == false)
                WalkSomewhere();
        }

        private bool GatherSomething()
        {
            if (unit.CurrentJob.canGather == false) return false;
            var notEmptyResources = (from resource in Map.Instance.resources where resource.Value.Count > 0 select resource.Key).ToList();
            if (notEmptyResources.Count == 0) return false;
            ResourceType resourceType = notEmptyResources[Random.Range(0, notEmptyResources.Count)];
            unit.GatherResource(resourceType);
            return true;
        }

        private void WalkSomewhere()
        {
            var direction = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)) * Random.Range(IdleWalkRadius.x, IdleWalkRadius.y);
            var target = unit.transform.position + direction;
            var moveState = new MoveState(unit, target, 0.1f);
            PushState(moveState);
        }
    }
}