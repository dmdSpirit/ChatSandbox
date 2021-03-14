using UnityEngine;

namespace dmdspirit
{
    public class AttackState : State
    {
        private Unit unit;
        private ObjectRadius targetRadius;
        private HitPoints target;
        private float attackTimer = 0;
        private ChaseState chaseState;

        public AttackState(Unit unit, HitPoints target)
        {
            this.unit = unit;
            Debug.Log($"{unit.name} started attacking {target.name}.");
            this.target = target;
            targetRadius = target.GetComponent<ObjectRadius>();
        }

        public override void Update()
        {
            if (target == null || target.IsAlive == false)
            {
                chaseState?.StopState(false);
                chaseState = null;
                StopState();
                return;
            }

            if (chaseState != null) return;

            var targetPosition = targetRadius == null ? target.transform.position : targetRadius.GetClosestPoint(target.transform.position);
            var distance = Vector3.Distance(unit.transform.position, targetPosition);
            if (distance > unit.CurrentJob.attackRange)
            {
                PushChaseState();
                return;
            }

            if (attackTimer >= unit.CurrentJob.attackCooldown)
            {
                unit.ShootProjectile(target);
                attackTimer = 0;
            }
            attackTimer += Time.deltaTime;
        }

        private void PushChaseState()
        {
            chaseState = new ChaseState(unit, target, unit.CurrentJob.attackRange);
            chaseState.OnStateFinish += ChaseStateFinishedHandler;
            PushState(chaseState);
        }

        private void ChaseStateFinishedHandler(State state)
        {
            chaseState = null;
        }
    }
}