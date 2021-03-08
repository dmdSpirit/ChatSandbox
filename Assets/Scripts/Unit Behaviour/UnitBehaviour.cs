using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace dmdspirit
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class UnitBehaviour : MonoBehaviour
    {
        private Unit unit;
        private Stack<State> stateStack;
        private SearchForEnemiesState searchForEnemiesState;

        private void Update()
        {
            // HACK:
            if (unit.HitPoints.IsAlive == false) return;
            searchForEnemiesState?.Update();
            // TODO: Handle empty state stack.
            if (stateStack.Count != 0)
                stateStack.Peek().Update();
            else
            {
                var idleState = new IdleState(unit);
                PushNewStateHandler(idleState);
            }
        }

        public void Initialize(Unit unit)
        {
            this.unit = unit;
            stateStack = new Stack<State>();
            PushNewStateHandler(new IdleState(unit));
            StartSearchingForEnemies();
            unit.OnDeath += DeathHandler;
        }

        private void DeathHandler(Unit unit)
        {
            StopCurrentState();
            searchForEnemiesState.StopState();
            searchForEnemiesState = null;
        }

        public void Respawn()
        {
            StartSearchingForEnemies();
            PushNewStateHandler(new IdleState(unit));
        }

        public void GatherResource(ResourceType resourceType)
        {
            if (resourceType == ResourceType.None)
                return;
            StopCurrentState();
            var gatherState = new GatherState(unit, resourceType);
            PushNewStateHandler(gatherState);
        }

        public void GatherResourceNode(ResourceNode node)
        {
            if (node == null) return;
            StopCurrentState();
            var gatherState = new GatherState(unit, node);
            PushNewStateHandler(gatherState);
        }

        public void ChangeJob(UnitJob job)
        {
            StopCurrentState();
            PushNewStateHandler(new ChangeJobState(unit, job));
        }

        public void Build(BuildingType buildingType, MapPosition mapPosition, TileDirection direction)
        {
            var mapTile = Map.Instance.GetTile(mapPosition);
            if (mapTile.CheckCanBuild(unit.UnitTeam.teamTag, buildingType) == false) return;
            if (mapTile.ConstructionSite != null)
                buildingType = mapTile.ConstructionSite.Building.type;
            StopCurrentState();
            // TODO: Check if buildingType is specified or if there is a construction site.
            var buildState = new BuildState(unit, Map.Instance.GetTile(mapPosition), buildingType, direction);
            PushNewStateHandler(buildState);
        }

        public void AttackTarget(HitPoints target)
        {
            var attackState = new AttackState(unit, target);
            attackState.OnStateFinish += (x) => StartSearchingForEnemies();
            PushNewStateHandler(attackState);
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
            newState.OnStateFinish += StateFinishHandler;
            newState.OnPushState += PushNewStateHandler;
            stateStack.Push(newState);
        }

        private void StopCurrentState()
        {
            if (stateStack.Count == 0) return;
            var currentState = stateStack.Peek();
            currentState.StopState();
        }

        private void StartSearchingForEnemies()
        {
            searchForEnemiesState = new SearchForEnemiesState(unit);
            searchForEnemiesState.OnStateFinish += (x) => searchForEnemiesState = null;
        }

        public void Patrol(MapPosition first, MapPosition? second)
        {
            StopCurrentState();
            PushNewStateHandler(new PatrolState(unit, first, second));
        }

        public void Move(MapPosition position)
        {
            StopCurrentState();
            PushNewStateHandler(new MoveState(unit, Map.Instance.GetTile(position).transform.position, 1.5f));
        }

        public void StopDelivery()
        {
            var currentState = stateStack.Peek();
            bool isDelivery = currentState is DeliveryState;
            var state = currentState;
            while (state != null && isDelivery == false)
            {
                state = state.parentState;
                isDelivery = state is DeliveryState;
            }

            if (isDelivery)
                StopCurrentState();
        }
    }
}