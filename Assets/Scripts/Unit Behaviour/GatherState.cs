using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace dmdspirit
{
    public class GatherState : State
    {
        private NavMeshAgent agent;
        private Resource target;
        private ResourceType resourceType;
        private float gatheringDistance;
        private float gatheringCooldown;
        private float gatheringTimer = 0f;
        private float gatheringAmount;
        private Unit unit;

        public GatherState(NavMeshAgent agent, float gatheringDistance, float gatheringCooldown, float gatheringAmount, ResourceType resourceType = ResourceType.None, Resource target = null)
        {
            // HACK: Not sure.
            // TODO: Subscribe to target OnResourceDepleted.
            unit = agent.GetComponent<Unit>();
            if (unit.IsPlayer)
                Debug.Log($"{agent.gameObject.name} started gather state for {resourceType.ToString()}.");
            this.agent = agent;
            this.gatheringDistance = gatheringDistance;
            this.gatheringCooldown = gatheringCooldown;
            this.gatheringAmount = gatheringAmount;
            if (target != null)
            {
                this.target = target;
                this.resourceType = target.value.type;
            }
            else
            {
                this.resourceType = resourceType;
                this.target = null;
            }

            // TODO: If no resource type is None start gathering random resource. Or go idle?
        }

        public override void Update()
        {
            if (unit.carriedResource.value >= unit.maxCarryingCapacity || (unit.carriedResource.type != resourceType && unit.carriedResource.type != ResourceType.None))
            {
                ReturnResources();
                return;
            }

            if (target == null || target.value.value == 0)
            {
                SearchForTarget();
                return;
            }

            if (Vector3.Distance(agent.transform.position, target.transform.position) > gatheringDistance)
            {
                PushMoveState(target.transform.position);
            }
            else
            {
                if (gatheringTimer >= gatheringCooldown)
                {
                    var desiredValue = Mathf.Min(gatheringAmount, unit.maxCarryingCapacity - unit.carriedResource.value);
                    var gatheredAmount = target.GatherResource(desiredValue);
                    unit.carriedResource.value += gatheredAmount;
                    if (unit.carriedResource.type == ResourceType.None)
                        unit.carriedResource.type = target.value.type;
                    gatheringTimer = 0;
                }

                gatheringTimer += Time.deltaTime;
            }
        }

        // TODO: Move to base class.
        private void PushMoveState(Vector3 moveDestination)
        {
            var moveToBaseState = new MoveState(moveDestination, agent, gatheringDistance);
            PushState(moveToBaseState);
        }

        private void ReturnResources()
        {
            var baseEntrancePosition = unit.UnitTeam.baseBuilding.entrance.position;
            if (Vector3.Distance(baseEntrancePosition, unit.transform.position) <= gatheringDistance)
            {
                unit.LoadResourcesToBase();
                return;
            }

            PushMoveState(baseEntrancePosition);
        }

        private void SearchForTarget()
        {
            if (resourceType == ResourceType.None)
            {
                Debug.LogError($"{agent.gameObject.name} is trying to search for None resource.");
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
                var newDistance = Vector3.Distance(agent.transform.position, possibleResource.transform.position);
                if (newDistance < distance)
                {
                    target = possibleResource;
                    distance = newDistance;
                }
            }
        }
    }
}