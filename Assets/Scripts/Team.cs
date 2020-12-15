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
        public event Action<ResourceType, int> OnResourceChange;
        public event Action<Unit> OnUnitAdded;

        public Color teamColor = Color.green;
        public BaseBuilding baseBuilding = default;

        [SerializeField] private float spawnCooldown = 1f;
        [SerializeField] private Unit unitPrefab = default;
        [SerializeField] private string teamName = "team";
        [SerializeField] private TeamUI ui;

        public float Wood { get; protected set; }
        public float Stone { get; protected set; }

        private List<Unit> units;
        private List<string> players;
        private List<int> botUnits;

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
            this.players = players;
            this.maxUnitCount = maxUnitCount;
            StartCoroutine(SpawnUnits());
            ui.gameObject.SetActive(true);
            ui.Initialize(this, 0, 0);
        }

        public void AddResource(ResourceValue value)
        {
            Debug.Log($"Resource added to {teamName} team: ({value.type}, {value.value}).");
            // FIXME: UGLY!
            switch (value.type)
            {
                case ResourceType.None:
                    return;
                case ResourceType.Tree:
                    Wood += value.value;
                    OnResourceChange?.Invoke(ResourceType.Tree, (int) Wood);
                    break;
                case ResourceType.Stone:
                    Stone += value.value;
                    OnResourceChange?.Invoke(ResourceType.Stone, (int) Stone);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
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