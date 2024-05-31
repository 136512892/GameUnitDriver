using UnityEngine;

/// <summary>
/// 第一人称相机控制
/// </summary>
public class FirstPersonCameraController : MonoBehaviour
{
    //初始化时隐藏鼠标光标
    [SerializeField] private bool hideCursorOnStart = true;

    //灵敏度
    [SerializeField, Range(1f, 10f)]
    private float sensitivity = 3f;
    //垂直方向角度最小值限制
    [SerializeField, Range(-80f, -10f)]
    private float rotXMinLimit = -40f;
    //垂直方向角度最大值限制
    [SerializeField, Range(10f, 80f)]
    private float rotXMaxLimit = 70f;

    private float rotX, rotY;

    private void Start()
    {
        if (hideCursorOnStart)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    private void Update()
    {
        float horizontal = Input.GetAxis("Mouse X") 
            * Time.deltaTime * 100f * sensitivity;
        float vertical = Input.GetAxis("Mouse Y") 
            * Time.deltaTime * 100f * sensitivity;
        rotY += horizontal;
        rotX -= vertical;
        rotX = Mathf.Clamp(rotX, rotXMinLimit, rotXMaxLimit);
        Quaternion targetRotation = Quaternion.Euler(rotX, rotY, 0f);
        transform.rotation = targetRotation;
    }
}