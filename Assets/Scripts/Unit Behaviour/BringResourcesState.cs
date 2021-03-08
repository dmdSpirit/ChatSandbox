using UnityEngine;

namespace dmdspirit
{
    public class BringResourcesState : State
    {
        private Unit unit;
        private ConstructionSite constructionSite;
        private Resource resourceToDeliver;
        private bool isCarryingResources;
        private Delivery delivery;

        public BringResourcesState(Unit unit, ConstructionSite constructionSite)
        {
            this.unit = unit;
            this.constructionSite = constructionSite;
        }

        public override void Update()
        {
            if (delivery == null)
            {
                // Unit does not have delivery order, but is near construction site and is carrying some needed resources.
                if (unit.carriedResource.type != ResourceType.None && unit.carriedResource.value != 0 && Vector3.Distance(unit.transform.position, constructionSite.transform.position) <= unit.CurrentJob.buildingDistance)
                {
                    var resourceLeft = constructionSite.TryAddResource(unit.carriedResource);
                }

                if (constructionSite.TryRequestDelivery(unit, out delivery) == false)
                {
                    // TODO: Check if site has enough resources, start building it.
                    StopState();
                    return;
                }
            }
            else
            {
                // Unit has delivery order and already if carrying resources needed.
                if (unit.carriedResource.type == delivery.resource.type && unit.carriedResource.value == delivery.resource.value)
                {
                    if (Vector3.Distance(unit.transform.position, constructionSite.transform.position) > unit.CurrentJob.buildingDistance)
                    {
                        PushMoveState(unit, constructionSite.transform.position, unit.CurrentJob.buildingDistance);
                        return;
                    }

                    constructionSite.FinishDelivery(delivery);
                    delivery = null;
                }
                // Unit has delivery order but does not has resources.
                else
                {
                    if (Vector3.Distance(unit.transform.position, unit.UnitTeam.baseBuilding.entrance.position) > unit.CurrentJob.buildingDistance)
                    {
                        PushMoveState(unit, unit.UnitTeam.baseBuilding.entrance.position, unit.CurrentJob.buildingDistance);
                        return;
                    }

                    var resourceValueNeeded = delivery.resource.value;
                    if (unit.carriedResource.type != ResourceType.None && unit.carriedResource.value != 0)
                    {
                        if (unit.carriedResource.type != delivery.resource.type)
                        {
                            unit.UnitTeam.AddResource(unit.carriedResource);
                            unit.carriedResource.Clear();
                        }
                        else
                            resourceValueNeeded -= unit.carriedResource.value;
                    }

                    var resourceFromTeam = unit.UnitTeam.storedResources.GetAnyResourceUpToValue(delivery.resource.type, resourceValueNeeded);
                    unit.carriedResource.value += resourceFromTeam;
                }
            }
        }
    }
}