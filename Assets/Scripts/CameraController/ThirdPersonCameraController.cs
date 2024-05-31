using UnityEngine;

/// <summary>
/// 第三人称相机控制
/// </summary>
public class ThirdPersonCameraController : MonoBehaviour
{
    //人物角色
    [SerializeField]
    private Transform avatar;
    [SerializeField] private bool backOfAvatar;
    //鼠标灵敏度
    [SerializeField, Range(1f, 10f)]
    private float sensitivity = 6f;
    //旋转角度x值
    private float rotX;
    //旋转角度y值
    private float rotY;

    //垂直方向角度最小值限制
    [SerializeField, Range(-80f, -10f)]
    private float rotXMinLimit = -40f;
    //垂直方向角度最大值限制
    [SerializeField, Range(10f, 80f)]
    private float rotXMaxLimit = 70f;
    [SerializeField]
    private float rotationSpeed = 3f;
    //高度
    [SerializeField, Range(1f, 5f)]
    private float height = 2f;

    //鼠标滚轮灵敏度
    [SerializeField]
    private float scollSensitivity = 2f;
    //是否反转滚动方向
    [SerializeField]
    private bool invertScrollDirection;
    //最小距离
    [SerializeField]
    private float minDistanceLimit = 2f;
    //最大距离
    [SerializeField]
    private float maxDistanceLimit = 5f;
    //当前距离
    private float currentDistance = 2f;
    //目标距离
    private float targetDistance; 

    //相机半径
    [SerializeField] private float cameraRadius = .2f;
    //障碍物层级
    [SerializeField] private LayerMask obstacleLayer;
    //碰撞信息
    private RaycastHit hitInfo;

    private void Start()
    {
        currentDistance = Mathf.Clamp(currentDistance, 
            minDistanceLimit, maxDistanceLimit);
        targetDistance = currentDistance;
    }
    private void LateUpdate()
    {
        //鼠标右键按下时旋转视角
        if (Input.GetMouseButton(1))
        {
            float horizontal = Input.GetAxis("Mouse X")
                * Time.deltaTime * 100f * sensitivity;
            float vertical = Input.GetAxis("Mouse Y")
                * Time.deltaTime * 100f * sensitivity;
            rotY += horizontal;
            rotX -= vertical;
            rotX = Mathf.Clamp(rotX, rotXMinLimit, rotXMaxLimit);
        }
        //目标旋转值
        //Quaternion targetRotation = Quaternion.Euler(
        //    rotX, avatar.eulerAngles.y, 0f); //y值取人物角色的欧拉角y值
        Quaternion targetRotation = Quaternion.Euler(
            rotX, backOfAvatar ? avatar.eulerAngles.y : rotY, 0f);
        targetRotation = Quaternion.Lerp(transform.rotation,
            targetRotation, Time.deltaTime * rotationSpeed);

        //鼠标滚轮滚动时改变距离
        currentDistance -= Input.GetAxis("Mouse ScrollWheel")
            * Time.deltaTime * 100f * scollSensitivity
            * (invertScrollDirection ? -1f : 1f);
        //距离钳制
        currentDistance = Mathf.Clamp(currentDistance,
            minDistanceLimit, maxDistanceLimit);
        //插值
        targetDistance = Mathf.Lerp(targetDistance,
            currentDistance, Time.deltaTime);

        Vector3 targetPosition = targetRotation * Vector3.back
            * targetDistance + avatar.position + Vector3.up * height;
        //避障
        targetPosition = ObstacleAvoidance(
            targetPosition,
            avatar.position + Vector3.up * height,
            cameraRadius,
            currentDistance,
            obstacleLayer);
        //赋值
        transform.rotation = targetRotation;
        transform.position = targetPosition;
    }

    //避障
    private Vector3 ObstacleAvoidance(Vector3 current, 
        Vector3 target, float radius, float maxDistance, LayerMask layerMask)
    {
        Ray ray = new Ray(target, current - target);
        if (Physics.SphereCast(ray, radius, 
            out hitInfo, maxDistance, layerMask))
        {
            return ray.GetPoint(hitInfo.distance - radius * 2f);
        }
        return current;
    }

//    private void OnDrawGizmos()
//    {
//        Vector3 current = transform.position;
//        Vector3 target = avatar.position + Vector3.up * height;
//        Vector3 final = ObstacleAvoidance(current, target,
//            cameraRadius, distance, obstacleLayer);
//        Gizmos.DrawLine(current, target);
//        Gizmos.DrawWireSphere(current + Vector3.up * .1f, .1f);
//        Gizmos.DrawWireSphere(target + Vector3.up * .1f, .1f);
//        Gizmos.color = Color.yellow;
//        Gizmos.DrawWireSphere(hitInfo.point + Vector3.up * .1f, .1f);
//        Gizmos.color = Color.cyan;
//        Gizmos.DrawWireSphere(final + Vector3.up * .1f, .1f);
//#if UNITY_EDITOR
//        UnityEditor.Handles.Label(target, "球形投射检测起点");
//        UnityEditor.Handles.Label(current, "球形投射检测终点");
//        UnityEditor.Handles.Label(hitInfo.point, "碰撞点");
//        UnityEditor.Handles.Label(final, "避障后的点");
//#endif
//    }
}