// GENERATED AUTOMATICALLY FROM 'Assets/InputActions.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace dmdspirit
{
    public class @InputActions : IInputActionCollection, IDisposable
    {
        public InputActionAsset asset { get; }
        public @InputActions()
        {
            asset = InputActionAsset.FromJson(@"{
    ""name"": ""InputActions"",
    ""maps"": [
        {
            ""name"": ""base"",
            ""id"": ""c9029b03-33e6-4a68-a276-f391b39e7406"",
            ""actions"": [
                {
                    ""name"": ""Mouse Left Click"",
                    ""type"": ""Button"",
                    ""id"": ""b38a9916-e6e3-4ec0-85e3-8b310facd0b5"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Mouse Position"",
                    ""type"": ""Value"",
                    ""id"": ""5606a810-6737-403b-81d0-87e1eade2aa1"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Mouse Right Click"",
                    ""type"": ""Button"",
                    ""id"": ""268b5085-4b78-48a1-9ee5-f5b10066b9b9"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""a826ffa7-e9d0-44d8-91fe-a30b0d16112b"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Mouse Left Click"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""6397fe3a-d91d-4dd5-a8e2-18473210ed9d"",
                    ""path"": ""<Mouse>/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Mouse Position"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""bc8ece84-4dd2-4fde-97f1-5f9de70188b9"",
                    ""path"": ""<Mouse>/rightButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Mouse Right Click"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": [
        {
            ""name"": ""MainScheme"",
            ""bindingGroup"": ""MainScheme"",
            ""devices"": [
                {
                    ""devicePath"": ""<Mouse>"",
                    ""isOptional"": false,
                    ""isOR"": false
                }
            ]
        }
    ]
}");
            // base
            m_base = asset.FindActionMap("base", throwIfNotFound: true);
            m_base_MouseLeftClick = m_base.FindAction("Mouse Left Click", throwIfNotFound: true);
            m_base_MousePosition = m_base.FindAction("Mouse Position", throwIfNotFound: true);
            m_base_MouseRightClick = m_base.FindAction("Mouse Right Click", throwIfNotFound: true);
        }

        public void Dispose()
        {
            UnityEngine.Object.Destroy(asset);
        }

        public InputBinding? bindingMask
        {
            get => asset.bindingMask;
            set => asset.bindingMask = value;
        }

        public ReadOnlyArray<InputDevice>? devices
        {
            get => asset.devices;
            set => asset.devices = value;
        }

        public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

        public bool Contains(InputAction action)
        {
            return asset.Contains(action);
        }

        public IEnumerator<InputAction> GetEnumerator()
        {
            return asset.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Enable()
        {
            asset.Enable();
        }

        public void Disable()
        {
            asset.Disable();
        }

        // base
        private readonly InputActionMap m_base;
        private IBaseActions m_BaseActionsCallbackInterface;
        private readonly InputAction m_base_MouseLeftClick;
        private readonly InputAction m_base_MousePosition;
        private readonly InputAction m_base_MouseRightClick;
        public struct BaseActions
        {
            private @InputActions m_Wrapper;
            public BaseActions(@InputActions wrapper) { m_Wrapper = wrapper; }
            public InputAction @MouseLeftClick => m_Wrapper.m_base_MouseLeftClick;
            public InputAction @MousePosition => m_Wrapper.m_base_MousePosition;
            public InputAction @MouseRightClick => m_Wrapper.m_base_MouseRightClick;
            public InputActionMap Get() { return m_Wrapper.m_base; }
            public void Enable() { Get().Enable(); }
            public void Disable() { Get().Disable(); }
            public bool enabled => Get().enabled;
            public static implicit operator InputActionMap(BaseActions set) { return set.Get(); }
            public void SetCallbacks(IBaseActions instance)
            {
                if (m_Wrapper.m_BaseActionsCallbackInterface != null)
                {
                    @MouseLeftClick.started -= m_Wrapper.m_BaseActionsCallbackInterface.OnMouseLeftClick;
                    @MouseLeftClick.performed -= m_Wrapper.m_BaseActionsCallbackInterface.OnMouseLeftClick;
                    @MouseLeftClick.canceled -= m_Wrapper.m_BaseActionsCallbackInterface.OnMouseLeftClick;
                    @MousePosition.started -= m_Wrapper.m_BaseActionsCallbackInterface.OnMousePosition;
                    @MousePosition.performed -= m_Wrapper.m_BaseActionsCallbackInterface.OnMousePosition;
                    @MousePosition.canceled -= m_Wrapper.m_BaseActionsCallbackInterface.OnMousePosition;
                    @MouseRightClick.started -= m_Wrapper.m_BaseActionsCallbackInterface.OnMouseRightClick;
                    @MouseRightClick.performed -= m_Wrapper.m_BaseActionsCallbackInterface.OnMouseRightClick;
                    @MouseRightClick.canceled -= m_Wrapper.m_BaseActionsCallbackInterface.OnMouseRightClick;
                }
                m_Wrapper.m_BaseActionsCallbackInterface = instance;
                if (instance != null)
                {
                    @MouseLeftClick.started += instance.OnMouseLeftClick;
                    @MouseLeftClick.performed += instance.OnMouseLeftClick;
                    @MouseLeftClick.canceled += instance.OnMouseLeftClick;
                    @MousePosition.started += instance.OnMousePosition;
                    @MousePosition.performed += instance.OnMousePosition;
                    @MousePosition.canceled += instance.OnMousePosition;
                    @MouseRightClick.started += instance.OnMouseRightClick;
                    @MouseRightClick.performed += instance.OnMouseRightClick;
                    @MouseRightClick.canceled += instance.OnMouseRightClick;
                }
            }
        }
        public BaseActions @base => new BaseActions(this);
        private int m_MainSchemeSchemeIndex = -1;
        public InputControlScheme MainSchemeScheme
        {
            get
            {
                if (m_MainSchemeSchemeIndex == -1) m_MainSchemeSchemeIndex = asset.FindControlSchemeIndex("MainScheme");
                return asset.controlSchemes[m_MainSchemeSchemeIndex];
            }
        }
        public interface IBaseActions
        {
            void OnMouseLeftClick(InputAction.CallbackContext context);
            void OnMousePosition(InputAction.CallbackContext context);
            void OnMouseRightClick(InputAction.CallbackContext context);
        }
    }
}
