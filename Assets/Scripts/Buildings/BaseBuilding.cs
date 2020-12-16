using System;
using UnityEngine;

namespace dmdspirit
{
    public class BaseBuilding : Building
    {
        public Transform entrance;

        private void Awake()
        {
            type = BuildingType.Base;
        }
    }
}