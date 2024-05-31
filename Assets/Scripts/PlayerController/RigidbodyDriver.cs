using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RigidbodyDriver : MonoBehaviour
{
    private Rigidbody rb;
    private Camera mainCamera;
    private Animator animator;
    private Vector3 moveDirection;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float rotateSpeed = 10f;
    [SerializeField] private LayerMask groundLayer;
    private RaycastHit hitInfo;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        mainCamera = Camera.main != null 
            ? Camera.main
            : FindObjectOfType<Camera>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        //获取用户输入
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        //根据相机的朝向获取移动方向
        moveDirection = (Vector3.ProjectOnPlane(
            mainCamera.transform.forward, Vector3.up) * vertical
            + Vector3.ProjectOnPlane(mainCamera.transform.right,
            Vector3.up) * horizontal).normalized;
        //坡面检测
        Ray ray = new Ray(transform.position + Vector3.up * .2f, Vector3.down);
        if (Physics.Raycast(ray, out hitInfo, .3f, groundLayer,
            QueryTriggerInteraction.Ignore))
        {
            //坡面的坡度和法线方向与世界空间上方向向量之间的角度相等
            float slopeAngle = Vector3.Angle(hitInfo.normal, Vector3.up);
            //在坡面上
            if (slopeAngle > 0)
            {
                //将移动方向投射到坡面上
                moveDirection = Vector3.ProjectOnPlane(
                    moveDirection, hitInfo.normal).normalized;
            }
        }
        //移动
        rb.MovePosition(transform.position 
            + Time.deltaTime * moveSpeed * moveDirection);
        //当前方向与目标方向的角度差
        float angle = Vector3.SignedAngle(
            transform.forward, moveDirection, Vector3.up);
        //旋转
        rb.MoveRotation(transform.rotation 
            * Quaternion.Euler(0f, angle 
                * Time.deltaTime * rotateSpeed, 0f));
        //设置动画
        animator.SetBool("Move", moveDirection != Vector3.zero);
    }

    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        if (mainCamera == null) return;
        Vector3 pos = mainCamera.transform.position;
        pos.y = 0f;
        Vector3 fpop = Vector3.ProjectOnPlane(
            mainCamera.transform.forward, Vector3.up);
        Vector3 rpop = Vector3.ProjectOnPlane(
            mainCamera.transform.right, Vector3.up);
        UnityEditor.Handles.DrawLine(pos, pos + fpop, 3f);
        UnityEditor.Handles.DrawLine(pos, pos + rpop, 3f);
        GUIStyle style = new GUIStyle(GUI.skin.label)
        {
            fontSize = 20,
            fontStyle = FontStyle.Bold
        };
        UnityEditor.Handles.Label(pos + fpop, "前方", style);
        UnityEditor.Handles.Label(pos + rpop, "右方", style);
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + moveDirection);
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(hitInfo.point, hitInfo.point + hitInfo.normal);
        UnityEditor.Handles.Label(transform.position + moveDirection, "移动方向");
        UnityEditor.Handles.Label(hitInfo.point + hitInfo.normal, "法线方向");
#endif
    }
}