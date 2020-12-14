using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

namespace dmdspirit
{
    [RequireComponent(typeof(NavMeshAgent), typeof(Collider))]
    public class Unit : MonoBehaviour
    {
        private enum UnitState
        {
            Idle,
            Moving,
            MovingToResourceNode,
            Gathering,
            CarryingResourcesToBase
        }

        [SerializeField] private new Renderer renderer;
        [SerializeField] private float gatheringDistance = .5f;
        [SerializeField] private float gatheringCooldown = 1f;
        [SerializeField] private float gatheringAmount = 1;

        [SerializeField] private float maxCarryingCapacity = 10;

        // [SerializeField] private float moveStoppingDistance = 0.1f;
        [SerializeField] private LayerMask resourceLayer = default;
        [SerializeField] private float searchSphereIncrementRadius = 5f;
        [SerializeField] private TMP_Text nameplate;

        // IMPROVE: Ugly.
        public BaseBuilding baseBuilding;
        public Team team;

        public string player;

        private NavMeshAgent navAgent;
        private UnitState state;
        private Resource targetResource;

        private ResourceValue carriedResource;
        private Coroutine gatheringCoroutine;

        private void Awake()
        {
            navAgent = GetComponent<NavMeshAgent>();
            nameplate.text = string.Empty;
        }

        private void Update()
        {
            switch (state)
            {
                case UnitState.Idle:
                case UnitState.Gathering:
                // IMPROVE: Add move stopping distance check.
                case UnitState.Moving:
                    break;
                case UnitState.MovingToResourceNode:
                    if (targetResource == null)
                    {
                        if (carriedResource.value > 0)
                            ReturnResources();
                        else
                        {
                            state = UnitState.Idle;
                            StopUnit();
                        }

                        return;
                    }

                    if (Vector3.Distance(targetResource.transform.position, transform.position) <= gatheringDistance)
                    {
                        state = UnitState.Gathering;
                        gatheringCoroutine = StartCoroutine(GatherResource());
                    }

                    break;
                case UnitState.CarryingResourcesToBase:
                    if (Vector3.Distance(transform.position, baseBuilding.entrance.position) <= gatheringDistance)
                    {
                        StopUnit();
                        team.AddResource(carriedResource);
                        carriedResource = new ResourceValue();
                        state = UnitState.Idle;
                    }

                    break;
            }

            DrawPath();
        }

        public void SetName(string unitName)
        {
            name = unitName;
            nameplate.text = unitName;
            player = unitName;
        }

        public void SetUnitColor(Color color)
        {
            renderer.material.SetColor("_Color", color);
        }

        public void MoveTo(Vector3 destination)
        {
            state = UnitState.Moving;
            StartMoving(destination);
        }

        public void GatherResource(Resource resource)
        {
            Debug.Log($"{name} got gather command for {resource.name}");
            if (gatheringCoroutine != null) StopCoroutine(gatheringCoroutine);
            // FIXME: For now unit will drop everything it is carrying now.
            carriedResource = new ResourceValue();
            targetResource = resource;
            state = UnitState.MovingToResourceNode;
            StartMoving(resource.transform.position);
        }

        // IMPROVE: Clear this.
        public void FindAndGatherResource(ResourceType type)
        {
            Debug.Log($"{name} is looking for {type.ToString()} resource.");
            // IMPROVE: Move each resource to separate layer.
            Resource target = null;
            var searchRadius = searchSphereIncrementRadius;
            var resourceList = new List<Resource>();
            while (target == null)
            {
                resourceList.Clear();
                var resources = Physics.OverlapSphere(transform.position, searchRadius, resourceLayer);
                if (resources.Length != 0)
                {
                    foreach (var resourceObject in resources)
                    {
                        var resource = resourceObject.GetComponent<Resource>();
                        if (resource.value.type == type)
                            resourceList.Add(resource);
                    }

                    if (resourceList.Count != 0)
                    {
                        target = resourceList[0];
                        var distance = Vector3.Distance(transform.position, target.transform.position);
                        for (var i = 1; i < resourceList.Count; i++)
                        {
                            var newDistance = Vector3.Distance(transform.position, resourceList[i].transform.position);
                            if (newDistance < distance)
                            {
                                target = resourceList[i];
                                distance = newDistance;
                            }
                        }

                        break;
                    }
                }

                searchRadius += searchSphereIncrementRadius;
                if (searchRadius > 1000)
                {
                    Debug.Log($"{name} could not find resource of type {type.ToString()}.");
                    state = UnitState.Idle;
                    return;
                }
            }

            GatherResource(target);
        }

        private void StartMoving(Vector3 destination)
        {
            if (gatheringCoroutine != null) StopCoroutine(gatheringCoroutine);
            var path = new NavMeshPath();
            if (navAgent.CalculatePath(destination, path) == false)
            {
                Debug.LogError($"Could not find path to {destination.ToString()} for {name}");
                return;
            }

            navAgent.SetPath(path);
            Debug.Log($"{name} is moving to {destination.ToString()}");
        }

        private IEnumerator GatherResource()
        {
            StopUnit();
            carriedResource.type = targetResource.value.type;
            while (carriedResource.value < maxCarryingCapacity && targetResource != null && targetResource.value.value > 0)
            {
                var desiredValue = Mathf.Min(gatheringAmount, maxCarryingCapacity - carriedResource.value);
                var gatheredAmount = targetResource.GatherResource(desiredValue);
                carriedResource.value += gatheredAmount;
                Debug.Log($"{name} gathered {gatheredAmount} of {carriedResource.type}.");
                yield return new WaitForSeconds(gatheringCooldown);
            }

            ReturnResources();
        }

        private void DrawPath()
        {
            var path = navAgent.path;
            if (path.corners.Length <= 1) return;
            for (var i = 0; i < path.corners.Length - 1; i++)
                Debug.DrawLine(path.corners[i], path.corners[i + 1], Color.red);
        }

        private void ReturnResources()
        {
            if (carriedResource.value == 0)
            {
                state = UnitState.Idle;
                StopUnit();
                return;
            }

            Debug.Log($"{name} is carrying resources back to base.");
            state = UnitState.CarryingResourcesToBase;
            StartMoving(baseBuilding.entrance.position);
        }

        private void StopUnit()
        {
            // HACK: Check if it is working.
            navAgent.ResetPath();
        }
    }
}