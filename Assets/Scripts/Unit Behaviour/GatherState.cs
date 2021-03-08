using UnityEngine;

namespace dmdspirit
{
    public class GatherState : State
    {
        private Unit unit;
        private ResourceNode target;
        private ResourceType resourceType;
        private float gatheringTimer = 0f;

        private UnitJob Job => unit.CurrentJob;
        private MoveState moveState;

        public GatherState(Unit unit, ResourceType resourceType) : this(unit)
        {
            this.resourceType = resourceType;
            target = null;
        }

        public GatherState(Unit unit, ResourceNode node) : this(unit)
        {
            target = node;
            resourceType = node.value.type;
        }

        private GatherState(Unit unit)
        {
            this.unit = unit;
            if (unit.IsPlayer)
                Debug.Log($"{unit.gameObject.name} started gather state for {resourceType.ToString()}.");
        }

        private void SetTarget(ResourceNode node)
        {
            if (target != null)
                target.OnResourceDepleted -= ResourceDepletedHandler;
            resourceType = node.value.type;
            target = node;
            target.OnResourceDepleted += ResourceDepletedHandler;
        }

        // [Is it so important right now?]
        // TODO: Interrupt unit movement/resource gathering if resource node is depleted.

        // [Unit should not be able to change job mid state, unless for some debug purposes]
        // TODO: Do not store values like max carrying capacity, cause we don't know if unit can change job mid state.

        private void ResourceDepletedHandler(ResourceNode node)
        {
            moveState?.StopState(false);
            target = null;
        }

        public override void StopState(bool stopParent = true)
        {
            if (target != null)
                target.OnResourceDepleted -= ResourceDepletedHandler;
            base.StopState(stopParent);
        }

        public override void Update()
        {
            if (moveState != null) return;
            if (unit.carriedResource.value >= Job.maxCarryingCapacity || (unit.carriedResource.type != resourceType && unit.carriedResource.type != ResourceType.None))
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

            if (Vector3.Distance(unit.transform.position, target.transform.position) > Job.gatheringDistance)
            {
                PushMoveState(unit, target.transform.position, Job.gatheringDistance);
            }
            else
            {
                if (gatheringTimer >= Job.gatheringCooldown)
                {
                    var desiredValue = Mathf.Min(Job.gatheringAmount, Job.maxCarryingCapacity - unit.carriedResource.value);
                    var gatheredAmount = target.GatherResource(desiredValue);
                    unit.AddResource(resourceType, gatheredAmount);
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
            if (Vector3.Distance(baseEntrancePosition, unit.transform.position) <= Job.gatheringDistance)
            {
                unit.LoadResourcesToBase();
                return;
            }

            PushMoveState(unit, baseEntrancePosition, Job.gatheringDistance);
        }

        // TODO: Rewrite using distance priority logic.
        private void SearchForTarget()
        {
            var possibleResources = Map.Instance.GetClosestToPositionResourceNodes(unit.UnitTeam.baseBuilding.transform.position);
            // var possibleResources = Map.Instance.resources[resourceType];
            // // FIXME: Check before searching. And may be I should send unit to gather other resources instead.
            // if (possibleResources.Count == 0)
            // {
            //     if (unit.carriedResource.value == 0)
            //         StopState();
            //     else
            //         ReturnResources();
            //     return;
            // }
            //
            // // TODO: Check if there are no resources of needed type on the map left. 
            // var distance = Mathf.Infinity;
            // foreach (var possibleResource in possibleResources)
            // {
            //     var newDistance = Vector3.Distance(unit.transform.position, possibleResource.transform.position);
            //     if (newDistance < distance)
            //     {
            //         target = possibleResource;
            //         distance = newDistance;
            //     }
            // }
        }
    }
}