using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace dmdspirit
{
    public class TeamUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text[] unitNames;
        [SerializeField] private TMP_Text stoneValue;
        [SerializeField] private TMP_Text woodValue;
        [SerializeField] private Image background;

        private Team team;

        // IMPROVE: Handle unit list, not just adding a unit.
        private int unitNumber = 0;

        public void Initialize(Team team, int stone, int wood)
        {
            stoneValue.SetText(stone.ToString());
            woodValue.SetText(wood.ToString());
            this.team = team;
            team.OnResourceChange += ResourceChangeHandler;
            team.OnUnitAdded += UnitAddedHandler;
            var bgColor = team.teamColor;
            bgColor.a = .8f;
            background.color = bgColor;
            foreach (var unitName in unitNames)
                unitName.gameObject.SetActive(false);
        }

        private void UnitAddedHandler(Unit unit)
        {
            unitNames[unitNumber].gameObject.SetActive(true);
            unitNames[unitNumber].SetText(unit.name);
            unitNumber++;
        }

        private void ResourceChangeHandler()
        {
            woodValue.text = team.storedResources.wood.ToString();
            stoneValue.text = team.storedResources.stone.ToString();
        }

        public void UpdateUnitName(int nameId, string newName) => unitNames[nameId].text = newName;
    }
}