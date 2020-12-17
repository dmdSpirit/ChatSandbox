using System;
using TMPro;
using UnityEngine;

namespace dmdspirit
{
    [RequireComponent(typeof(UnitBehaviour))]
    public class Unit : MonoBehaviour
    {
        public event Action<Unit> OnDeath;

        [SerializeField] private new Renderer renderer;
        [SerializeField] private TMP_Text nameplate;

        [SerializeField] private float maxHP = 10f;
        [SerializeField] private float damage = 1f;
        [SerializeField] private float attackCooldown = 2f;
        [SerializeField] private float attackDistance = 1.2f;

        [SerializeField] private ProgressBar hpBar;

        public float maxCarryingCapacity = 10f;
        public ResourceValue carriedResource;

        public bool IsAlive => hp > 0;


        private UnitBehaviour unitBehaviour;

        // IMPROVE: Duplicated by Player basically.
        public bool IsPlayer { get; protected set; }
        public string Player { get; private set; }
        public Team UnitTeam { get; protected set; }

        private float hp;

        private void Awake()
        {
            unitBehaviour = GetComponent<UnitBehaviour>();
            carriedResource = new ResourceValue();
        }

        private void Start()
        {
            hpBar.gameObject.SetActive(false);
        }

        private void Update()
        {
            // HACK: For now will do.
            if (IsAlive == false)
                Destroy(gameObject);
        }

        public void Initialize(Team team, string name, Color color, bool isPlayer = false)
        {
            UnitTeam = team;
            nameplate.text = isPlayer ? name : string.Empty;
            this.name = name;
            IsPlayer = isPlayer;
            if (isPlayer)
                Player = name;
            renderer.material.SetColor("_Color", color);
            unitBehaviour.Initialize();
            hp = maxHP;
        }

        public void SwapBotForPlayer(string playerName)
        {
            nameplate.text = playerName;
            name = playerName;
            IsPlayer = true;
            Player = playerName;
        }

        public void GatherResource(ResourceType resourceType)
        {
            if (IsAlive == false) return;
            unitBehaviour.GatherResource(resourceType);
        }

        public void LoadResourcesToBase()
        {
            UnitTeam.AddResource(carriedResource);
            carriedResource.type = ResourceType.None;
            carriedResource.value = 0;
        }

        public void Build(BuildingType buildingType, MapPosition mapPosition)
        {
            if (IsAlive == false) return;
            unitBehaviour.Build(buildingType, mapPosition);
        }

        public void DealDamage(Unit target)
        {
            Debug.Log($"{name} zzzaps {target.name} for {damage} damage.");
            target.TakeDamage(damage);
        }

        public void TakeDamage(float damage)
        {
            hp -= damage;
            Debug.Log($"{name} takes {damage} damage. HP: ({hp}/{maxHP})");
            // TODO: Update hp bar.
            if (hp <= 0)
                OnDeath?.Invoke(this);
            else
            {
                if (hpBar.gameObject.activeSelf == false)
                    hpBar.gameObject.SetActive(true);
                hpBar.SetProgress(hp / maxHP);
            }
        }

        public void AttackUnit(Unit target)
        {
            unitBehaviour.AttackUnit(target, attackDistance, attackCooldown);
        }
    }
}