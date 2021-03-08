using System.Collections.Generic;
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
            // TODO: Get next command from command priority list in Team.
            if (unit.IsPlayer || GatherSomething() == false)
                WalkSomewhere();
        }

        private bool GatherSomething()
        {
            if (unit.CurrentJob.canGather == false) return false;
            var basePosition = unit.UnitTeam.baseBuilding.transform.position;
            var priorityGatherRadius = unit.CurrentJob.priorityGatherRadius;
            var possibleResourceNodes = Map.Instance.GetClosestToPositionResourceNodes(basePosition);
            if (possibleResourceNodes.Count == 0) return false;
            var priorityNodes = possibleResourceNodes
                .Where(p => p.Value <= priorityGatherRadius)
                .Select(p => p.Key)
                .ToList();

            foreach (var possibleResourceNodePair in possibleResourceNodes)
            {
                if (possibleResourceNodePair.Value > priorityGatherRadius) continue;
                priorityNodes.Add(possibleResourceNodePair.Key);
            }

            ResourceNode resourceToGather = null;
            if (priorityNodes.Count != 0)
                resourceToGather = priorityNodes[Random.Range(0, priorityNodes.Count)];
            else
                resourceToGather = possibleResourceNodes
                    .OrderBy(p => p.Value)
                    .Select(p => p.Key)
                    .FirstOrDefault();
            if (resourceToGather == null) return false;
            unit.GatherNode(resourceToGather);
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