using System;
using UnityEngine;

namespace dmdspirit
{
    public class HitPoints : MonoBehaviour
    {
        public event Action OnDeath;
        public event Action OnHPUpdate;

        [SerializeField] private ProgressBar HPBar;

        public float HP { get; private set; }
        public float maxHP { get; private set; }

        public bool IsAlive => HP > 0;

        // HACK: ??
        public bool CanMove => unit != null;

        private Unit unit;

        private void Start()
        {
            unit = GetComponent<Unit>();
        }

        public void Initialize(float maxHP)
        {
            this.maxHP = maxHP;
            ResetHP();
            UpdateUI();
        }

        public void GetHit(float damage)
        {
            HP = Mathf.Max(0, HP - damage);
            Debug.Log($"{name} takes {damage} damage. HP: ({HP}/{maxHP})");
            if (HP <= 0)
                OnDeath?.Invoke();
            UpdateUI();
        }

        public void ChageMaxHP(float newMaxHP, bool setMaxHP = false)
        {
            var oldMaxHP = maxHP == 0 ? newMaxHP : maxHP;
            maxHP = newMaxHP;
            if (setMaxHP)
                ResetHP();
            else
                HP *= maxHP / oldMaxHP;
            UpdateUI();
        }

        public void ResetHP()
        {
            HP = maxHP;
            UpdateUI();
        }

        public bool IsInRange(Vector3 attackerPosition, float range) => Vector3.Distance(attackerPosition, transform.position) <= range;

        private void UpdateUI()
        {
            if (HP == maxHP)
                HPBar.gameObject.SetActive(false);
            else
            {
                HPBar.gameObject.SetActive(true);
                HPBar.SetProgress(HP / maxHP);
            }

            OnHPUpdate?.Invoke();
        }
    }
}