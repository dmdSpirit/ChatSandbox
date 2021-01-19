using UnityEngine;
using UnityEngine.InputSystem;

namespace dmdspirit
{
    public class SelectionController : MonoBehaviour
    {
        public Unit currentUnit;

        private void Start()
        {
            InputController.Instance.OnUnitSelected += UnitSelectedHandler;
            InputController.Instance.OnMoveCommand += MoveCommandHandler;
            InputController.Instance.OnGatherCommand += GatherCommandHandler;
            InputController.Instance.OnFindAndGatherCommand += FindAndGatherCommandHandler;
        }

        private void UnitSelectedHandler(Unit unit)
        {
            if (currentUnit == unit) return;
            currentUnit = unit;
            Debug.Log($"{unit.name} is selected.");
        }

        private void MoveCommandHandler(Vector3 destination)
        {
        }

        private void GatherCommandHandler(ResourceNode resourceNode)
        {
        }

        private void FindAndGatherCommandHandler(ResourceType type)
        {
        }
    }
}