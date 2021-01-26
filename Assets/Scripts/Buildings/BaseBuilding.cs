using System;
using UnityEngine;

namespace dmdspirit
{
    public class BaseBuilding : Building
    {
        protected override void Awake()
        {
            base.Awake();
            type = BuildingType.Base;
        }
    }
}