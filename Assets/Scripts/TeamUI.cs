using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace dmdspirit
{
    public class TeamUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI[] unitNames;
        [SerializeField] private TextMeshProUGUI stoneValue;
        [SerializeField] private TextMeshProUGUI woodValue;
        [SerializeField] private Image background;

        private Team team;

        // IMPROVE: Handle unit list, not just adding a unit.
        private int unitNumber = 0;

        public void Initialize(Team team, int stone, int wood)
        {
            stoneValue.SetText(stone.ToString());
            woodValue.SetText(wood.ToString());
            team.OnResourceChange += ResourceChangeHandler;
            team.OnUnitAdded += UnitAddedHandler;
            var bgColor = team.teamColor;
            bgColor.a = .3f;
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

        private void ResourceChangeHandler(ResourceType type, int value)
        {
            switch (type)
            {
                case ResourceType.None:
                    break;
                case ResourceType.Tree:
                    woodValue.SetText(value.ToString());
                    break;
                case ResourceType.Stone:
                    stoneValue.SetText(value.ToString());
                    break;
            }
        }
    }
}