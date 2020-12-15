using System;
using System.Collections.Generic;
using UnityEngine;

namespace dmdspirit
{
    public class Map : MonoSingleton<Map>
    {
        public Dictionary<ResourceType, List<Resource>> resources { get; protected set; }

        private void Awake()
        {
            resources = new Dictionary<ResourceType, List<Resource>>();
            foreach (ResourceType resourceType in Enum.GetValues(typeof(ResourceType)))
            {
                if (resourceType == ResourceType.None) continue;
                resources.Add(resourceType, new List<Resource>());
            }
        }

        private void Start()
        {
            var resourceObjects = FindObjectsOfType<Resource>();
            foreach (var resource in resourceObjects)
            {
                resources[resource.value.type].Add(resource);
                resource.OnResourceDepleted += ResourceDepletedHandler;
            }
        }

        private void ResourceDepletedHandler(Resource resource)
        {
            resources[resource.value.type].Remove(resource);
        }
    }
}