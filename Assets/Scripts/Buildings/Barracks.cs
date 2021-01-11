using System;
using UnityEngine;

namespace dmdspirit
{
    public class Barracks : Building
    {
        [SerializeField] private ResourceCost warriorCost;
        
        private void Awake()
        {
            type = BuildingType.Barracks;
        }
    }
}