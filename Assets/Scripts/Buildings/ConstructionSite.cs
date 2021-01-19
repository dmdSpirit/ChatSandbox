using System;
using System.Collections.Generic;
using System.Linq;
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
            OnDeliveryInterrupted?.Invoke(this);
            unit.OnDeath -= UnitDeathHandler;
        }
    }
    
    public class ConstructionSite : MonoBehaviour
    {
        public enum ConstructionState
        {
            GatheringResources,
            Building,
            Finished
        }

        public event Action<ConstructionSite> OnBuildingSiteFinished;

        [SerializeField] private ProgressBar progressBar;

        public Building Building { get; private set; }
        public ConstructionState State { get; private set; }

        private Team team;
        private float buildingPoints;
        private List<Delivery> deliveries;

        private ResourceCollection resourcesLeftToCollect;

        private void Update()
        {
            if (State == ConstructionState.GatheringResources && resourcesLeftToCollect.IsEmpty())
            {
                State = ConstructionState.Building;
            }

            if (State == ConstructionState.Building && buildingPoints >= Building.buildingPointsCost)
            {
                Building.isFinished = true;
                OnBuildingSiteFinished?.Invoke(this);
                State = ConstructionState.Finished;
            }
        }

        public void Initialize(Team team, Building buildingPrefab, TileDirection direction)
        {
            this.team = team;
            Building = Instantiate(buildingPrefab, Vector3.zero, Quaternion.Euler(0, (int) direction * 90, 0), transform);
            Building.Initialize(team);
            buildingPoints = 0;
            progressBar.SetProgress(buildingPoints / Building.buildingPointsCost);
            resourcesLeftToCollect = new ResourceCollection(Building.cost);
            State = ConstructionState.GatheringResources;
            deliveries = new List<Delivery>();
        }

        public void AddBuildingPoints(float buildingPoints)
        {
            if (State != ConstructionState.Building) return;
            this.buildingPoints += buildingPoints;
            // TODO: Animate building process.
            progressBar.SetProgress(this.buildingPoints / Building.buildingPointsCost);
        }

        public void DestroySite()
        {
            if (State == ConstructionState.Finished)
                Building.transform.SetParent(transform.parent);
            Destroy(gameObject);
        }

        public void FinishDelivery(Delivery delivery)
        {
            AddResources(delivery.resource);
            delivery.unit.carriedResource.Clear();
            deliveries.Remove(delivery);
            delivery.OnDeliveryInterrupted -= DeliveryInterruptedHandler;
        }

        public int TryAddResource(Resource resource)
        {
            if (resource.value == 0) return 0;
            var resourcesNeeded = GetNeededResourcesWithDeliveries();
            if (resourcesNeeded.ContainsKey(resource.type) == false) return 0;
            // IMPROVE: Unit should be able override delivery if it has resources needed right now.

            var resourceLeft = resource.value - Mathf.Min(resource.value, resourcesNeeded[resource.type]);
            resource.value -= resourceLeft;
            AddResources(resource);
            return resourceLeft;
        }

        private void AddResources(Resource resource)
        {
            if (resourcesLeftToCollect.GetResourceValue(resource.type) < resource.value)
            {
                Debug.LogError($"Something went wrong with delivery system. Construction site {name} got too much resources delivered ({resource.type.ToString()},{resource.value})");
                return;
            }

            resource.value *= -1;
            resourcesLeftToCollect.AddResources(resource);
        }

        private void DeliveryInterruptedHandler(Delivery delivery)
        {
            deliveries.Remove(delivery);
            delivery.OnDeliveryInterrupted -= DeliveryInterruptedHandler;
        }

        public bool TryRequestDelivery(Unit unit, out Delivery delivery)
        {
            delivery = null;
            if (State != ConstructionState.GatheringResources || resourcesLeftToCollect.IsEmpty()) return false;
            var resourcesNeeded = GetNeededResourcesWithDeliveries();
            var teamResources = unit.UnitTeam.storedResources;
            var deliveryResourceType = resourcesNeeded.Keys.FirstOrDefault(resourceType => teamResources.HasEnough(resourceType, Mathf.Min(resourcesNeeded[resourceType], unit.CurrentJob.maxCarryingCapacity)));
            if (deliveryResourceType == ResourceType.None) deliveryResourceType = resourcesNeeded.First().Key;
            var resource = new Resource {type = deliveryResourceType, value = Mathf.Min(unit.CurrentJob.maxCarryingCapacity, resourcesNeeded[deliveryResourceType])};
            delivery = new Delivery(unit, resource);
            deliveries.Add(delivery);
            delivery.OnDeliveryInterrupted += DeliveryInterruptedHandler;
            return true;
        }

        private Dictionary<ResourceType, int> GetNeededResourcesWithDeliveries()
        {
            var resourcesNeeded = resourcesLeftToCollect.GetResourcesLeft();
            foreach (var delivery in deliveries)
            {
                resourcesNeeded[delivery.resource.type] -= delivery.resource.value;
                if (resourcesNeeded[delivery.resource.type] == 0)
                    resourcesNeeded.Remove(delivery.resource.type);
            }

            return resourcesNeeded;
        }
    }
}