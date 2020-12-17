﻿using System;
using UnityEngine;

namespace dmdspirit
{
    public class BuildingSite : MonoBehaviour
    {
        public event Action OnBuildingComplete;

        [SerializeField] private ProgressBar progressBar;

        public Building Building { get; private set; }

        private Team team;
        private float buildingPoints;
        private bool isFinished;

        public void Initialize(Team team, Building buildingPrefab)
        {
            this.team = team;
            Building = Instantiate(buildingPrefab, transform);
            Building.Initialize(team);
            buildingPoints = 0;
            progressBar.SetProgress(buildingPoints / Building.buildingPointsCost);
        }

        public void AddBuildingPoints(float buildingPoints)
        {
            this.buildingPoints += buildingPoints;
            // TODO: Animate building process.
            progressBar.SetProgress(this.buildingPoints / Building.buildingPointsCost);

            if (this.buildingPoints >= Building.buildingPointsCost)
            {
                isFinished = true;
                Building.isFinished = true;
                OnBuildingComplete?.Invoke();
            }
        }

        public void DestroySite()
        {
            if (isFinished)
                Building.transform.SetParent(transform.parent);
            Destroy(gameObject);
        }
    }
}