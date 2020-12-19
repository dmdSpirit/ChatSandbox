using TMPro;
using UnityEngine;

namespace dmdspirit
{
    public class UnitInfoUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text unitName;
        [SerializeField] private ProgressBar unitHPBar;

        private Unit unit;

        public void Initialize(Unit unit)
        {
            // TODO: Add events to update unit info.
            this.unit = unit;
            unit.OnUpdateHP += UpdateHpHandler;
            unit.OnOwnerChanged += OwnerChangedHandler;
            OwnerChangedHandler();
            UpdateHpHandler();
        }

        private void OwnerChangedHandler() => unitName.text = unit.name;
        private void UpdateHpHandler() => unitHPBar.SetProgress(unit.HP / unit.CurrentJob.maxHP);
    }
}