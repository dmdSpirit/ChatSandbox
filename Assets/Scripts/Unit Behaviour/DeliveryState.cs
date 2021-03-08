using System;
using UnityEngine;

namespace dmdspirit
{
    public class Delivery
    {
        public event Action<Delivery> OnDeliveryInterrupted;

        public Unit unit;
        public Resource resource;

        public Delivery(Unit unit, Resource resource)
        {
            this.unit = unit;
            this.resource = resource;
            unit.OnDeath += UnitDeathHandler;
        }

        private void UnitDeathHandler(Unit unit)
        {
            AbandonDelivery();
            unit.OnDeath -= UnitDeathHandler;
        }

        public void AbandonDelivery() => OnDeliveryInterrupted?.Invoke(this);
    }

    public class DeliveryState : State
    {
        private Unit unit;
        private ConstructionSite constructionSite;
        private Delivery delivery;

        // IMPROVE: Move TryGetDelivery to interface.
        public DeliveryState(Unit unit, ConstructionSite constructionSite)
        {
            this.unit = unit;
            this.constructionSite = constructionSite;
        }

        public override void Update()
        {
            // Check if unit is close to building site and has resources needed.
            if (delivery == null)
            {
                if (constructionSite.TryRequestDelivery(unit, out delivery) == false)
                    StopState();
                return;
            }

            if (unit.carriedResource == delivery.resource)
            {
                if (Vector3.Distance(unit.transform.position, constructionSite.transform.position) > unit.CurrentJob.buildingDistance)
                {
                    PushMoveState(unit, constructionSite.transform.position, unit.CurrentJob.buildingDistance);
                    return;
                }

                constructionSite.FinishDelivery(delivery);
                unit.DropResources();
                delivery = null;
                StopState(false);
                return;
            }

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

            var resourceFromTeam = unit.UnitTeam.GetAnyResourceUpToValue(delivery.resource.type, resourceValueNeeded);
            unit.AddResource(delivery.resource.type, resourceFromTeam);
        }

        public override void StopState(bool stopParent = true)
        {
            if (delivery != null)
                delivery.AbandonDelivery();
            base.StopState(stopParent);
        }
    }
}