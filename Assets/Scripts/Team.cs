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

        public ResourceCost storedResources;

        private List<Unit> units;
        private List<string> players;
        private List<int> botUnits;
        private List<Building> buildings;

        private int maxUnitCount = 3;

        private void Start()
        {
            baseBuilding.SetColor(teamColor);
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
            units = new List<Unit>();
            for (var i = 0; i < maxUnitCount; i++)
            {
                var unit = Instantiate(unitPrefab, baseBuilding.entrance.position, Quaternion.identity, transform);
                units.Add(unit);
                var isPlayerUnit = i < players.Count;
                string unitName;
                if (isPlayerUnit)
                {
                    unitName = players[i];
                    GameController.Instance.PlayerUnitCreated(players[i], unit);
                }
                else
                {
                    unitName = string.Concat(teamName, " ", units.Count);
                    botUnits.Add(i);
                }

                unit.Initialize(this, unitName, teamColor, isPlayerUnit);
                OnUnitAdded?.Invoke(unit);
                yield return new WaitForSeconds(spawnCooldown);
            }
        }

        public Unit SwapBotForPlayer(string userName)
        {
            if (botUnits.Count == 0) return null;
            var botId = botUnits[0];
            botUnits.Remove(botUnits[0]);

            units[botId].SwapBotForPlayer(userName);
            ui.UpdateUnitName(botId, userName);
            return units[botId];
        }
    }
}