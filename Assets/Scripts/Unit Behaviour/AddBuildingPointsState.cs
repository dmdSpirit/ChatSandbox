using UnityEngine;

namespace dmdspirit
{
    public class AddBuildingPointsState : State
    {
        private Unit unit;
        private ConstructionSite constructionSite;

        private float elapsedSince;

        public AddBuildingPointsState(Unit unit, ConstructionSite constructionSite)
        {
            this.unit = unit;
            this.constructionSite = constructionSite;
        }

        public override void Update()
        {
            if (constructionSite.State == ConstructionSite.ConstructionState.Finished)
            {
                StopState();
                return;
            }

            if (Vector3.Distance(unit.transform.position, constructionSite.transform.position) > unit.CurrentJob.buildingDistance)
            {
                PushMoveState(unit, constructionSite.transform.position, unit.CurrentJob.buildingDistance);
                return;
            }

            elapsedSince += Time.deltaTime;
            if (elapsedSince < unit.CurrentJob.buildingPointsCooldown)
                return;
            elapsedSince = 0;
            constructionSite.AddBuildingPoints(unit.CurrentJob.buildingPoints);
        }
    }
}