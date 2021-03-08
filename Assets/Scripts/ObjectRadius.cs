using UnityEngine;

namespace dmdspirit
{
    public class ObjectRadius : MonoBehaviour
    {
        [SerializeField] private float radius;

        // TODO: Replace with calculation based on object form.
        public Vector3 GetClosestPoint(Vector3 position) => Vector3.MoveTowards(transform.position, position, radius);
    }
}