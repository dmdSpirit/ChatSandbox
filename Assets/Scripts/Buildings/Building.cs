using UnityEngine;

namespace dmdspirit
{
    public class Building : MonoBehaviour
    {
        [SerializeField] private Renderer[] renderers;

        public ResourceCost cost;
        public float buildingPointsCost;
        public BuildingType type;
        
        public void SetColor(Color color)
        {
            foreach (var renderer in renderers)
                renderer.material.SetColor("_Color", color);
        }
    }
}