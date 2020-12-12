using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace dmdspirit
{
    public class Team : MonoBehaviour
    {
        public event Action<ResourceType, int> OnResourceChange;
        public event Action<Unit> OnUnitAdded;

        public Color teamColor = Color.green;

        [SerializeField] private BaseBuilding baseBuilding = default;
        [SerializeField] private int unitCount = 3;
        [SerializeField] private float spawnCooldown = 1f;
        [SerializeField] private Unit unitPrefab = default;
        [SerializeField] private string teamName = "team";
        [SerializeField] private TeamUI ui;

        private float tree;
        private float stone;
        private List<Unit> units;

        private void Start()
        {
            baseBuilding.SetColor(teamColor);
            baseBuilding.name = "Base";
            StartCoroutine(SpawnUnits());
            ui.Initialize(this, 0, 0);
        }

        private IEnumerator SpawnUnits()
        {
            yield return new WaitForSeconds(spawnCooldown);
            units = new List<Unit>();
            for (var i = 0; i < unitCount; i++)
            {
                var unit = Instantiate(unitPrefab, baseBuilding.entrance.position, Quaternion.identity, transform);
                units.Add(unit);
                unit.SetUnitColor(teamColor);
                unit.baseBuilding = baseBuilding;
                unit.team = this;
                unit.name = string.Concat(teamName, " ", units.Count);
                OnUnitAdded?.Invoke(unit);
                yield return new WaitForSeconds(spawnCooldown);
            }
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
                    tree += value.value;
                    OnResourceChange?.Invoke(ResourceType.Tree, (int) value.value);
                    break;
                case ResourceType.Stone:
                    stone += value.value;
                    OnResourceChange?.Invoke(ResourceType.Stone, (int) value.value);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}