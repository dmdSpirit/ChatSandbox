using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace dmdspirit
{
    public class ResourceNode : MonoBehaviour
    {
        public event Action<ResourceNode> OnResourceDepleted;

        public Resource value;
        public bool isAlive { get; private set; }

        private void Start()
        {
            isAlive = true;
            transform.Rotate(new Vector3(0, 1, 0), Random.Range(-180, 180));
        }

        private void Update()
        {
            if (value.value <= 0)
            {
                OnResourceDepleted?.Invoke(this);
                isAlive = false;
                DestroyNode();
            }
        }

        public int GatherResource(int desiredValue)
        {
            var gatheredAmount = Mathf.Min(desiredValue, value.value);
            value.value -= gatheredAmount;
            return gatheredAmount;
        }

        private void DestroyNode()
        {
            Destroy(gameObject);
        }
    }
}