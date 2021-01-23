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
        [SerializeField] private Transform radiusCircle;

        // TODO: Create projectile to show attack.

        private Coroutine currentAttack = null;
        private float timeSinceLastAttack = 0f;

        private void Awake()
        {
            type = BuildingType.Tower;
            radiusCircle.localScale = Vector3.one * (attackRadius / 5f);
        }

        private void Update()
        {
            timeSinceLastAttack += Time.deltaTime;
            if (isFinished == false || IsAlive() == false || currentAttack != null) return;
            var potentialTarget = GameController.Instance.GetEnemyTeam(Team).GetAllPotentialTargets().Where(target => target.IsInRage(transform.position, attackRadius)).ToList().OrderBy(target => Vector3.Distance(transform.position, ((MonoBehaviour) target).transform.position)).FirstOrDefault();
            if (potentialTarget == null) return;
            Debug.Log($"{name} has found potential target: {((MonoBehaviour) potentialTarget).name}");
            currentAttack = StartCoroutine(Attack(potentialTarget));
        }

        private IEnumerator Attack(ICanBeHit target)
        {
            while (target != null && target.IsAlive() && target.IsInRage(transform.position, attackRadius))
            {
                if (timeSinceLastAttack < attackCooldown)
                    yield return new WaitForSeconds(attackCooldown - timeSinceLastAttack);
                // TODO: Spawn projectile.
                target.GetHit(attackDamage);
                timeSinceLastAttack = 0;
                yield return new WaitForSeconds(attackCooldown);
            }

            currentAttack = null;
        }
    }
}