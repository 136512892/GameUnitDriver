using UnityEngine;

/// <summary>
/// 漫游类型相机控制
/// </summary>
public class RoamCameraController : MonoBehaviour
{
    //移动速度
    [SerializeField] 
    private float moveSpeed = 50f;
    //加速系数，Shift按下时起作用
    [SerializeField] 
    private float boostFactor = 3.5f;
    //是否反转方向，用于鼠标滚轮移动 
    [SerializeField] 
    private bool invertScrollDirection = false;
    //鼠标转动的灵敏度
    [Range(0.1f, 20f), SerializeField] 
    private float mouseMovementSensitivity = 10f;
    //反转水平转动方向
    [SerializeField] private bool invertY = false;
    //插值速度
    [SerializeField]
    private float lerpSpeed = 10f;
    //目标坐标值
    private Vector3 targetPos;
    //目标旋转值
    private Vector3 targetRot;

    private void OnEnable()
    {
        targetPos = transform.position;
        targetRot = transform.eulerAngles;
    }
    private void Update()
    {
        //按住鼠标右键拖动时旋转视角
        if (Input.GetMouseButton(1))
        {
            float x = Input.GetAxis("Mouse X");
            float y = Input.GetAxis("Mouse Y");
            targetRot += new Vector3(y * (invertY ? 1f : -1f), x, 0f)
                * mouseMovementSensitivity * Time.deltaTime * 100f;
        }
        //移动方向
        Vector3 motion = Vector3.zero;
        if (Input.GetKey(KeyCode.W)) //向前移动
            motion += Vector3.forward;
        if (Input.GetKey(KeyCode.S)) //向后移动
            motion += Vector3.back;
        if (Input.GetKey(KeyCode.A)) //向左移动
            motion += Vector3.left;
        if (Input.GetKey(KeyCode.D)) //向右移动
            motion += Vector3.right;
        if (Input.GetKey(KeyCode.Q)) //向下移动
            motion += Vector3.down;
        if (Input.GetKey(KeyCode.E)) //向上移动
            motion += Vector3.up;

        float wheelValue = Input.GetAxis("Mouse ScrollWheel");
        motion += wheelValue == 0 ? Vector3.zero
            : (wheelValue > 0 ? Vector3.forward : Vector3.back)
                * (invertScrollDirection ? -1 : 1);

        motion = motion.normalized;
        if (Input.GetKey(KeyCode.LeftShift))
            motion *= boostFactor;
        motion *= Time.deltaTime * moveSpeed;
        targetPos += Quaternion.Euler(targetRot) * motion;
        transform.position = Vector3.Lerp(transform.position, 
            targetPos, Time.deltaTime * lerpSpeed);
        transform.rotation = Quaternion.Lerp(transform.rotation, 
            Quaternion.Euler(targetRot), Time.deltaTime * lerpSpeed);
    }
}