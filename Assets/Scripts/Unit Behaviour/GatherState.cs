using UnityEngine;
using UnityEngine.AI;

namespace dmdspirit
{
    public class GatherState : State
    {
        private Unit unit;
        private Resource target;
        private ResourceType resourceType;
        private float gatheringTimer = 0f;

        private float GatheringDistance => unit.CurrentJob.gatheringDistance;
        private float GatheringCooldown => unit.CurrentJob.gatheringCooldown;
        private float GatheringAmount => unit.CurrentJob.gatheringAmount;
        private float MaxCarryingCapacity => unit.CurrentJob.maxCarryingCapacity;

        public GatherState(Unit unit, ResourceType resourceType)
        {
            if (resourceType == ResourceType.None)
            {
                Debug.LogError($"{unit.name} is trying to gather resource of type {ResourceType.None}.");
                StopState();
                return;
            }

            // HACK: Not sure.
            // TODO: Subscribe to target OnResourceDepleted.
            this.unit = unit;
            this.resourceType = resourceType;
            target = null;
            if (unit.IsPlayer)
                Debug.Log($"{unit.gameObject.name} started gather state for {resourceType.ToString()}.");
        }

        public override void Update()
        {
            if (unit.carriedResource.value >= MaxCarryingCapacity || (unit.carriedResource.type != resourceType && unit.carriedResource.type != ResourceType.None))
            {
                ReturnResources();
                return;
            }

            // HACK: Depleted nodes can still return non null target.
            if (target == null || target.value.value == 0)
            {
                SearchForTarget();
                return;
            }

            if (Vector3.Distance(unit.transform.position, target.transform.position) > GatheringDistance)
            {
                PushMoveState(unit, target.transform.position, GatheringDistance);
            }
            else
            {
                if (gatheringTimer >= GatheringCooldown)
                {
                    var desiredValue = Mathf.Min(GatheringAmount, MaxCarryingCapacity - unit.carriedResource.value);
                    var gatheredAmount = target.GatherResource(desiredValue);
                    unit.carriedResource.value += gatheredAmount;
                    if (unit.carriedResource.type == ResourceType.None)
                        unit.carriedResource.type = target.value.type;
                    gatheringTimer = 0;
                }

                gatheringTimer += Time.deltaTime;
            }
        }

        private void ReturnResources()
        {
            var baseEntrancePosition = unit.UnitTeam.baseBuilding.entrance.position;
            if (Vector3.Distance(baseEntrancePosition, unit.transform.position) <= GatheringDistance)
            {
                unit.LoadResourcesToBase();
                return;
            }

            PushMoveState(unit, baseEntrancePosition, GatheringDistance);
        }

        private void SearchForTarget()
        {
            if (resourceType == ResourceType.None)
            {
                Debug.LogError($"{unit.name} is trying to search for None resource.");
                return;
            }

            var possibleResources = Map.Instance.resources[resourceType];
            // FIXME: Check before searching. And may be I should send unit to gather other resources instead.
            if (possibleResources.Count == 0)
            {
                if (unit.carriedResource.value == 0)
                    StopState();
                else
                    ReturnResources();
                return;
            }

            // TODO: Check if there are no resources of needed type on the map left. 
            var distance = Mathf.Infinity;
            foreach (var possibleResource in possibleResources)
            {
                var newDistance = Vector3.Distance(unit.transform.position, possibleResource.transform.position);
                if (newDistance < distance)
                {
                    target = possibleResource;
                    distance = newDistance;
                }
            }
        }
    }
}