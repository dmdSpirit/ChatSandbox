using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace dmdspirit
{
    public class Team : MonoBehaviour
    {
        public event Action OnResourceChange;
        public event Action<Unit> OnUnitAdded;

        public TeamTag teamTag;
        public Color teamColor = Color.green;
        public string teamName = "team";
        public BaseBuilding baseBuilding = default;
        public ResourceCost storedResources;

        [SerializeField] private TeamUI ui;
        [SerializeField] private Unit unitPrefab = default;
        [SerializeField] private float spawnCooldown = 1f;
        [SerializeField] private float respawnTimer = 15;

        public List<Unit> Units { get; private set; }

        private List<Building> buildings;

        private void Start()
        {
            baseBuilding.Initialize(this);
        }

        public void HideUI() => ui.gameObject.SetActive(false);

        public void Initialize(List<string> players, int maxUnitCount)
        {
            buildings = new List<Building>();
            buildings.Add(baseBuilding);
            StartCoroutine(SpawnUnits(players, maxUnitCount));
            ui.gameObject.SetActive(true);
            storedResources = new ResourceCost();
            ui.Initialize(this);
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

        // IMPROVE: Add on creation of building site.
        public void AddBuilding(BuildingSite buildingSite)
        {
            var building = buildingSite.Building;
            buildings.Add(building);
            buildingSite.DestroySite();
        }

        private IEnumerator SpawnUnits(List<string> players, int maxUnitCount)
        {
            yield return new WaitForSeconds(spawnCooldown);
            Units = new List<Unit>();
            // BUG: Not handled correctly if someone !join while units are spawning.
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

                OnUnitAdded?.Invoke(unit);
                unit.Initialize(this, unitName, teamColor, isPlayerUnit);
                yield return new WaitForSeconds(spawnCooldown);
            }
        }

        private IEnumerator RespawnUnit(string unitName, bool isPlayer)
        {
            yield return new WaitForSeconds(respawnTimer);
            // TODO: Update spawn timer on UI.
            var unit = Instantiate(unitPrefab, baseBuilding.entrance.position, Quaternion.identity, transform);
            Units.Add(unit);
            unit.Initialize(this, unitName, teamColor, isPlayer);
            GameController.Instance.RegisterPlayerUnit(unitName, unit);
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
            StartCoroutine(RespawnUnit(unit.name, unit.IsPlayer));
        }
    }
}