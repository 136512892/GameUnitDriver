using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 观察者视角相机控制
/// </summary>
public class GodlikeCameraController : MonoBehaviour
{
    //移动速度
    [SerializeField]
    private float moveSpeed = 50f;
    //加速系数，Shift按下时起作用
    [SerializeField]
    private float boostFactor = 3.5f;
    //用于鼠标滚轮移动 是否反转方向
    [SerializeField]
    private bool invertScrollDirection = false;
    //鼠标转动的灵敏度
    [Range(0.1f, 20f), SerializeField]
    private float mouseMovementSensitivity = 10f;
    //插值速度
    [SerializeField]
    private float lerpSpeed = 10f;

    //目标坐标值
    private Vector3 targetPos;
    //目标角度值
    private Vector3 targetRot;

    private Vector3 center = Vector3.zero;
    //用于记录上一帧的鼠标位置
    private Vector2 lastMousePosition;

    //是否启用鼠标光标处于屏幕边缘时向外移动
    [SerializeField]
    private bool enableScreenEdgeMove;
    //该值定义屏幕边缘区域
    [SerializeField]
    private float screenEdgeDefine = 2f;

    private void OnEnable()
    {
        targetPos = transform.position;
        targetRot = transform.eulerAngles;
        lastMousePosition = Input.mousePosition;
    }

    private void Update()
    {
        //鼠标右键开始被按下时计算视角旋转围绕的中心点
        if (Input.GetMouseButtonDown(1))
        {
            //相机前方与世界空间中正下方向量的余弦值
            float cos = Vector3.Dot(transform.forward, Vector3.down);
            //根据余弦值和相机坐标y值求得从相机位置沿相机前方到地平面的距离
            float distance = transform.position.y / cos;
            distance = distance < 0f ? 0f : distance;
            center = transform.position + transform.forward * distance;
            center = !float.IsNaN(center.magnitude) ? center : Vector3.zero;
        }
        else if (Input.GetMouseButton(1))
        {
            //当前帧与上一帧鼠标位置发生的偏移量
            Vector3 mousePosDelta = new Vector2(
                Input.mousePosition.x - lastMousePosition.x,
                Input.mousePosition.y - lastMousePosition.y);
            //鼠标右键按下拖动时绕交点旋转
            transform.RotateAround(center, Vector3.up, mousePosDelta.x
                * Time.deltaTime * mouseMovementSensitivity);
            transform.RotateAround(center, transform.right, -mousePosDelta.y
                * Time.deltaTime * mouseMovementSensitivity);
            targetPos = transform.position;
            targetRot = transform.eulerAngles;
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
        
        //鼠标左键是否按住，鼠标位置是否处于窗口内
        if (Input.GetMouseButton(0)  
            && Screen.safeArea.Contains(Input.mousePosition))
        {
            //当前帧的鼠标位置减去上一帧的鼠标位置得到偏移量
            Vector3 mousePosDelta = new Vector2(
                Input.mousePosition.x - lastMousePosition.x,
                Input.mousePosition.y - lastMousePosition.y);
            motion += Vector3.left * mousePosDelta.x 
                + Vector3.back * mousePosDelta.y;
        }

        float wheelValue = Input.GetAxis("Mouse ScrollWheel");
        motion += wheelValue == 0 ? Vector3.zero
            : ((wheelValue > 0 ? Vector3.forward : Vector3.back)
                * (invertScrollDirection ? -1f : 1f));

        if (enableScreenEdgeMove)
        {
            bool isMouseOnHorizontalScreenEdge =
                Input.mousePosition.x <= screenEdgeDefine ||
                Input.mousePosition.x >= Screen.width - screenEdgeDefine;
            bool isMouseOnVerticalScreenEdge =
                Input.mousePosition.y <= screenEdgeDefine ||
                Input.mousePosition.y >= Screen.height - screenEdgeDefine;
            if (isMouseOnHorizontalScreenEdge)
                motion += Input.mousePosition.x <= screenEdgeDefine
                    ? Vector3.left : Vector3.right;
            if (isMouseOnVerticalScreenEdge)
                motion += Input.mousePosition.y <= screenEdgeDefine
                    ? Vector3.back : Vector3.forward;
        }

        //归一化
        motion = motion.normalized;
        //按住左Shift键时移动加速
        if (Input.GetKey(KeyCode.LeftShift))
            motion *= boostFactor;
        motion *= Time.deltaTime * moveSpeed;
        targetPos += (wheelValue != 0f ? Quaternion.Euler(targetRot) 
            : Quaternion.Euler(0f, targetRot.y, targetRot.z)) * motion;
        transform.position = Vector3.Lerp(transform.position,
            targetPos, Time.deltaTime * lerpSpeed);
        transform.rotation = Quaternion.Lerp(transform.rotation,
            Quaternion.Euler(targetRot), Time.deltaTime * lerpSpeed);

        //记录鼠标位置
        lastMousePosition = Input.mousePosition;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(center, .2f);
#if UNITY_EDITOR
        Vector3 b = transform.position + Vector3.down * transform.position.y;
        Handles.DrawDottedLine(transform.position, b, 6f);
        Handles.DrawDottedLine(transform.position, center, 6f);
        GUIStyle style = new GUIStyle(GUI.skin.label)
        {
            fontSize = 25,
            fontStyle = FontStyle.Bold
        };
        Handles.Label(center, string.Format("center {0}", center), style);
        Handles.Label(transform.position + (center - b).normalized * .5f, "a", style);
        Handles.Label((transform.position + center) * .5f, "distance", style);
        Handles.Label((transform.position + b) * .5f, "y", style);
        Handles.Label((transform.position + b) * .5f
            + (b - center).normalized * 1.5f, "distance = y / Cos(a)", style);
#endif
    }
}