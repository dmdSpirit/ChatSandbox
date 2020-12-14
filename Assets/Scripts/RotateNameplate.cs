using UnityEngine;

namespace dmdspirit
{
    public class RotateNameplate : MonoBehaviour
    {
        private Quaternion rotation;

        private void Awake()
        {
            rotation = transform.rotation;
        }

        private void Update()
        {
            transform.SetPositionAndRotation(transform.position, rotation);
        }
    }
}