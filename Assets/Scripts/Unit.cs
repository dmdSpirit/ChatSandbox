using System;
using TMPro;
using Unity.Jobs.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.AI;

namespace dmdspirit
{
    public interface ICanBeHit
    {
        void GetHit(float damage);
        bool IsAlive();
        bool IsInRage(Vector3 attacker, float range);
    }
    
    [RequireComponent(typeof(UnitBehaviour), typeof(NavMeshAgent))]
    public class Unit : MonoBehaviour, ICanBeHit
    {
        public event Action<Unit> OnDeath;
        public event Action OnUpdateHP;
        public event Action OnOwnerChanged;
        public event Action OnCarriedResourceChanged;
        public event Action OnJobChanged;

        [SerializeField] private new Renderer renderer;
        [SerializeField] private TMP_Text nameplate;
        [SerializeField] private ProgressBar hpBar;

        [SerializeField] private UnitJobType defaultJob = UnitJobType
            .Worker;

        [SerializeField] private UnitJob[] jobs;

        public Resource carriedResource;

        public bool IsPlayer { get; private set; }
        public Team UnitTeam { get; private set; }
        public int UnitID { get; private set; }
        public NavMeshAgent Agent { get; private set; }
        public float HP { get; private set; }
        public UnitJob CurrentJob { get; private set; }

        private UnitBehaviour unitBehaviour;

        private void Awake()
        {
            unitBehaviour = GetComponent<UnitBehaviour>();
            Agent = GetComponent<NavMeshAgent>();
            carriedResource = new Resource();
        }

        private void Update()
        {
            // HACK: For now will do.
            if (IsAlive() == false)
                Destroy(gameObject);
        }
        
        public bool IsAlive() => HP > 0;

        public void Initialize(Team team, int unitID, Color color, string playerName)
        {
            UnitTeam = team;
            nameplate.text = playerName;
            IsPlayer = string.IsNullOrEmpty(playerName) == false;
            UnitID = unitID;
            name = IsPlayer ? playerName : $"{team.teamName} {unitID}";
            renderer.material.SetColor("_Color", color);
            OnUpdateHP += UpdateHPBar;
            ChangeJob(defaultJob, true);
            unitBehaviour.Initialize(this);
        }

        public void Respawn()
        {
            HP = CurrentJob.maxHP;
            OnUpdateHP?.Invoke();
            unitBehaviour.Respawn();
        }

        public void SwapBotForPlayer(string playerName)
        {
            IsPlayer = true;
            nameplate.text = playerName;
            name = playerName;
            OnOwnerChanged?.Invoke();
        }

        public void CommandToChangeJob(UnitJobType jobType)
        {
            if (TryGetJobFromType(jobType, out var job) == false || jobType == CurrentJob.jobType || UnitTeam.CheckHasBuilding(job.buildingNeeded) == false) return;
            unitBehaviour.ChangeJob(job);
        }

        public void ChangeJob(UnitJobType jobType, bool setMaxHP = false)
        {
            // TODO: Write job factory of some sort.
            if (TryGetJobFromType(jobType, out var job) == false)
            {
                Debug.LogError($"{name} could not change job to jobType {jobType.ToString()}");
                return;
            }


            var oldMaxHP = CurrentJob == null ? job.maxHP : CurrentJob.maxHP;
            CurrentJob = job;
            if (setMaxHP)
                HP = CurrentJob.maxHP;
            else
                HP *= (CurrentJob.maxHP / oldMaxHP);
            OnUpdateHP?.Invoke();
            OnJobChanged?.Invoke();
        }

        private bool TryGetJobFromType(UnitJobType jobType, out UnitJob result)
        {
            result = null;
            foreach (var job in jobs)
            {
                if (job.jobType != jobType) continue;
                result = job;
                return true;
            }

            return false;
        }

        public void GatherResource(ResourceType resourceType)
        {
            if (CurrentJob.canGather == false) return;
            if (IsAlive() == false) return;
            unitBehaviour.GatherResource(resourceType);
        }

        public void LoadResourcesToBase()
        {
            if (carriedResource.type == ResourceType.None) return;
            UnitTeam.AddResource(carriedResource);
            DropResources();
        }

        public void Build(BuildingType buildingType, MapPosition mapPosition, TileDirection direction)
        {
            if (CurrentJob.canBuild == false) return;
            if (IsAlive() == false) return;
            unitBehaviour.Build(buildingType, mapPosition, direction);
        }

        public void AddResource(Resource resource) => AddResource(resource.type, resource.value);

        public void AddResource(ResourceType type, int quantity)
        {
            if (carriedResource.type != ResourceType.None && carriedResource.type != type)
            {
                Debug.LogError($"Something is trying to add wrong resource type to {name}.");
                return;
            }

            if (carriedResource.type == ResourceType.None)
                carriedResource.type = type;

            carriedResource.value += quantity;
            OnCarriedResourceChanged?.Invoke();
        }

        public void SpendCarriedResource(int value)
        {
            carriedResource.value -= value;
            if (carriedResource.value <= 0)
                carriedResource.Clear();
            OnCarriedResourceChanged?.Invoke();
        }

        public void DropResources()
        {
            carriedResource.Clear();
            OnCarriedResourceChanged?.Invoke();
        }

        public void AttackUnit(ICanBeHit target) => unitBehaviour.AttackUnit(target);

        public void DealDamage(ICanBeHit target)
        {
            target.GetHit(CurrentJob.damage);
        }

        private void UpdateHPBar()
        {
            var value = HP / CurrentJob.maxHP;
            hpBar.gameObject.SetActive(value != 1);
            hpBar.SetProgress(value);
        }

        public void Patrol(MapPosition first, MapPosition? second)
        {
            if (CurrentJob.canPatrol == false) return;
            unitBehaviour.Patrol(first, second);
        }

        public void Move(MapPosition position)
        {
            unitBehaviour.Move(position);
        }

        public void GetHit(float damage)
        {
            HP -= damage;
            Debug.Log($"{name} takes {damage} damage. HP: ({HP}/{CurrentJob.maxHP})");
            if (HP <= 0)
            {
                // TODO: Update unit UI on death.
                OnDeath?.Invoke(this);
                return;
            }

            OnUpdateHP?.Invoke();
        }

        public bool IsInRage(Vector3 attacker, float range) => Vector3.Distance(attacker, transform.position) <= range;
    }
}