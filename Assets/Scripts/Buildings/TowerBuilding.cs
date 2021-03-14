using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace dmdspirit
{
    public class TowerBuilding : Building
    {
        [SerializeField] private float attackDamage;
        [SerializeField] private float attackRadius;
        [SerializeField] private float attackCooldown;
        [SerializeField] private float projectileSpeed;
        [SerializeField] private Transform radiusCircle;
        [SerializeField] private Transform shootingOrigin;
        [SerializeField] private Projectile projectilePrefab;

        // TODO: Create projectile to show attack.

        private Coroutine currentAttack = null;
        private float timeSinceLastAttack = 0f;

        protected override void Awake()
        {
            base.Awake();
            type = BuildingType.Tower;
            radiusCircle.localScale = Vector3.one * (attackRadius / 5f);
        }

        private void Update()
        {
            timeSinceLastAttack += Time.deltaTime;
            if (isFinished == false || HitPoints.IsAlive == false || currentAttack != null) return;
            var potentialTarget = GameController.Instance.GetEnemyTeam(Team).GetAllPotentialTargets().Where(target => target.IsInRange(transform.position, attackRadius)).ToList().OrderBy(target => Vector3.Distance(transform.position, target.transform.position)).FirstOrDefault();
            if (potentialTarget == null) return;
            Debug.Log($"{name} has found potential target: {potentialTarget.name}");
            currentAttack = StartCoroutine(Attack(potentialTarget));
        }

        private IEnumerator Attack(HitPoints target)
        {
            while (target != null && target.IsAlive && target.IsInRange(transform.position, attackRadius))
                
                
            {
                if (timeSinceLastAttack < attackCooldown)
                    yield return new WaitForSeconds(attackCooldown - timeSinceLastAttack);
                ShootProjectile(target);
                timeSinceLastAttack = 0;
                yield return new WaitForSeconds(attackCooldown);
            }

            currentAttack = null;
        }

        private void ShootProjectile(HitPoints target)
        {
            var projectile = Instantiate(projectilePrefab, shootingOrigin.position, Quaternion.identity);
            projectile.Initialize(target, attackDamage, projectileSpeed, Team.teamColor);
        }
    }
}