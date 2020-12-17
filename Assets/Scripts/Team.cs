using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace dmdspirit
{
    public class Team : MonoBehaviour
    {
        public TeamTag teamTag;
        public event Action OnResourceChange;
        public event Action<Unit> OnUnitAdded;

        public Color teamColor = Color.green;
        public BaseBuilding baseBuilding = default;

        [SerializeField] private float spawnCooldown = 1f;
        [SerializeField] private Unit unitPrefab = default;
        [SerializeField] private string teamName = "team";
        [SerializeField] private TeamUI ui;
        [SerializeField] private float respawnTimer = 15;

        public ResourceCost storedResources;

        public List<Unit> Units { get; private set; }

        private List<string> players;
        private List<int> botUnits;
        private List<Building> buildings;

        private int maxUnitCount;

        private void Start()
        {
            baseBuilding.Initialize(this);
            baseBuilding.name = "Base";
        }

        // HACK: OMG(
        public void HideUI() => ui.gameObject.SetActive(false);

        public void Initialize(List<string> players, int maxUnitCount)
        {
            botUnits = new List<int>();
            buildings = new List<Building>();
            this.players = players;
            this.maxUnitCount = maxUnitCount;
            StartCoroutine(SpawnUnits());
            ui.gameObject.SetActive(true);
            storedResources = new ResourceCost();
            ui.Initialize(this, 0, 0);
        }

        public void AddResource(ResourceValue value)
        {
            Debug.Log($"Resource added to {teamName} team: ({value.type}, {value.value}).");
            storedResources.AddResources(value);
            OnResourceChange?.Invoke();
        }

        public void SpendResources(ResourceCost cost)
        {
            storedResources.SpendResource(cost);
            OnResourceChange?.Invoke();
        }

        public void AddBuilding(BuildingSite buildingSite)
        {
            var building = buildingSite.Building;
            buildings.Add(building);
            buildingSite.DestroySite();
        }

        private IEnumerator SpawnUnits()
        {
            yield return new WaitForSeconds(spawnCooldown);
            Units = new List<Unit>();
            for (var i = 0; i < maxUnitCount; i++)
            {
                var unit = Instantiate(unitPrefab, baseBuilding.entrance.position, Quaternion.identity, transform);
                Units.Add(unit);
                unit.OnDeath += UnitDeathHandler;
                var isPlayerUnit = i < players.Count;
                string unitName;
                if (isPlayerUnit)
                {
                    unitName = players[i];
                    GameController.Instance.RegisterPlayerUnit(players[i], unit);
                }
                else
                {
                    unitName = string.Concat(teamName, " ", Units.Count);
                    botUnits.Add(i);
                }

                unit.Initialize(this, unitName, teamColor, isPlayerUnit);
                OnUnitAdded?.Invoke(unit);
                yield return new WaitForSeconds(spawnCooldown);
            }
        }

        private IEnumerator SpawnUnit(string unitName, bool isPlayer)
        {
            yield return new WaitForSeconds(respawnTimer);
            // TODO: Update spawn timer on UI.
            var unit = Instantiate(unitPrefab, baseBuilding.entrance.position, Quaternion.identity, transform);
            Units.Add(unit);
            unit.Initialize(this, unitName, teamColor, isPlayer);
            GameController.Instance.RegisterPlayerUnit(unitName, unit);
            OnUnitAdded?.Invoke(unit);
        }

        public Unit SwapBotForPlayer(string userName)
        {
            if (botUnits.Count == 0) return null;
            var botId = botUnits[0];
            botUnits.Remove(botUnits[0]);

            Units[botId].SwapBotForPlayer(userName);
            ui.UpdateUnitName(botId, userName);
            return Units[botId];
        }

        private void UnitDeathHandler(Unit unit)
        {
            Units.Remove(unit);
            StartCoroutine(SpawnUnit(unit.name, unit.IsPlayer));
        }
    }
}