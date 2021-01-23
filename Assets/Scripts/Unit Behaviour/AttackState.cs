﻿using UnityEngine;
using UnityEngine.AI;

namespace dmdspirit
{
    public class AttackState : State
    {
        private Unit unit;
        private ICanBeHit target;
        private float attackTimer = 0;
        private const float shortChase = 2f;

        private float AttackRange => unit.CurrentJob.attackRange;
        private float AttackCooldown => unit.CurrentJob.attackCooldown;

        // HACK: This fixes unit resetting attack cd when its target is pushed away.

        public AttackState(Unit unit, ICanBeHit target)
        {
            // BUG: Units are constantly starting attackState while in combat. 
            this.unit = unit;
            Debug.Log($"{unit.name} started attacking {((MonoBehaviour)target).name}.");
            this.target = target;
        }

        public override void Update()
        {
            if (target == null || target.IsAlive() == false)
            {
                StopState();
                return;
            }


            var distance = Vector3.Distance(unit.transform.position, ((MonoBehaviour) target).transform.position);
            if (distance > AttackRange)
            {
                if (target is Unit)
                {
                    PushChaseState();
                    // HACK: To stop instant attacks when target is pushed away little by little.
                    if (distance > shortChase)
                        attackTimer = 0;
                    return;
                }

                PushMoveState(unit, ((MonoBehaviour) target).transform.position, AttackRange);
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
            attackTimer = AttackCooldown;
        }

        private void PushChaseState()
        {
            var chaseState = new ChaseState(unit, (Unit)target, AttackRange);
            PushState(chaseState);
        }
    }
}