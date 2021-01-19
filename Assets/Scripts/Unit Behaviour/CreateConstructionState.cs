using UnityEngine;

namespace dmdspirit
{
    public class CreateConstructionState : State
    {
        private Unit unit;
        private BuildingType buildingType;
        private MapPosition tilePosition;
        private Vector3 constructionPosition;
        private TileDirection direction;

        private float BuildingDistance => unit.CurrentJob.buildingDistance;

        public CreateConstructionState(Unit unit, MapPosition mapPosition, BuildingType buildingType, TileDirection direction)
        {
            if (GameController.Instance.CanBeBuild.Contains(buildingType) == false)
            {
                Debug.LogError($"{unit.name} is trying to build {buildingType.ToString()}, but this type of building is not allowed.");
                // FIXME: Unit Behaviour does not handle correctly when state is stopped in constructor.
                StopState();
                return;
            }

            this.unit = unit;
            tilePosition = mapPosition;
            this.buildingType = buildingType;
            this.direction = direction;
            constructionPosition = Map.Instance.GetTile(tilePosition).transform.position;
        }

        public override void Update()
        {
            if (Vector3.Distance(unit.transform.position, constructionPosition) > BuildingDistance)
            {
                PushMoveState(unit, constructionPosition, BuildingDistance);
                return;
            }

            var constructionSite = BuildingController.Instance.CreateConstructionSite(unit.UnitTeam, buildingType, Map.Instance.GetTile(tilePosition), direction);
            StopState();
            
            // FIXME: Not sure this will work correctly.
            PushState(new BringResourcesState(unit, constructionSite));
        }
    }
}