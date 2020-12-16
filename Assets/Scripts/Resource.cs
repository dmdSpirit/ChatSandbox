﻿using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace dmdspirit
{
    public enum ResourceType
    {
        None,
        Wood,
        Stone
    }

    [Serializable]
    public struct ResourceValue
    {
        public ResourceType type;
        // TODO: All resource values should be int.
        public float value;
    }

    public class Resource : MonoBehaviour
    {
        public event Action<Resource> OnResourceDepleted;

        public ResourceValue value;

        private void Start()
        {
            transform.Rotate(new Vector3(0, 1, 0), Random.Range(-180, 180));
        }

        private void Update()
        {
            if (value.value <= 0)
            {
                OnResourceDepleted?.Invoke(this);
                DestroyNode();
            }
        }

        public float GatherResource(float desiredValue)
        {
            var gatheredAmount = Mathf.Min(desiredValue, value.value);
            value.value -= gatheredAmount;
            return gatheredAmount;
        }

        private void DestroyNode()
        {
            Debug.Log($"{name} was depleted.");
            Destroy(gameObject);
        }
    }
}