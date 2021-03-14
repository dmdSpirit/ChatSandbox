using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace dmdspirit
{
    public class GatherState : State
    {
        private Unit unit;
        private ResourceType resourceType;
        private ResourceNode resourceNode;
        private GatherNodeState gatherNodeState;

        public GatherState(Unit unit, ResourceType resourceType)
        {
            this.unit = unit;
            this.resourceType = resourceType;
        }

        public override void Update()
        {
            var basePosition = unit.UnitTeam.baseBuilding.transform.position;
            var priorityGatherRadius = unit.CurrentJob.priorityGatherRadius;
            var possibleResourceNodes = Map.Instance.GetResourceNodesOfType(resourceType);
            if (possibleResourceNodes.Count == 0)
            {
                StopState();
                return;
            }

            var nodeDistances = possibleResourceNodes.ToDictionary(node => node, node => Vector3.Distance(node.transform.position, basePosition));
            var nodesToChooseFrom = nodeDistances.Where(n => n.Value <= priorityGatherRadius).Select(k => k.Key).ToList();
            if (nodesToChooseFrom.Count == 0)
                resourceNode = nodeDistances.OrderBy(n => n.Value).First().Key;
            else
                resourceNode = nodesToChooseFrom[Random.Range(0, nodesToChooseFrom.Count)];
            gatherNodeState = new GatherNodeState(unit, resourceNode);
            gatherNodeState.OnStateFinish += GatherNodeStateFinishedHandler;
            PushState(gatherNodeState);
        }

        private void GatherNodeStateFinishedHandler(State state)
        {
            gatherNodeState = null;
        }
    }
}