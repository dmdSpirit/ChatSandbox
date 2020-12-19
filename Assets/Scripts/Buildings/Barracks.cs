using System;
using UnityEngine;

namespace dmdspirit
{
    public class Barracks : Building
    {
        [SerializeField] private ResourceCost warriorCost;
        [SerializeField] private Transform entrance;
        
        private void Awake()
        {
            type = BuildingType.Barracks;
        }
    }
}