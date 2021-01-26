using System;
using UnityEngine;

namespace dmdspirit
{
    public class Barracks : Building
    {
        [SerializeField] private ResourceCollection warriorCollection;
        
        protected override void Awake()
        {
            base.Awake();
            type = BuildingType.Barracks;
        }
    }
}