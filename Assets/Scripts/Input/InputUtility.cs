using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
#endif

public class InputUtility
{
    /// <summary>
    /// 获取指定按键的状态
    /// </summary>
    /// <param name="keyCode">按键</param>
    /// <returns>按键状态</returns>
    public static KeyState GetKeyState(KeyCode keyCode)
    {
        if (keyCode == KeyCode.None) return KeyState.NONE;

//未使用新的输入系统
#if !ENABLE_INPUT_SYSTEM
        return Input.GetKeyDown(keyCode) ? KeyState.PRESSED
            : Input.GetKey(keyCode) ? KeyState.HOLD
            : Input.GetKeyUp(keyCode) ? KeyState.RELEASED
            : KeyState.NONE;
//使用新的输入系统
#else
        ButtonControl buttonCtrl;
        int keyCodeV = (int)keyCode;
        //字母A-Z
        if (keyCodeV >= (int)KeyCode.A 
            && keyCodeV <= (int)KeyCode.Z)
        {
            buttonCtrl = Keyboard.current.GetChildControl<KeyControl>(
                keyCode.ToString());
        }
        //数字0-9
        else if (keyCodeV >= (int)KeyCode.Alpha0 
            && keyCodeV <= (int)KeyCode.Alpha9)
        {
            buttonCtrl = Keyboard.current.GetChildControl<KeyControl>(
                (keyCodeV - (int)KeyCode.Alpha0).ToString());
        }
        //数字小键盘0-9
        else if (keyCodeV >= (int)KeyCode.Keypad0 
            && keyCodeV <= (int)KeyCode.Keypad9)
        {
            buttonCtrl = Keyboard.current.GetChildControl<KeyControl>(
                string.Format("numpad{0}",
                    keyCodeV - (int)KeyCode.Keypad0));
        }
        //F1-F15
        else if (keyCodeV >= (int)KeyCode.F1
            && keyCodeV <= (int)KeyCode.F15)
        {
            buttonCtrl = Keyboard.current.GetChildControl<KeyControl>(
                keyCode.ToString());
        }
        else
        {
            switch (keyCode)
            {
                case KeyCode.Backspace:
                    buttonCtrl = Keyboard.current.backspaceKey; break;
                case KeyCode.Delete:
                    buttonCtrl = Keyboard.current.deleteKey; break;
                case KeyCode.Tab:
                    buttonCtrl = Keyboard.current.tabKey; break;
                case KeyCode.KeypadPeriod:
                    buttonCtrl = Keyboard.current.numpadPeriodKey; break;
                case KeyCode.KeypadDivide:
                    buttonCtrl = Keyboard.current.numpadDivideKey; break;
                case KeyCode.KeypadMultiply:
                    buttonCtrl = Keyboard.current.numpadMultiplyKey; break;
                case KeyCode.KeypadMinus:
                    buttonCtrl = Keyboard.current.numpadMinusKey; break;
                case KeyCode.KeypadPlus:
                    buttonCtrl = Keyboard.current.numpadPlusKey; break;
                case KeyCode.KeypadEnter:
                    buttonCtrl = Keyboard.current.numpadEnterKey; break;
                case KeyCode.KeypadEquals:
                    buttonCtrl = Keyboard.current.numpadEqualsKey; break;
                case KeyCode.UpArrow:
                    buttonCtrl = Keyboard.current.upArrowKey; break;
                case KeyCode.DownArrow:
                    buttonCtrl = Keyboard.current.downArrowKey; break;
                case KeyCode.RightArrow:
                    buttonCtrl = Keyboard.current.rightArrowKey; break;
                case KeyCode.LeftArrow:
                    buttonCtrl = Keyboard.current.leftArrowKey; break;
                case KeyCode.Insert:
                    buttonCtrl = Keyboard.current.insertKey; break;
                case KeyCode.Home:
                    buttonCtrl = Keyboard.current.homeKey; break;
                case KeyCode.End:
                    buttonCtrl = Keyboard.current.endKey; break;
                case KeyCode.PageUp:
                    buttonCtrl = Keyboard.current.pageUpKey; break;
                case KeyCode.PageDown:
                    buttonCtrl = Keyboard.current.pageDownKey; break;
                case KeyCode.Quote:
                    buttonCtrl = Keyboard.current.quoteKey; break;
                case KeyCode.Comma:
                    buttonCtrl = Keyboard.current.commaKey; break;
                case KeyCode.Minus:
                    buttonCtrl = Keyboard.current.minusKey; break;
                case KeyCode.Period:
                    buttonCtrl = Keyboard.current.periodKey; break;
                case KeyCode.Slash:
                    buttonCtrl = Keyboard.current.slashKey; break;
                case KeyCode.Semicolon:
                    buttonCtrl = Keyboard.current.semicolonKey; break;
                case KeyCode.BackQuote:
                    buttonCtrl = Keyboard.current.backquoteKey; break;
                case KeyCode.Numlock:
                    buttonCtrl = Keyboard.current.numLockKey; break;
                case KeyCode.CapsLock:
                    buttonCtrl = Keyboard.current.capsLockKey; break;
                case KeyCode.ScrollLock:
                    buttonCtrl = Keyboard.current.scrollLockKey; break;
                case KeyCode.LeftShift:
                    buttonCtrl = Keyboard.current.leftShiftKey; break;
                case KeyCode.RightShift:
                    buttonCtrl = Keyboard.current.rightShiftKey; break;
                case KeyCode.LeftControl:
                    buttonCtrl = Keyboard.current.leftCtrlKey; break;
                case KeyCode.RightControl:
                    buttonCtrl = Keyboard.current.rightCtrlKey; break;
                case KeyCode.LeftAlt:
                    buttonCtrl = Keyboard.current.leftAltKey; break;
                case KeyCode.RightAlt:
                    buttonCtrl = Keyboard.current.rightAltKey; break;
                case KeyCode.LeftCommand:
                    buttonCtrl = Keyboard.current.leftCommandKey; break;
                case KeyCode.RightCommand:
                    buttonCtrl = Keyboard.current.rightCommandKey; break;
                case KeyCode.LeftWindows:
                    buttonCtrl = Keyboard.current.leftWindowsKey; break;
                case KeyCode.RightWindows:
                    buttonCtrl = Keyboard.current.rightWindowsKey; break;
                case KeyCode.Print:
                    buttonCtrl = Keyboard.current.printScreenKey; break;
                default: buttonCtrl = null; break;
            }
        }
        return buttonCtrl != null 
            ? (buttonCtrl.wasPressedThisFrame ? KeyState.PRESSED 
                : buttonCtrl.isPressed ? KeyState.HOLD 
                : buttonCtrl.wasReleasedThisFrame ? KeyState.RELEASED 
                : KeyState.NONE)
            : KeyState.NONE;
#endif
    }

    /// <summary>
    /// 鼠标左键是否点击
    /// </summary>
    /// <param name="clickPosition">点击的位置</param>
    /// <returns>是否点击</returns>
    public static bool IsLeftMouseButtonClick(out Vector2 clickPosition)
    {
        bool isClick = false;
        clickPosition = Vector2.zero;
#if !ENABLE_INPUT_SYSTEM
        isClick = Input.GetMouseButtonDown(0);
#else
        isClick = Mouse.current != null 
            && Mouse.current.leftButton.wasPressedThisFrame;
#endif
        if (isClick)
        {
#if !ENABLE_INPUT_SYSTEM
            clickPosition = Input.mousePosition;
#else
            clickPosition = Mouse.current.position.ReadValue();
#endif
        }
        return isClick;
    }
}
