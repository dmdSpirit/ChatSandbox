using UnityEngine;

namespace dmdspirit
{
    public enum UnitJobType
    {
        Worker,
        Warrior
    }
    
    [CreateAssetMenu(menuName = "Unit job", fileName = "UnitJob")]
    public class UnitJob : ScriptableObject
    {
        public UnitJobType jobType;
        public Sprite icon;
        public float maxHP;
        public Projectile projectilePrefab;
        public float damage;
        public float attackRange;
        public float attackCooldown;
        public float projectileSpeed;
        public float aggroRadius;
        public Vector2 idleWalkRadius;
        public float movementSpeed;
        // IMPROVE: Create a list of available activities.
        public bool canGather;
        public int gatheringAmount;
        public float gatheringDistance;
        public float gatheringCooldown;
        public int maxCarryingCapacity;
        public bool canBuild;
        public float buildingDistance;
        public float buildingPoints;
        public float buildingPointsCooldown;
        public BuildingType buildingNeeded;
        public GameObject modelPrefab;
        public bool canPatrol;
        public float priorityGatherRadius = 40f;
    }
}