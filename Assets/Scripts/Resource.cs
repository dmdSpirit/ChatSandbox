using UnityEngine;

namespace dmdspirit
{
    public enum ResourceType
    {
        None,
        Tree,
        Stone
    }

    public class Resource : MonoBehaviour
    {
        public ResourceType type;
        public int value;

        private void Start()
        {
            transform.Rotate(new Vector3(0, 1, 0), Random.Range(-180, 180));
        }
    }
}