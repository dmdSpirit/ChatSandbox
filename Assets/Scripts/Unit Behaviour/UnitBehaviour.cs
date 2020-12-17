using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace dmdspirit
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class UnitBehaviour : MonoBehaviour
    {
        [SerializeField] private float idleWalkRadius = 5f;
        [SerializeField] private float aggroRadius = 10f;

        public float gatheringDistance = 1f;
        public float gatheringAmount = 1f;
        public float gatheringCooldown = 2f;
        public float buildingRadius = 3f;
        public float buildingSpeed = 1f;

        private Unit unit;
        private Stack<State> stateStack;
        private SearchForEnemiesState searchForEnemiesState;
        private NavMeshAgent agent;


        private void Awake()
        {
            stateStack = new Stack<State>();
            agent = GetComponent<NavMeshAgent>();
            unit = GetComponent<Unit>();
        }

        private void Update()
        {
            // HACK:
            if (unit.IsAlive == false) return;
            searchForEnemiesState?.Update();
            // TODO: Handle empty state stack.
            if (stateStack.Count != 0)
                stateStack.Peek().Update();
            else
            {
                var idleState = new IdleState(agent, idleWalkRadius);
                PushNewStateHandler(idleState);
            }
        }

        public void Initialize()
        {
            var idleState = new IdleState(agent, idleWalkRadius);
            PushNewStateHandler(idleState);
            StartSearchingForEnemies();
        }

        private void StateFinishHandler(State finishedState)
        {
            // FIXME: Handle potential indefinite loops.
            var currentState = stateStack.Peek();
            if (currentState != finishedState)
            {
                Debug.LogError($"Trying to finish state {finishedState.GetType()} but it is not on top of the state stack.");
                return;
            }

            stateStack.Pop();
        }

        private void PushNewStateHandler(State newState)
        {
            stateStack.Push(newState);
            newState.OnStateFinish += StateFinishHandler;
            newState.OnPushState += PushNewStateHandler;
        }

        public void GatherResource(ResourceType resourceType)
        {
            StopCurrentState();
            var gatherState = new GatherState(agent, gatheringDistance, gatheringCooldown, gatheringAmount, resourceType);
            PushNewStateHandler(gatherState);
        }

        public void Build(BuildingType buildingType, MapPosition mapPosition)
        {
            StopCurrentState();
            var buildState = new BuildState(agent, buildingType, Map.Instance.GetTile(mapPosition), buildingRadius, buildingSpeed);
            PushNewStateHandler(buildState);
        }

        private void StopCurrentState()
        {
            if (stateStack.Count > 0)
            {
                var currentState = stateStack.Peek();
                currentState.StopState();
            }
        }

        public void AttackUnit(Unit target, float attackDistance, float attackCooldown)
        {
            var attackState = new AttackState(unit, target, attackDistance, attackCooldown);
            attackState.OnStateFinish += (x) => StartSearchingForEnemies();
            PushNewStateHandler(attackState);
        }

        private void StartSearchingForEnemies()
        {
            searchForEnemiesState = new SearchForEnemiesState(unit, aggroRadius);
            searchForEnemiesState.OnStateFinish += (x) => searchForEnemiesState = null;
        }
    }
}