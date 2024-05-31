// GENERATED AUTOMATICALLY FROM 'Assets/Scripts/Input/InputActionAssetExample.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class @InputActionAssetExample : IInputActionCollection, IDisposable
{
    public InputActionAsset asset { get; }
    public @InputActionAssetExample()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""InputActionAssetExample"",
    ""maps"": [
        {
            ""name"": ""Example"",
            ""id"": ""c9f155ed-b7b6-4d72-97f3-241578ce9b41"",
            ""actions"": [
                {
                    ""name"": ""IsClick"",
                    ""type"": ""Button"",
                    ""id"": ""13422762-1ec8-4eab-b45f-2c76a50f44e2"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""MousePosition"",
                    ""type"": ""Value"",
                    ""id"": ""3709cb72-64c3-4fd5-870d-19b7aec0cb76"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""b93c82bf-bb38-4a48-a233-d813a009f98a"",
                    ""path"": ""<Mouse>/press"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""IsClick"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""ab96aa73-7eba-4315-8f48-5a28736ef7e1"",
                    ""path"": ""<Touchscreen>/press"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""IsClick"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""915c3329-19db-4a76-8a17-ca68dee7266a"",
                    ""path"": ""<Mouse>/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MousePosition"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""5cd322fd-d881-4aa0-b3c3-9d34eb21b53d"",
                    ""path"": ""<Touchscreen>/primaryTouch/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MousePosition"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // Example
        m_Example = asset.FindActionMap("Example", throwIfNotFound: true);
        m_Example_IsClick = m_Example.FindAction("IsClick", throwIfNotFound: true);
        m_Example_MousePosition = m_Example.FindAction("MousePosition", throwIfNotFound: true);
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

    // Example
    private readonly InputActionMap m_Example;
    private IExampleActions m_ExampleActionsCallbackInterface;
    private readonly InputAction m_Example_IsClick;
    private readonly InputAction m_Example_MousePosition;
    public struct ExampleActions
    {
        private @InputActionAssetExample m_Wrapper;
        public ExampleActions(@InputActionAssetExample wrapper) { m_Wrapper = wrapper; }
        public InputAction @IsClick => m_Wrapper.m_Example_IsClick;
        public InputAction @MousePosition => m_Wrapper.m_Example_MousePosition;
        public InputActionMap Get() { return m_Wrapper.m_Example; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(ExampleActions set) { return set.Get(); }
        public void SetCallbacks(IExampleActions instance)
        {
            if (m_Wrapper.m_ExampleActionsCallbackInterface != null)
            {
                @IsClick.started -= m_Wrapper.m_ExampleActionsCallbackInterface.OnIsClick;
                @IsClick.performed -= m_Wrapper.m_ExampleActionsCallbackInterface.OnIsClick;
                @IsClick.canceled -= m_Wrapper.m_ExampleActionsCallbackInterface.OnIsClick;
                @MousePosition.started -= m_Wrapper.m_ExampleActionsCallbackInterface.OnMousePosition;
                @MousePosition.performed -= m_Wrapper.m_ExampleActionsCallbackInterface.OnMousePosition;
                @MousePosition.canceled -= m_Wrapper.m_ExampleActionsCallbackInterface.OnMousePosition;
            }
            m_Wrapper.m_ExampleActionsCallbackInterface = instance;
            if (instance != null)
            {
                @IsClick.started += instance.OnIsClick;
                @IsClick.performed += instance.OnIsClick;
                @IsClick.canceled += instance.OnIsClick;
                @MousePosition.started += instance.OnMousePosition;
                @MousePosition.performed += instance.OnMousePosition;
                @MousePosition.canceled += instance.OnMousePosition;
            }
        }
    }
    public ExampleActions @Example => new ExampleActions(this);
    public interface IExampleActions
    {
        void OnIsClick(InputAction.CallbackContext context);
        void OnMousePosition(InputAction.CallbackContext context);
    }
}
