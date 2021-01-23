using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace dmdspirit
{
    public class ConstructionSite : MonoBehaviour, ICanBeHit
    {
        public enum ConstructionState
        {
            GatheringResources,
            Building,
            Finished
        }

        public event Action<ConstructionSite> OnConstructionSiteFinished;

        [SerializeField] private ProgressBar progressBar;

        public Building Building { get; private set; }
        public ConstructionState State { get; private set; }
        public Team Team { get; private set; }
        public MapTile Tile { get; private set; }

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
                Building.transform.SetParent(Tile.transform);
                OnConstructionSiteFinished?.Invoke(this);
                State = ConstructionState.Finished;
            }
        }

        public void Initialize(Team team, Building buildingPrefab, MapTile tile, TileDirection direction)
        {
            Tile = tile;
            Team = team;
            Building = Instantiate(buildingPrefab, transform.position, Quaternion.Euler(0, (int) direction * 90, 0), transform);
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
            Debug.Log($"{name} building points added {buildingPoints}.");
            progressBar.SetProgress(this.buildingPoints / Building.buildingPointsCost);
        }

        public void DestroySite()
        {
            Destroy(gameObject);
        }

        public void FinishDelivery(Delivery delivery)
        {
            AddResources(delivery.resource);
            deliveries.Remove(delivery);
            delivery.OnDeliveryInterrupted -= DeliveryInterruptedHandler;
        }

        public int TryAddResource(Resource resource)
        {
            if (resource.value == 0 || resource.type == ResourceType.None) return resource.value;
            var resourcesNeeded = GetNeededResourcesWithDeliveries();
            if (resourcesNeeded.ContainsKey(resource.type) == false) return resource.value;
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
            if (deliveryResourceType == ResourceType.None)
            {
                if (resourcesNeeded.Count == 0)
                    return false;
                deliveryResourceType = resourcesNeeded.FirstOrDefault().Key;
            }

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

        public void GetHit(float damage)
        {
        }

        public bool IsAlive() => true;
        public bool IsInRage(Vector3 attacker, float range) => Vector3.Distance(attacker, transform.position) <= range;
    }
}