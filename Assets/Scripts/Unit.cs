using System;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

namespace dmdspirit
{
    [RequireComponent(typeof(UnitBehaviour), typeof(NavMeshAgent))]
    public class Unit : MonoBehaviour
    {
        public event Action<Unit> OnDeath;
        public event Action OnUpdateHP;
        public event Action OnOwnerChanged;

        [SerializeField] private new Renderer renderer;
        [SerializeField] private TMP_Text nameplate;
        [SerializeField] private ProgressBar hpBar;

        [SerializeField] private UnitJobType defaultJob = UnitJobType
            .Worker;

        [SerializeField] private UnitJob[] jobs;

        public ResourceValue carriedResource;

        public bool IsAlive => HP > 0;
        public bool IsPlayer => string.IsNullOrEmpty(PlayerName);
        public Team UnitTeam { get; private set; }
        public int UnitID { get; private set; }
        public string PlayerName { get; private set; }
        public NavMeshAgent Agent { get; private set; }
        public float HP { get; private set; }
        public UnitJob CurrentJob { get; private set; }

        private UnitBehaviour unitBehaviour;

        private void Awake()
        {
            unitBehaviour = GetComponent<UnitBehaviour>();
            Agent = GetComponent<NavMeshAgent>();
            carriedResource = new ResourceValue();
        }

        private void Update()
        {
            // HACK: For now will do.
            if (IsAlive == false)
                Destroy(gameObject);
        }

        public void Initialize(Team team, int unitID, Color color, string playerName = "")
        {
            UnitTeam = team;
            nameplate.text = playerName;
            PlayerName = playerName;
            UnitID = unitID;
            name = IsPlayer ? PlayerName : $"{team.} {unitID}";
            renderer.material.SetColor("_Color", color);
            OnUpdateHP += UpdateHPBar;
            ChangeJob(defaultJob, true);
            unitBehaviour.Initialize(this);
        }

        public void SwapBotForPlayer(string playerName)
        {
            PlayerName = playerName;
            nameplate.text = playerName;
            name = playerName;
        }

        public void ChangeJob(UnitJobType jobType, bool setMaxHP = false)
        {
            foreach (var job in jobs)
            {
                if (job.jobType != jobType) continue;
                CurrentJob = job;
                break;
            }

            if (setMaxHP)
                HP = CurrentJob.maxHP;
            OnUpdateHP?.Invoke();
        }

        public void GatherResource(ResourceType resourceType)
        {
            if (CurrentJob.canGather == false) return;
            if (IsAlive == false) return;
            unitBehaviour.GatherResource(resourceType);
        }

        public void LoadResourcesToBase()
        {
            UnitTeam.AddResource(carriedResource);
            carriedResource.type = ResourceType.None;
            carriedResource.value = 0;
        }

        public void Build(BuildingType buildingType, MapPosition mapPosition, TileDirection direction)
        {
            if (CurrentJob.canBuild == false) return;
            if (IsAlive == false) return;
            unitBehaviour.Build(buildingType, mapPosition, direction);
        }

        public void DealDamage(Unit target)
        {
            Debug.Log($"{name} zzzaps {target.name} for {CurrentJob.damage} damage.");
            target.TakeDamage(CurrentJob.damage);
        }

        public void TakeDamage(float damage)
        {
            HP -= damage;
            Debug.Log($"{name} takes {damage} damage. HP: ({HP}/{CurrentJob.maxHP})");
            if (HP <= 0)
            {
                OnDeath?.Invoke(this);
                return;
            }

            OnUpdateHP?.Invoke();
        }

        public void AttackUnit(Unit target) => unitBehaviour.AttackUnit(target);

        private void UpdateHPBar()
        {
            var value = HP / CurrentJob.maxHP;
            hpBar.gameObject.SetActive(value != 1);
            hpBar.SetProgress(value);
        }
    }
}