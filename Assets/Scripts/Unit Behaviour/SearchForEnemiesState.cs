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
            // IMPROVE: Better check unit lists that OverlapSphere every update for every unit?
            var enemyUnits = GameController.Instance.GetEnemyTeam(unit.UnitTeam).Units;
            Unit closestEnemy = null;
            var distance = AggroRadius;
            foreach (var enemy in enemyUnits)
            {
                // HACK: check to cover some problems in unitList update for GameController.
                if (enemy == null) continue;
                var newDistance = Vector3.Distance(unit.transform.position, enemy.transform.position);
                if (newDistance <= distance)
                {
                    distance = newDistance;
                    closestEnemy = enemy;
                }
            }

            if (closestEnemy == null) return;
            unit.AttackUnit(closestEnemy);
            StopState();
        }
    }
}