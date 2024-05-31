//第1章/InputManagerExample.cs

using UnityEngine;

public class InputManagerExample : MonoBehaviour
{
    private void Update()
    {
        //GetMouseButtonInputExample();
        //GetKeyCodeInputExample();
        //GetAxisInputExample();
        //GetButtonInputExample();
        GetTouchInputExample();
    }
    private void GetTouchInputExample()
    {
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            Debug.Log(string.Format(
                "单点触摸 触摸坐标{0}", touch.position));
        }
        else if (Input.touchCount >= 2)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                Debug.Log(string.Format(
                    "多点触摸 [{0}] {1}", i, touch.phase));
            }
        }
    }
    private void GetButtonInputExample()
    {
        if (Input.GetButtonDown("Jump"))
            Debug.Log("Jump按钮开始被按下");
        if (Input.GetButton("Jump"))
            Debug.Log("Jump按钮持续被按下");
        if (Input.GetButtonUp("Jump"))
            Debug.Log("Jump按钮被抬起");
    }
    private void GetAxisInputExample()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Debug.Log(string.Format("[{0},{1}]", horizontal, vertical));
    }
    private void GetKeyCodeInputExample()
    {
        if (Input.GetKeyDown(KeyCode.A))
            Debug.Log("键盘A键开始被按下");
        if (Input.GetKey(KeyCode.A))
            Debug.Log("键盘A键持续被按下");
        if (Input.GetKeyUp(KeyCode.A))
            Debug.Log("键盘A键被抬起");
    }
    private void GetMouseButtonInputExample()
    {
        if (Input.GetMouseButtonDown(0))
            Debug.Log("鼠标左键开始被按下");
        if (Input.GetMouseButton(0))
            Debug.Log("鼠标左键持续被按下");
        if (Input.GetMouseButtonUp(0))
            Debug.Log("鼠标左键被抬起");
    }

    private bool IsClickExample(out Vector3 clickPosition)
    {
        bool isClick = false;
        clickPosition = Vector3.zero;
#if UNITY_STANDALONE
        isClick = Input.GetMouseButtonDown(0);
#elif UNITY_ANDROID
        isClick = Input.touchCount == 1;
#endif
        if (isClick)
        {
#if UNITY_STANDALONE
            clickPosition = Input.mousePosition;
#elif UNITY_ANDROID
            clickPosition = Input.GetTouch(0).position;
#endif
        }
        return isClick;
    }
}