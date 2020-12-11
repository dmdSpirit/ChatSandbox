using UnityEngine;

namespace dmdspirit
{
    public class BaseBuilding : MonoBehaviour
    {
        public Transform entrance;

        [SerializeField] private Renderer[] renderers;

        public void SetColor(Color color)
        {
            foreach (var renderer in renderers)
                renderer.material.SetColor("_Color", color);
        }
    }
}