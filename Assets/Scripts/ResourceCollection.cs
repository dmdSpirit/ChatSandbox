using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace dmdspirit
{
    public enum ResourceType
    {
        None,
        Wood,
        Stone
    }

    [Serializable]
    public struct Resource
    {
        public ResourceType type;
        public int value;

        public void Clear()
        {
            type = ResourceType.None;
            value = 0;
        }
    }

    [Serializable]
    public class ResourceCollection
    {
        public int[] resources;

        public ResourceCollection()
        {
            resources = new int[Enum.GetValues(typeof(ResourceType)).Length - 1];
        }

        public Dictionary<ResourceType, int> GetResourcesLeft()
        {
            var result = new Dictionary<ResourceType, int>();
            if (IsEmpty()) return result;
            for (var i = 0; i < resources.Length; i++)
            {
                if (resources[i] == 0) continue;
                result.Add(TypeFromIndex(i), resources[i]);
            }

            return result;
        }

        public ResourceCollection(ResourceCollection rc)
        {
            resources = new int[Enum.GetValues(typeof(ResourceType)).Length - 1];
            AddResources(rc);
        }

        public bool HasEnough(ResourceCollection rc) => resources.Where((resource, i) => resource < rc.resources[i]).Any() == false;
        public bool HasEnough(Resource resource) => HasEnough(resource.type, resource.value);
        public bool HasEnough(ResourceType type, int value) => resources[IndexFromType(type)] >= value;

        public bool IsEmpty() => resources.All(resource => resource == 0);

        public bool TrySpendResource(ResourceCollection rc)
        {
            if (HasEnough(rc) == false)
                return false;

            for (var i = 0; i < resources.Length; i++)
                resources[i] -= rc.resources[i];
            return true;
        }

        public int GetAnyResourceUpToValue(Resource resource) => GetAnyResourceUpToValue(resource.type, resource.value);

        public int GetAnyResourceUpToValue(ResourceType type, int value)
        {
            var result = Mathf.Min(resources[IndexFromType(type)], value);
            resources[IndexFromType(type)] -= result;
            return result;
        }

        public void AddResources(Resource addedResource) => resources[IndexFromType(addedResource.type)] += addedResource.value;

        public void AddResources(ResourceCollection addedResources)
        {
            for (var i = 0; i < resources.Length; i++)
                resources[i] += addedResources.resources[i];
        }

        public override string ToString() => string.Join(", ", resources.Select((t, i) => $"{TypeFromIndex(i)}: {t}").ToList());

        public int GetResourceValue(ResourceType type) => resources[(int) type - 1];

        private int IndexFromType(ResourceType type) => (int) type - 1;
        private ResourceType TypeFromIndex(int i) => (ResourceType) (i + 1);
    }
}