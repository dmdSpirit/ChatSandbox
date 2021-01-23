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

    public class Building : MonoBehaviour, ICanBeHit
    {
        [SerializeField] private Renderer[] renderers;

        public Transform entrance;
        public ResourceCollection cost;
        public float buildingPointsCost;
        public BuildingType type;

        public bool isFinished;
        public int controlRadius = 1;

        public Team Team { get; protected set; }

        public void Initialize(Team team, bool isFinished = false)
        {
            Team = team;
            this.isFinished = isFinished;
            foreach (var renderer in renderers)
                renderer.material.SetColor("_Color", team.teamColor);
        }

        // TODO: Implement building HP and stuff.
        public void GetHit(float damage)
        {
            return;
        }

        public bool IsAlive() => true;

        // IMPROVE: Include building size to calculation.
        public bool IsInRage(Vector3 attacker, float range) => Vector3.Distance(attacker, transform.position) <= range;
    }
}