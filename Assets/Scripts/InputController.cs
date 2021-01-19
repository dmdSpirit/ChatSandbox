using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace dmdspirit
{
    public class InputController : MonoSingleton<InputController>
    {
        public event Action<Unit> OnUnitSelected;
        public event Action<Vector3> OnMoveCommand;
        public event Action<ResourceNode> OnGatherCommand;
        public event Action<ResourceType> OnFindAndGatherCommand;

        [SerializeField] private LayerMask floorAndResourceMask;

        private InputActions inputActions;

        private void Awake()
        {
            inputActions = new InputActions();
            inputActions.@base.MouseLeftClick.performed += LeftClick;
            inputActions.@base.MouseRightClick.performed += RightClick;
            inputActions.@base.GatherStone.performed += (InputAction.CallbackContext cx) => OnFindAndGatherCommand?.Invoke(ResourceType.Stone);
            inputActions.@base.GatherTree.performed += (InputAction.CallbackContext cx) => OnFindAndGatherCommand?.Invoke(ResourceType.Wood);
        }

        private void OnEnable() => inputActions?.Enable();
        private void OnDisable() => inputActions?.Disable();

        // Select Unit.
        private void LeftClick(InputAction.CallbackContext context)
        {
            var position = inputActions.@base.MousePosition.ReadValue<Vector2>();
            if (position.x > Screen.width || position.y > Screen.height || position.x < 0 || position.y < 0) return;
            var ray = Camera.main.ScreenPointToRay(position);
            if (Physics.Raycast(ray, out var hit))
            {
                var unit = hit.collider.GetComponent<Unit>();
                if (unit == null) return;
                OnUnitSelected?.Invoke(unit);
            }
        }

        // Issue movement command.
        private void RightClick(InputAction.CallbackContext context)
        {
            var position = inputActions.@base.MousePosition.ReadValue<Vector2>();
            if (position.x > Screen.width || position.y > Screen.height || position.x < 0 || position.y < 0) return;
            var ray = Camera.main.ScreenPointToRay(position);
            if (Physics.Raycast(ray, out var hit, 1000, floorAndResourceMask))
            {
                var resource = hit.collider.GetComponent<ResourceNode>();
                if (resource == null)
                    OnMoveCommand?.Invoke(hit.point);
                else
                    OnGatherCommand?.Invoke(resource);
            }
        }
    }
}