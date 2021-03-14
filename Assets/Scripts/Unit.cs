using System;
using TMPro;
using Unity.Jobs.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.AI;

namespace dmdspirit
{
    [RequireComponent(typeof(UnitBehaviour), typeof(NavMeshAgent))]
    public class Unit : MonoBehaviour
    {

        public event Action<Unit> OnDeath;
        public event Action OnOwnerChanged;
        public event Action OnCarriedResourceChanged;
        public event Action OnJobChanged;

        [SerializeField] private new Renderer renderer;
        [SerializeField] private TMP_Text nameplate;

        [SerializeField] private UnitJobType defaultJob = UnitJobType
            .Worker;

        [SerializeField] private UnitJob[] jobs;

        public Resource carriedResource;

        public bool IsPlayer { get; private set; }
        public Team UnitTeam { get; private set; }
        public int UnitID { get; private set; }
        public NavMeshAgent Agent { get; private set; }
        public UnitJob CurrentJob { get; private set; }
        public HitPoints HitPoints { get; private set; }

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
            if (HitPoints.IsAlive == false)
                Destroy(gameObject);
        }

        public void Initialize(Team team, int unitID, Color color, string playerName)
        {
            HitPoints = GetComponent<HitPoints>();
            UnitTeam = team;
            nameplate.text = playerName;
            IsPlayer = string.IsNullOrEmpty(playerName) == false;
            UnitID = unitID;
            name = IsPlayer ? playerName : $"{team.teamName} {unitID}";
            renderer.material.SetColor("_Color", color);
            ChangeJob(defaultJob, true);
            unitBehaviour.Initialize(this);
            HitPoints.OnDeath += DeathHandler;
        }

        // IMRPOVE: Clear this.
        private void DeathHandler()
        {
            OnDeath?.Invoke(this);
        }

        public void Respawn()
        {
            HitPoints.ResetHP();
            unitBehaviour.Respawn();
        }

        public void SwapBotForPlayer(string playerName)
        {
            IsPlayer = true;
            nameplate.text = playerName;
            name = playerName;
            OnOwnerChanged?.Invoke();
        }

        public void ChangeJob(UnitJobType jobType, bool setMaxHP = false)
        {
            // TODO: Write job factory of some sort.
            if (TryGetJobFromType(jobType, out var job) == false)
            {
                Debug.LogError($"{name} could not change job to jobType {jobType.ToString()}");
                return;
            }

            CurrentJob = job;

            HitPoints.ChageMaxHP(CurrentJob.maxHP, setMaxHP);
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

        public void LoadResourcesToBase()
        {
            if (carriedResource.type == ResourceType.None) return;
            UnitTeam.AddResource(carriedResource);
            DropResources();
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

        public void AttackTarget(HitPoints target) => unitBehaviour.AttackTarget(target);

        public void DealDamage(HitPoints target) => target.GetHit(CurrentJob.damage);

        public void Command(ChatParser.Command command)
        {
            if (HitPoints.IsAlive == false) return;
            switch (command.commandType)
            {
                case ChatParser.ChatCommands.Gather:
                    GatherResource(command.resourceType);
                    break;
                case ChatParser.ChatCommands.Build:
                    if (CurrentJob.canBuild == false) return;
                    unitBehaviour.Build(command.buildingType, command.position, command.direction);
                    break;
                case ChatParser.ChatCommands.Job:
                    if (command.jobType == CurrentJob.jobType || TryGetJobFromType(command.jobType, out var job) == false || UnitTeam.CheckHasBuilding(job.buildingNeeded) == false) return;
                    unitBehaviour.ChangeJob(job);
                    break;
                case ChatParser.ChatCommands.Patrol:
                    if (CurrentJob.canPatrol == false) return;
                    unitBehaviour.Patrol(command.position, command.secondPosition);
                    break;
                case ChatParser.ChatCommands.Move:
                    unitBehaviour.Move(command.position);
                    break;
                case ChatParser.ChatCommands.Kill:
                    var enemyTeam = GameController.Instance.GetEnemyTeam(UnitTeam);
                    Unit target = string.IsNullOrEmpty(command.targetName) == false ? enemyTeam.GetUnit(command.targetIndex) : (command.teamTag != UnitTeam.teamTag ? enemyTeam.GetUnit(command.targetIndex) : null);
                    if (target == null) return;
                    unitBehaviour.AttackTarget(target.HitPoints);
                    break;
            }
        }

        public void ShootProjectile(HitPoints target)
        {
            // TODO: Pool projectiles.
            var projectile = Instantiate(CurrentJob.projectilePrefab, transform.position, Quaternion.identity);
            projectile.Initialize(target, CurrentJob.damage, CurrentJob.projectileSpeed, UnitTeam.teamColor);
        }

        public void GatherNode(ResourceNode node)
        {
            if (CurrentJob.canGather == false) return;
            unitBehaviour.GatherResourceNode(node);
        }

        public void GatherResource(ResourceType type)
        {
            if (CurrentJob.canGather == false) return;
            unitBehaviour.GatherResource(type);
        }

        public void StopDelivery() => unitBehaviour.StopDelivery();
    }
}