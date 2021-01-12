using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace dmdspirit
{
    public class UnitInfoUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text unitName;
        [SerializeField] private ProgressBar unitHPBar;
        [SerializeField] private Image background;
        [SerializeField] private CarriedResourceUI carriedResource;
        [SerializeField] private Image jobIcon;

        private Unit unit;

        public void Initialize(Unit unit)
        {
            // TODO: Add events to update unit info.
            this.unit = unit;
            unit.OnUpdateHP += UpdateHpHandler;
            unit.OnOwnerChanged += OwnerChangedHandler;
            unit.OnJobChanged += JobChangedHandler;
            unit.OnDeath += DeathHandler;
            OwnerChangedHandler();
            JobChangedHandler();
            UpdateHpHandler();
            var color = unit.UnitTeam.teamColor;
            color.a = .8f;
            background.color = color;
            carriedResource.Initilaize(unit);
        }

        private void DeathHandler(Unit obj) => unitHPBar.SetProgress(0);

        private void OwnerChangedHandler() => unitName.text = unit.name;
        private void UpdateHpHandler() => unitHPBar.SetProgress(unit.HP / unit.CurrentJob.maxHP);

        private void JobChangedHandler() => jobIcon.sprite = unit.CurrentJob.icon;
    }
}