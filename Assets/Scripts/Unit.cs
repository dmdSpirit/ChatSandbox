using System;
using UnityEngine;
using UnityEngine.AI;

namespace dmdspirit
{
    [RequireComponent(typeof(NavMeshAgent), typeof(Collider))]
    public class Unit : MonoBehaviour
    {
        [SerializeField] private Renderer renderer;
        
        private NavMeshAgent navAgent;
        private new Collider collider;

        private bool isMoving;

        private void Awake()
        {
            navAgent = GetComponent<NavMeshAgent>();
            collider = GetComponent<Collider>();
        }

        public void SetUnitColor(Color color)
        {
            renderer.material.SetColor("_Color", color);
        }

        public void MoveTo(Vector3 destination)
        {
            var path = new NavMeshPath();
            if (navAgent.CalculatePath(destination, path) == false)
            {
                Debug.LogError($"Could not find path to {destination.ToString()} for {name}");
                return;
            }

            DrawPath(path);
            navAgent.SetPath(path);
            Debug.Log($"{name} is moving to {destination.ToString()}");
        }

        public void GatherResource(Resource resource)
        {
            
        }
        
        private void DrawPath(NavMeshPath path)
        {
        }
    }
}