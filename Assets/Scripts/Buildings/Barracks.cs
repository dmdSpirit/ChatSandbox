using System;
using UnityEngine;

namespace dmdspirit
{
    public class Barracks : Building
    {
        [SerializeField] private ResourceCollection warriorCollection;
        
        private void Awake()
        {
            type = BuildingType.Barracks;
        }
    }
}