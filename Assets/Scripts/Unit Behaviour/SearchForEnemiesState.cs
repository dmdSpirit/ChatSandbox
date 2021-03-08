using UnityEngine;

namespace dmdspirit
{
    public class SearchForEnemiesState : State
    {
        private Unit unit;
        private float AggroRadius => unit.CurrentJob.aggroRadius;

        private static LayerMask unitLayer = LayerMask.NameToLayer("Unit");

        public SearchForEnemiesState(Unit unit)
        {
            this.unit = unit;
        }

        public override void Update()
        {
            var potentialTargets = GameController.Instance.GetEnemyTeam(unit.UnitTeam).GetAllPotentialTargets();
            HitPoints closestTarget = null;
            var distance = AggroRadius;
            foreach (var potentialTarget in potentialTargets)
            {
                var monoBeh = (MonoBehaviour) potentialTarget;
                var newDistance = Vector3.Distance(unit.transform.position, monoBeh.transform.position);
                if (newDistance > distance) continue;
                distance = newDistance;
                closestTarget = potentialTarget;
            }

            if (closestTarget == null) return;
            unit.AttackTarget(closestTarget);
            StopState();
        }
    }
}