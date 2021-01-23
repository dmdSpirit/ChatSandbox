using System;
using System.Collections.Generic;
using UnityEngine;

namespace dmdspirit
{
    // IMPROVE: Do I really need this?
    public class BuildingController : MonoSingleton<BuildingController>
    {
        [SerializeField] private ConstructionSite constructionSitePrefab;
        [SerializeField] private BaseBuilding basePrefab;
        [SerializeField] private TowerBuilding towerPrefab;
        [SerializeField] private Barracks barracksPrefab;

        public ResourceCollection GetBuildingCost(BuildingType buildingType) => GetBuildingPrefab(buildingType).cost;

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

        public ConstructionSite CreateConstructionSite(Team team, BuildingType buildingType, MapTile mapTile, TileDirection direction)
        {
            // TODO: Also add building facing direction.
            var constructionSite = Instantiate(constructionSitePrefab, mapTile.transform.position, Quaternion.identity, mapTile.transform);
            constructionSite.Initialize(team, GetBuildingPrefab(buildingType), mapTile, direction);
            team.AddBuildingSite(constructionSite);
            return constructionSite;
        }
    }
}