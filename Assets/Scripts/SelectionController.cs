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
            if (currentUnit == null) return;
            currentUnit.MoveTo(destination);
        }

        private void GatherCommandHandler(Resource resource)
        {
            if (currentUnit == null) return;
            currentUnit.GatherResource(resource);
        }

        private void FindAndGatherCommandHandler(ResourceType type)
        {
            if (currentUnit == null) return;
            currentUnit.FindAndGatherResource(type);
        }
    }
}