using UnityEngine;
using UnityEngine.AI;

namespace dmdspirit
{
    // TODO: Move enum somewhere else (BuildingController?)
    public enum BuildingType
    {
        None,
        Base,
        Tower
    }

    public class BuildState : State
    {
        private NavMeshAgent agent;
        private MapTile targetTile;
        private BuildingType buildingType;
        private float buildingDistance;
        private Unit unit;
        private BuildingSite buildingSite;
        private float buildingSpeed;

        public BuildState(NavMeshAgent agent, BuildingType buildingType, MapTile targetTile, float buildingDistance, float buildingSpeed)
        {
            if (GameController.Instance.canBeBuild.Contains(buildingType) == false)
            {
                Debug.LogError($"{agent.gameObject.name} is trying to build {buildingType.ToString()}, but this type of building is not allowed.");
                StopState();
                return;
            }

            unit = agent.GetComponent<Unit>();
            this.targetTile = targetTile;
            this.agent = agent;
            this.buildingType = buildingType;
            this.buildingDistance = buildingDistance;
            this.buildingSpeed = buildingSpeed;
        }

        public override void Update()
        {
            if (Vector3.Distance(agent.transform.position, targetTile.transform.position) > buildingDistance)
            {
                PushMoveState(targetTile.transform.position);
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
                buildingSite = BuildingController.Instance.CreateBuildingSite(unit.UnitTeam, buildingType, targetTile);
                buildingSite.OnBuildingComplete += BuildingCompleteHandler;
                unit.UnitTeam.SpendResources(cost);
                return;
            }

            buildingSite.AddBuildingPoints(buildingSpeed * Time.deltaTime);
        }

        private void PushMoveState(Vector3 moveDestination)
        {
            var moveToBaseState = new MoveState(moveDestination, agent, buildingDistance);
            PushState(moveToBaseState);
        }

        private void BuildingCompleteHandler()
        {
            Debug.Log($"{unit.name} has completed building {buildingType.ToString()}.");
            unit.UnitTeam.AddBuilding(buildingSite);
            StopState();
        }
    }
}