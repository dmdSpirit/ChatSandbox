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
        public float maxHP;
        public float damage;
        public float attackRange;
        public float attackCooldown;
        public float aggroRadius;
        public Vector2 idleWalkRadius;
        public float movementSpeed;
        // IMPROVE: Create a list of available activities.
        public bool canGather;
        public int gatheringAmount;
        public int gatheringDistance;
        public float gatheringCooldown;
        public int maxCarryingCapacity;
        public bool canBuild;
        public float buildingDistance;
        public float buildingSpeed;
        public BuildingType buildingNeeded;
        public GameObject modelPrefab;
    }
}