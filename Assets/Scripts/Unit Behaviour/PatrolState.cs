using UnityEngine;

namespace dmdspirit
{
    public class PatrolState : State
    {
        private Unit unit;
        private Vector3 firstPosition;
        private Vector3 secondPosition;
        private float stopDistance = 1.5f;
        private Vector3 currentTarget;

        public PatrolState(Unit unit, MapPosition firstTile, MapPosition? secondTile)
        {
            this.unit = unit;
            firstPosition = Map.Instance.GetTile(firstTile).transform.position;
            secondPosition = secondTile.HasValue ? Map.Instance.GetTile(secondTile.Value).transform.position : Map.Instance.GetMapPosition(unit.transform.position);
            currentTarget = firstPosition;
        }

        public override void Update()
        {
            if (Vector3.Distance(unit.transform.position, currentTarget) <= stopDistance)
                currentTarget = currentTarget == firstPosition ? secondPosition : firstPosition;
            PushMoveState(unit, currentTarget, stopDistance);
        }
    }
}