using System;
using UnityEngine;

namespace dmdspirit
{
    public enum BuildingType
    {
        None,
        Base,
        Tower,
        Barracks
    }

    public class Building : MonoBehaviour
    {
        [SerializeField] private Renderer[] renderers;

        public Transform entrance;
        public ResourceCollection cost;
        public float buildingPointsCost;
        public BuildingType type;

        public bool isFinished;
        public int controlRadius = 1;
        public float maxHP = 10f;
        
        public HitPoints HitPoints { get; protected set; }

        public Team Team { get; protected set; }

        protected virtual void Awake(){}

        public void Initialize(Team team, bool isFinished = false)
        {
            HitPoints = GetComponent<HitPoints>();
            HitPoints.OnDeath += DeathHandler;
            Team = team;
            this.isFinished = isFinished;
            foreach (var renderer in renderers)
                renderer.material.SetColor("_Color", team.teamColor);
            HitPoints.Initialize(maxHP);
        }

        private void DeathHandler()
        {
            Destroy(gameObject);
        }
    }
}