using UnityEngine;

namespace dmdspirit
{
    public class AttackState : State
    {
        private Unit unit;
        private ObjectRadius targetRadius;
        private HitPoints target;
        private float attackTimer = 0;
        private const float shortChase = 2f;

        private float AttackRange => unit.CurrentJob.attackRange;
        private float AttackCooldown => unit.CurrentJob.attackCooldown;

        // HACK: This fixes unit resetting attack cd when its target is pushed away.

        public AttackState(Unit unit, HitPoints target)
        {
            // BUG: Units are constantly starting attackState while in combat. 
            this.unit = unit;
            Debug.Log($"{unit.name} started attacking {target.name}.");
            this.target = target;
            targetRadius = target.GetComponent<ObjectRadius>();
        }

        public override void Update()
        {
            if (target == null || target.IsAlive == false)
            {
                StopState();
                return;
            }

            var targetPosition = targetRadius == null ? target.transform.position : targetRadius.GetClosestPoint(target.transform.position);
            var distance = Vector3.Distance(unit.transform.position, targetPosition);
            if (distance > AttackRange)
            {
                // FIXME: Should know if the target is able to move.
                if (target.CanMove)
                {
                    PushChaseState();
                    // HACK: To stop instant attacks when target is pushed away little by little.
                    if (distance > shortChase)
                        attackTimer = 0;
                    return;
                }

                PushMoveState(unit, targetPosition, AttackRange, targetRadius);
                return;
            }

            if (attackTimer <= 0)
                Attack();
            else
                attackTimer -= Time.deltaTime;
        }

        private void Attack()
        {
            unit.DealDamage(target);
            unit.ShootProjectile(target);
            attackTimer = AttackCooldown;
        }

        private void PushChaseState()
        {
            var chaseState = new ChaseState(unit, target, AttackRange);
            PushState(chaseState);
        }
    }
}