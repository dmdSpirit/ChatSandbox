using UnityEngine;

namespace dmdspirit
{
    public class BuildState : State
    {
        private Unit unit;
        private MapTile targetTile;
        private BuildingType buildingType;
        private TileDirection direction;
        private ConstructionSite constructionSite;

        private float BuildingDistance => unit.CurrentJob.buildingDistance;
        private float BuildingPoints => unit.CurrentJob.buildingPoints;

        public BuildState(Unit unit, MapTile targetTile, BuildingType buildingType, TileDirection direction)
        {
            this.unit = unit;
            this.targetTile = targetTile;
            this.buildingType = buildingType;
            this.direction = direction;
        }

        public override void Update()
        {
            if (constructionSite == null)
            {
                // Check if there is an existing construction site.
                if (targetTile.ConstructionSite != null)
                {
                    if (targetTile.ConstructionSite.Team != unit.UnitTeam)
                    {
                        StopState();
                        return;
                    }

                    constructionSite = targetTile.ConstructionSite;
                }
                else
                {
                    if (targetTile.CheckCanBuild(unit.UnitTeam.teamTag, buildingType) == false || GameController.Instance.CanBeBuild.Contains(buildingType) == false)
                    {
                        StopState();
                        return;
                    }

                    if (Vector3.Distance(unit.transform.position, targetTile.transform.position) > unit.CurrentJob.buildingDistance)
                    {
                        PushMoveState(unit, targetTile.transform.position, unit.CurrentJob.buildingDistance);
                        return;
                    }

                    // Try to create construction site.
                    constructionSite = BuildingController.Instance.CreateConstructionSite(unit.UnitTeam, buildingType, targetTile, direction);
                    targetTile.AddConstructionSite(constructionSite);
                    var resourcesLeft = constructionSite.TryAddResource(unit.carriedResource);
                    unit.SpendCarriedResource(unit.carriedResource.value - resourcesLeft);
                }
            }

            switch (constructionSite.State)
            {
                case ConstructionSite.ConstructionState.GatheringResources:
                    PushState(new DeliveryState(unit, constructionSite));
                    return;
                case ConstructionSite.ConstructionState.Building:
                    PushState(new AddBuildingPointsState(unit, constructionSite));
                    return;
                case ConstructionSite.ConstructionState.Finished:
                    StopState();
                    return;
            }
        }
    }
}