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
        [SerializeField] private float gatheringDistance = 1f;
        [SerializeField] private float gatheringAmount = 1f;
        [SerializeField] private float gatheringCooldown = 2f;

        private Unit unit;
        private Stack<State> stateStack;
        private NavMeshAgent agent;


        private void Awake()
        {
            stateStack = new Stack<State>();
            agent = GetComponent<NavMeshAgent>();
            unit = GetComponent<Unit>();
        }

        private void Update()
        {
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
            if (stateStack.Count > 0)
            {
                var currentState = stateStack.Peek();
                currentState.StopState();
            }

            var gatherState = new GatherState(agent, gatheringDistance, gatheringCooldown, gatheringAmount, resourceType);
            PushNewStateHandler(gatherState);
        }
    }
}