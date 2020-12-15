using System;
using TMPro;
using UnityEngine;

namespace dmdspirit
{
    [RequireComponent(typeof(UnitBehaviour))]
    public class Unit : MonoBehaviour
    {
        [SerializeField] private new Renderer renderer;
        [SerializeField] private TMP_Text nameplate;

        public float maxCarryingCapacity = 10f;
        public ResourceValue carriedResource;

        public bool IsPlayer { get; protected set; }

        private UnitBehaviour unitBehaviour;
        public Team UnitTeam { get; protected set; }

        private void Awake()
        {
            unitBehaviour = GetComponent<UnitBehaviour>();
            carriedResource = new ResourceValue();
        }

        public void Initialize(Team team, string name, Color color, bool isPlayer = false)
        {
            UnitTeam = team;
            nameplate.text = isPlayer ? name : string.Empty;
            this.name = name;
            IsPlayer = isPlayer;
            renderer.material.SetColor("_Color", color);
            unitBehaviour.Initialize();
        }

        public void SwapBotForPlayer(string playerName)
        {
            nameplate.text = playerName;
            name = playerName;
            IsPlayer = true;
        }

        public void GatherResource(ResourceType resourceType)
        {
            unitBehaviour.GatherResource(resourceType);
        }

        public void LoadResourcesToBase()
        {
            UnitTeam.AddResource(carriedResource);
            carriedResource.type = ResourceType.None;
            carriedResource.value = 0;
        }
    }
}