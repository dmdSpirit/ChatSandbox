using System;
using UnityEngine;

namespace dmdspirit
{
    [Serializable]
    // FIXME: Find a better name.
    // TODO: Rewrite for n resource types.
    public struct ResourceCost
    {
        public int stone;
        public int wood;

        public void AddResources(ResourceCost rc)
        {
            stone += rc.stone;
            wood += rc.wood;
        }

        public bool HasEnough(ResourceCost rc) => stone >= rc.stone && wood >= rc.wood;

        public void SpendResource(ResourceCost rc)
        {
            if (HasEnough(rc) == false)
            {
                Debug.LogError($"Could not spend resource, not enough.");
                return;
            }

            stone -= rc.stone;
            wood -= rc.wood;
        }

        public void AddResources(ResourceValue value)
        {
            switch (value.type)
            {
                case ResourceType.Wood:
                    wood += (int) value.value;
                    return;
                case ResourceType.Stone:
                    stone += (int) value.value;
                    return;
            }
        }

        public override string ToString() => $"stone: {stone}, wood: {wood}";
    }

    public class BuildingController : MonoSingleton<BuildingController>
    {
        [SerializeField] private BuildingSite buildingSitePrefab;
        [SerializeField] private BaseBuilding basePrefab;
        [SerializeField] private TowerBuilding towerPrefab;
        [SerializeField] private Barracks barracksPrefab;

        public ResourceCost GetBuildingCost(BuildingType buildingType) => GetBuildingPrefab(buildingType).cost;

        public Building GetBuildingPrefab(BuildingType buildingType)
        {
            switch (buildingType)
            {
                case BuildingType.None:
                    return null;
                case BuildingType.Base:
                    return basePrefab;
                case BuildingType.Tower:
                    return towerPrefab;
                case BuildingType.Barracks:
                    return barracksPrefab;
            }

            return null;
        }

        public BuildingSite CreateBuildingSite(Team team, BuildingType buildingType, MapTile mapTile, TileDirection direction)
        {
            // TODO: Also add building facing direction.
            var buildingSite = Instantiate(buildingSitePrefab, mapTile.transform.position, Quaternion.Euler(0, 90 * (int) direction, 0), mapTile.transform);
            buildingSite.Initialize(team, GetBuildingPrefab(buildingType));
            return buildingSite;
        }
    }
}