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

        protected Team team;

        public void Initialize(Team team, bool isFinished = false)
        {
            this.team = team;
            this.isFinished = isFinished;
            foreach (var renderer in renderers)
                renderer.material.SetColor("_Color", team.teamColor);
        }
    }
}