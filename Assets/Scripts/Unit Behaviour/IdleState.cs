using System;
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
        private GatherState gatherState;
        private MoveState moveState;

        public IdleState(Unit unit)
        {
            this.unit = unit;
        }

        public override void Update()
        {
            if (GatherSomething()) return;
            if (unit.carriedResource.type != ResourceType.None && unit.carriedResource.value > 0)
                ReturnResources();
            else
                WalkSomewhere();
        }

        private void ReturnResources()
        {
            moveState = PushMoveState(unit, unit.UnitTeam.baseBuilding.entrance.position, unit.CurrentJob.gatheringDistance);
            moveState.OnStateFinish += MoveToBaseFinishedHandler;
        }

        private void MoveToBaseFinishedHandler(State state)
        {
            unit.LoadResourcesToBase();
            MoveStateFinishedHandler(state);
        }
        
        private bool GatherSomething()
        {
            var resourceTypes = Resource.GetResourceTypes();
            var priorityGatherRadius = unit.CurrentJob.priorityGatherRadius;
            var basePosition = unit.UnitTeam.baseBuilding.transform.position;
            var nodesInPriorityRadius = resourceTypes.ToDictionary(type => type, type => Map.Instance.GetResourceNodesOfType(type).Where(node => Vector3.Distance(basePosition, node.transform.position) <= priorityGatherRadius));
            var possibleTypesToGather = nodesInPriorityRadius.Where(n => n.Value.Any()).Select(n => n.Key).Distinct().ToArray();
            ResourceType typeToGather;
            if (possibleTypesToGather.Contains(unit.carriedResource.type))
                typeToGather = unit.carriedResource.type;
            else if (possibleTypesToGather.Length == 0)
            {
                // Find closest resource type.
                var resourceNodes = new List<ResourceNode>();
                foreach (var type in resourceTypes)
                    resourceNodes.AddRange(Map.Instance.GetResourceNodesOfType(type));
                if (resourceNodes.Count == 0)
                    return false;
                ResourceNode closestNode = null;
                var minDistance = float.MaxValue;
                foreach (var node in resourceNodes)
                {
                    var distance = Vector3.Distance(node.transform.position, basePosition);
                    if (!(distance < minDistance)) continue;
                    closestNode = node;
                    minDistance = distance;
                }

                typeToGather = closestNode.Type;
            }
            else
                typeToGather = possibleTypesToGather[Random.Range(0, possibleTypesToGather.Length)];
            gatherState = new GatherState(unit, typeToGather);
            gatherState.OnStateFinish += GatherStateFinishedHandler;
            PushState(gatherState);
            return true;
        }

        private void GatherStateFinishedHandler(State state)
        {
            gatherState = null;
        }

        private void MoveStateFinishedHandler(State state)
        {
            moveState = null;
        }

        private void WalkSomewhere()
        {
            var direction = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)) * Random.Range(IdleWalkRadius.x, IdleWalkRadius.y);
            var target = unit.transform.position + direction;
            moveState = new MoveState(unit, target, 0.1f);
            moveState.OnStateFinish += MoveStateFinishedHandler;
            PushState(moveState);
        }
    }
}