using UnityEngine;
using UnityEngine.InputSystem;

public class InputSystemExample : MonoBehaviour
{
    //private InputActionAssetExample actionAsset;

    private PlayerInput playerInput;

    //private void Start()
    //{
    //    actionAsset = new InputActionAssetExample();
    //    actionAsset.Enable();
    //}

    private void OnEnable()
    {
        if (playerInput == null)
        {
            playerInput = GetComponentInChildren<PlayerInput>();
            playerInput.currentActionMap.Enable();
        }
        playerInput.onActionTriggered += OnActionTriggered;
    }
    private void OnDisable()
    {
        if (playerInput != null)
            playerInput.onActionTriggered -= OnActionTriggered;
    }
    private void OnActionTriggered(InputAction.CallbackContext context)
    {
        switch (context.action.name)
        {
            case "IsClick":
                Debug.Log("点击");
                break;
            case "MousePosition":
                Debug.Log(string.Format("鼠标位置：{0}", 
                    context.ReadValue<Vector2>()));
                break;
            default:
                break;
        }
    }

    private void Update()
    {
        //GetKeyInputExample();
        //GetMouseInputExample();
        //GetTouchScreenInputExample();
        //InputUtilityExample();
        //if (actionAsset.Example.IsClick.phase == InputActionPhase.Started)
        //    Debug.Log(actionAsset.Example.MousePosition.ReadValue<Vector2>());
    }
    private void InputUtilityExample()
    {
        KeyState keyState = InputUtility.GetKeyState(KeyCode.A);
        if (keyState == KeyState.PRESSED)
            Debug.Log("键盘A键开始被按下");
        else if (keyState == KeyState.HOLD)
            Debug.Log("键盘A键持续被按下");
        else if (keyState == KeyState.RELEASED)
            Debug.Log("键盘A键被抬起");
    }
    private void GetTouchScreenInputExample()
    {
        if (Touchscreen.current != null
            && Touchscreen.current.primaryTouch != null)
            Debug.Log(string.Format(
                "首要触摸输入坐标：{0}", 
                    Touchscreen.current.primaryTouch.position.ReadValue()));
    }
    private void GetMouseInputExample()
    {
        var leftButton = Mouse.current.leftButton;
        if (leftButton.wasPressedThisFrame)
            Debug.Log("鼠标左键开始被按下");
        if (leftButton.isPressed)
            Debug.Log("鼠标左键持续被按下");
        if (leftButton.wasReleasedThisFrame)
            Debug.Log("鼠标左键被抬起");
    }
    private void GetKeyInputExample()
    {
        var aKey = Keyboard.current.aKey;
        if (aKey.wasPressedThisFrame)
            Debug.Log("键盘A键开始被按下");
        if (aKey.isPressed)
            Debug.Log("键盘A键持续被按下");
        if (aKey.wasReleasedThisFrame)
            Debug.Log("键盘A键被抬起");
    }

    private bool IsClickExample(out Vector3 clickPosition)
    {
        bool isClick = false;
        clickPosition = Vector3.zero;
#if UNITY_STANDALONE
        isClick = Mouse.current != null 
            && Mouse.current.leftButton.wasPressedThisFrame;
#elif UNITY_ANDROID
        isClick = Touchscreen.current != null
            && Touchscreen.current.primaryTouch != null;
#endif
        if (isClick)
        {
#if UNITY_STANDALONE
            clickPosition = Mouse.current.position.ReadValue();
#elif UNITY_ANDROID
            clickPosition = Touchscreen.current.primaryTouch
                .position.ReadValue();
#endif
        }
        return isClick;
    }
}
