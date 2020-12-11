using UnityEngine;

namespace dmdspirit
{
    public class SelectionController : MonoBehaviour
    {
        public Unit currentUnit;

        private void Start()
        {
            InputController.Instance.OnUnitSelected += UnitSelectedHandler;
            InputController.Instance.OnMoveCommand += MoveCommandHandler;
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
    }
}