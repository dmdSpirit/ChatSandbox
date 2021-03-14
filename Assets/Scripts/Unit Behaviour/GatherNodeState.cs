using UnityEngine;

namespace dmdspirit
{
    // Gather specific node.
    public class GatherNodeState : State
    {
        private Unit unit;
        private ResourceNode node;
        private MoveState moveState;
        private float gatheringTimer = 0f;
        private bool isReturningResources = false;
        private bool nodeHasDepleted = false;

        public GatherNodeState(Unit unit, ResourceNode node)
        {
            this.unit = unit;
            this.node = node;
            node.OnResourceDepleted += ResourceDepletedHandler;
        }

        public override void Update()
        {
            if (isReturningResources && moveState != null)
                return;
            
            if (nodeHasDepleted)
            {
                moveState?.StopState(false);
                moveState = null;
                node = null;
                StopState();
                return;
            }

            if (moveState != null)
                return;

            if (unit.carriedResource.type!= ResourceType.None && unit.carriedResource.type != node.Type || unit.carriedResource.value >= unit.CurrentJob.maxCarryingCapacity)
            {
                var baseEntrance = unit.UnitTeam.baseBuilding.entrance.position;
                if (Vector3.Distance(baseEntrance, unit.transform.position) <= unit.CurrentJob.gatheringDistance)
                {
                    unit.LoadResourcesToBase();
                    return;
                }

                moveState = PushMoveState(unit, baseEntrance, unit.CurrentJob.gatheringDistance);
                moveState.OnStateFinish += MoveToBaseStateFinishedHandler;
                isReturningResources = true;
                return;
            }

            if (Vector3.Distance(unit.transform.position, node.transform.position) > unit.CurrentJob.maxCarryingCapacity)
            {
                moveState = PushMoveState(unit, node.transform.position, unit.CurrentJob.gatheringDistance);
                moveState.OnStateFinish += MoveStateFinishedHandler;
                return;
            }

            if (gatheringTimer >= unit.CurrentJob.gatheringCooldown)
            {
                var desiredValue = Mathf.Min(unit.CurrentJob.gatheringAmount, unit.CurrentJob.maxCarryingCapacity - unit.carriedResource.value);
                var gatheredAmount = node.GatherResource(desiredValue);
                unit.AddResource(node.Type, gatheredAmount);
                unit.carriedResource.type = node.Type;
                gatheringTimer = 0;
            }

            gatheringTimer += Time.deltaTime;
        }

        public override void StopState(bool stopParent = true)
        {
            if (node != null)
                node.OnResourceDepleted -= ResourceDepletedHandler;
            base.StopState(stopParent);
        }

        private void ResourceDepletedHandler(ResourceNode depletedNode)
        {
            nodeHasDepleted = true;
        }

        private void MoveToBaseStateFinishedHandler(State state)
        {
            unit.LoadResourcesToBase();
            MoveStateFinishedHandler(state);
            isReturningResources = false;
        }

        private void MoveStateFinishedHandler(State state)
        {
            moveState = null;
        }
    }
}