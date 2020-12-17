using System;
using UnityEngine;

namespace dmdspirit
{
    public class TowerBuilding : Building
    {
        [SerializeField] private float attackDamage;
        [SerializeField] private float attackRadius;
        [SerializeField] private float attackCooldown;
        [SerializeField] private Transform radiusCircle;

        // TODO: Create projectile to show attack.

        private float attackTimer;

        private void Awake()
        {
            type = BuildingType.Tower;
            attackTimer = 0;
            radiusCircle.localScale = Vector3.one * (attackRadius / 5f);
        }

        private void Update()
        {
            if (isFinished == false) return;
            // IMPROVE: Move to coroutine.
            if (attackTimer > attackCooldown)
            {
                var enemyUnits = GameController.Instance.GetEnemyTeam(team).Units.ToArray();
                // IMPROVE: Choose target based on some logic (nearest?)
                foreach (var enemyUnit in enemyUnits)
                {
                    if (enemyUnit.IsAlive && Vector3.Distance(transform.position, enemyUnit.transform.position) <= attackRadius)
                    {
                        Debug.Log($"{name} attacks {enemyUnit.name}.");
                        enemyUnit.TakeDamage(attackDamage);
                        attackTimer = 0;
                    }
                }
            }
            else
                attackTimer += Time.deltaTime;
        }
    }
}