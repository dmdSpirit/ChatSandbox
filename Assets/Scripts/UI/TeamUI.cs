﻿using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace dmdspirit
{
    public class TeamUI : MonoBehaviour
    {
        [SerializeField] private UnitInfoUI unitInfoUIPrefab;
        [SerializeField] private Transform unitListParent;
        [SerializeField] private TMP_Text stoneValue;
        [SerializeField] private TMP_Text woodValue;
        [SerializeField] private Image background;

        private Team team;

        public void Initialize(Team team)
        {
            this.team = team;
            team.OnResourceChange += ResourceChangeHandler;
            team.OnUnitAdded += UnitAddedHandler;
            ResourceChangeHandler();
            var bgColor = team.teamColor;
            bgColor.a = .8f;
            background.color = bgColor;
        }

        private void UnitAddedHandler(Unit unit)
        {
            var unitInfo = Instantiate(unitInfoUIPrefab, unitListParent);
            unitInfo.Initialize(unit);
        }

        private void ResourceChangeHandler()
        {
            woodValue.text = team.storedResources.wood.ToString();
            stoneValue.text = team.storedResources.stone.ToString();
        }
    }
}