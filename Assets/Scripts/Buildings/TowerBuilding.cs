using System;
using UnityEngine;

namespace dmdspirit
{
    public class TowerBuilding : Building
    {
        private void Awake()
        {
            type = BuildingType.Tower;
        }
    }
}