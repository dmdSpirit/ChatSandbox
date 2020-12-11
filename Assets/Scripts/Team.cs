using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace dmdspirit
{
    public class Team : MonoBehaviour
    {
        [SerializeField] private BaseBuilding baseBuilding = default;
        [SerializeField] private int unitCount = 3;
        [SerializeField] private float spawnCooldown = 1f;
        [SerializeField] private Color teamColor = Color.green;
        [SerializeField] private Unit unitPrefab = default;
        [SerializeField] private string teamName = "team";

        private int tree;
        private int stone;
        private List<Unit> units;

        private void Start()
        {
            baseBuilding.SetColor(teamColor);
            baseBuilding.name = "Base";
            StartCoroutine(SpawnUnits());
        }

        private IEnumerator SpawnUnits()
        {
            yield return new WaitForSeconds(spawnCooldown);
            units = new List<Unit>();
            for (var i = 0; i < unitCount; i++)
            {
                var unit = Instantiate(unitPrefab, baseBuilding.entrance);
                units.Add(unit);
                unit.SetUnitColor(teamColor);
                unit.name = string.Concat(teamName, " ", units.Count);
                yield return new WaitForSeconds(spawnCooldown);
            }
        }
    }
}