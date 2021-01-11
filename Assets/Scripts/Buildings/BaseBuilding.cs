using System;
using UnityEngine;

namespace dmdspirit
{
    public class BaseBuilding : Building
    {

        private void Awake()
        {
            type = BuildingType.Base;
        }
    }
}