using UnityEngine;

namespace dmdspirit
{
    public class ChangeJobState : State
    {
        private Unit unit;
        private UnitJob job;
        private Building targetBuilding;
        private float changeDistance = .3f;

        public ChangeJobState(Unit unit, UnitJob job)
        {
            this.unit = unit;
            this.job = job;
            var buildings = unit.UnitTeam.GetBuildingsOfType(job.buildingNeeded);
            var distance = float.MaxValue;
            foreach (var building in buildings)
            {
                // FIXME: Direct distance w/o pathfinding.
                var d = Vector3.Distance(unit.transform.position, building.entrance.position);
                if (d >= distance) continue;
                distance = d;
                targetBuilding = building;
            }
        }

        public override void Update()
        {
            if (Vector3.Distance(unit.transform.position, targetBuilding.entrance.position) >= changeDistance)
                PushMoveState(unit, targetBuilding.entrance.position, changeDistance);
            else
            {
                // IMPROVE: May be not the best way to do this. Don't like public ChangeJob.
                unit.ChangeJob(job.jobType);
                StopState();
            }
        }
    }
}