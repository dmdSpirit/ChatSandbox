using UnityEngine;
using UnityEngine.AI;

namespace dmdspirit
{
    // TODO: Move enum somewhere else (BuildingController?)
    public enum BuildingType
    {
        None,
        Base,
        Tower,
        Barracks
    }

    public class BuildState : State
    {
        private Unit unit;
        private MapTile targetTile;
        private BuildingType buildingType;
        private TileDirection direction;
        private BuildingSite buildingSite;

        private float BuildingDistance => unit.CurrentJob.buildingDistance;
        private float BuildingSpeed => unit.CurrentJob.buildingSpeed;

        public BuildState(Unit unit, MapTile targetTile, BuildingType buildingType, TileDirection direction)
        {
            this.unit = unit;
            if (GameController.Instance.CanBeBuild.Contains(buildingType) == false)
            {
                Debug.LogError($"{unit.name} is trying to build {buildingType.ToString()}, but this type of building is not allowed.");
                // FIXME: Unit Behaviour does not handle correctly when state is stopped in constructor.
                StopState();
                return;
            }

            this.targetTile = targetTile;
            this.buildingType = buildingType;
            this.direction = direction;
        }

        public override void Update()
        {
            if (Vector3.Distance(unit.transform.position, targetTile.transform.position) > BuildingDistance)
            {
                PushMoveState(unit, targetTile.transform.position, BuildingDistance);
                return;
            }

            if (buildingSite == null)
            {
                if (targetTile.isEmpty == false)
                {
                    Debug.Log($"Something blocked tile that {unit.name} was trying to build in. {targetTile.Position.ToString()}");
                    StopState();
                    return;
                }

                var cost = BuildingController.Instance.GetBuildingCost(buildingType);
                if (unit.UnitTeam.storedResources.HasEnough(cost) == false)
                    return;

                Debug.Log($"{unit.name} has started building {buildingType.ToString()}.");
                buildingSite = BuildingController.Instance.CreateBuildingSite(unit.UnitTeam, buildingType, targetTile, direction);
                buildingSite.OnBuildingComplete += BuildingCompleteHandler;
                unit.UnitTeam.SpendResources(cost);
                return;
            }

            buildingSite.AddBuildingPoints(BuildingSpeed * Time.deltaTime);
        }

        private void BuildingCompleteHandler()
        {
            Debug.Log($"{unit.name} has completed building {buildingType.ToString()}.");
            unit.UnitTeam.AddBuilding(buildingSite);
            StopState();
        }
    }
}