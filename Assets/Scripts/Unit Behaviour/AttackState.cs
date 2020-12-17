using UnityEngine;
using UnityEngine.AI;

namespace dmdspirit
{
    public class AttackState : State
    {
        private NavMeshAgent agent;
        private Unit target;
        private Unit unit;
        private float attackDistance;
        private float attackCooldown;
        private float attackTimer;

        private float stortChase = 4f;

        // IMPROVE: Get unit instead of agent. Base states do not need NavMeshAgent.
        public AttackState(Unit unit, Unit target, float attackDistance, float attackCooldown)
        {
            this.unit = unit;
            agent = unit.GetComponent<NavMeshAgent>();
            Debug.Log($"{unit.name} started attacking {target.name}.");
            this.target = target;
            this.attackDistance = attackDistance;
            this.attackCooldown = attackCooldown;
        }


        public override void Update()
        {
            if (target == null || target.IsAlive == false)
            {
                StopState();
                return;
            }

            var distance = Vector3.Distance(agent.transform.position, target.transform.position);
            if (distance > attackDistance)
            {
                PushChaseState();
                // HACK: To stop instant attacks when target is pushed away little by little.
                if (distance > stortChase)
                    attackTimer = 0;
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
            attackTimer = attackCooldown;
        }

        private void PushChaseState()
        {
            var chaseState = new ChaseState(unit, target, attackDistance);
            PushState(chaseState);
        }
    }
}