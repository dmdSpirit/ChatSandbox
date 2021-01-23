using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

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
        public ResourceCollection storedResources;

        [SerializeField] private TeamUI ui;
        [SerializeField] private Unit unitPrefab = default;
        [SerializeField] private float spawnCooldown = 1f;
        [SerializeField] private float respawnTimer = 15;

        public List<Unit> Units { get; private set; }


        private List<Building> buildings;
        private List<ConstructionSite> constructionSites;

        private void Start()
        {
            baseBuilding.Initialize(this);
        }

        public static int GetTeamIndexFromTag(TeamTag tag) => (int) tag - 1;
        public static TeamTag GetTeamTagFromIndex(int index) => (TeamTag) (index + 1);

        public void HideUI() => ui.gameObject.SetActive(false);

        public void Initialize(List<string> players, int maxUnitCount)
        {
            buildings = new List<Building>();
            constructionSites = new List<ConstructionSite>();
            buildings.Add(baseBuilding);
            StartCoroutine(SpawnUnits(players, maxUnitCount));
            ui.gameObject.SetActive(true);
            storedResources = new ResourceCollection();
            ui.Initialize(this);
            // HACK: 
            var baseTile = baseBuilding.transform.parent.GetComponent<MapTile>();
            if (baseTile == null)
            {
                Debug.LogError($"Could not fild MapTile component if baseBuilding parent.");
                return;
            }

            Map.Instance.AddTeamControl(this, baseTile, baseBuilding.controlRadius);
        }

        public List<ICanBeHit> GetAllPotentialTargets()
        {
            var result = new List<ICanBeHit>();
            result.AddRange(Units.Where(unit => unit.IsAlive()).ToList());
            result.AddRange(buildings.Where(building => building.IsAlive() && building.isFinished));
            result.AddRange(constructionSites.Where(constructionSite => constructionSite.IsAlive()));
            return result;
        }

        public void AddBuildingSite(ConstructionSite constructionSite)
        {
            constructionSites.Add(constructionSite);
            constructionSite.OnConstructionSiteFinished += ConstructionSiteFinishedHandler;
        }

        private void ConstructionSiteFinishedHandler(ConstructionSite constructionSite)
        {
            constructionSites.Remove(constructionSite);
            AddBuilding(constructionSite);
        }

        public int GetAnyResourceUpToValue(ResourceType type, int value)
        {
            var result = storedResources.GetAnyResourceUpToValue(type, value);
            if (result != 0)
                OnResourceChange?.Invoke();
            return result;
        }

        public List<Building> GetBuildingsOfType(BuildingType type) => buildings.Where(x => x.type == type).ToList();

        public bool CheckHasBuilding(BuildingType type) => buildings.Any(x => x.type == type);

        public void AddResource(Resource value)
        {
            storedResources.AddResources(value);
            OnResourceChange?.Invoke();
        }

        public void SpendResources(ResourceCollection collection)
        {
            storedResources.TrySpendResource(collection);
            OnResourceChange?.Invoke();
        }

        public void AddBuilding(ConstructionSite constructionSite)
        {
            var building = constructionSite.Building;
            buildings.Add(building);
            constructionSite.DestroySite();
            Map.Instance.AddTeamControl(this, constructionSite.Tile, building.controlRadius);
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
                var unitName = string.Empty;
                if (isPlayerUnit)
                {
                    unitName = players[i];
                    GameController.Instance.RegisterPlayerUnit(players[i], unit);
                }

                unit.Initialize(this, i, teamColor, unitName);

                OnUnitAdded?.Invoke(unit);
                yield return new WaitForSeconds(spawnCooldown);
            }
        }

        private IEnumerator RespawnUnit(Unit unit)
        {
            yield return new WaitForSeconds(respawnTimer);
            // TODO: Update spawn timer on
            unit.transform.position = baseBuilding.entrance.position;
            unit.gameObject.SetActive(true);
            unit.Respawn();
        }

        public void SwapBotForPlayer(string userName)
        {
            var unit = Units.FirstOrDefault(x => x.IsPlayer == false);
            if (unit == null)
            {
                Debug.LogError($"{userName} is trying to join {teamName}, but the team is full.");
                return;
            }

            unit.SwapBotForPlayer(userName);
            GameController.Instance.RegisterPlayerUnit(userName, unit);
        }

        private void UnitDeathHandler(Unit unit)
        {
            unit.gameObject.SetActive(false);
            StartCoroutine(RespawnUnit(unit));
        }
    }
}