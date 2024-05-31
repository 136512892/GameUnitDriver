using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class CharacterControllerDriver : MonoBehaviour
{
    private CharacterController cc;
    private Camera mainCamera;
    private Animator animator;
    private Vector3 moveDirection;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float rotateSpeed = 10f;
    [SerializeField] private LayerMask groundLayer;
    private RaycastHit hitInfo;

    private bool isGrounded;
    [SerializeField] private float groundCheckOffset = .03f;
    private Vector3 groundNormal;
    [SerializeField] private float gravity = -9.81f;
    private float verticalVelocity;
    private Vector3 slopeVelocity;

    private void Start()
    {
        cc = GetComponent<CharacterController>();
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
        GroundCheck();
        GravityApply();
        //移动
        cc.Move(Time.deltaTime * moveSpeed * moveDirection
            + Time.deltaTime * verticalVelocity * Vector3.up
            + Time.deltaTime * slopeVelocity);
        //当前方向与目标方向的角度差
        float angle = Vector3.SignedAngle(
            transform.forward, moveDirection, Vector3.up);
        //旋转
        transform.rotation *= Quaternion.Euler(0f, angle
                * Time.deltaTime * rotateSpeed, 0f);
        //设置动画
        animator.SetBool("Move", moveDirection != Vector3.zero);
    }

    //地面检测
    private void GroundCheck()
    {
        Vector3 slopeVelocity = Vector3.zero;
        float radius = cc.radius;
        Vector3 origin = transform.position 
            + (radius + groundCheckOffset) * Vector3.up;
        if (Physics.SphereCast(origin, radius, Vector3.down, 
            out RaycastHit hitInfo, groundCheckOffset * 2f, 
            groundLayer, QueryTriggerInteraction.Ignore))
        {
            //isGrounded = true;
            groundNormal = hitInfo.normal;
            float angle = Vector3.Angle(hitInfo.normal, Vector3.up);
            isGrounded = angle <= cc.slopeLimit;
            if (!isGrounded)
            {
                Vector3 slopeRight = Vector3.Cross(
                    hitInfo.normal, Vector3.up).normalized;
                slopeVelocity = Vector3.Cross(
                    hitInfo.normal, slopeRight).normalized;
                Debug.DrawLine(hitInfo.point, hitInfo.point + hitInfo.normal);
                Debug.DrawLine(hitInfo.point, hitInfo.point + Vector3.up);
                Debug.DrawLine(hitInfo.point, hitInfo.point + slopeRight);
                Debug.DrawLine(hitInfo.point, hitInfo.point + slopeVelocity);
            }
        }
        else isGrounded = false;

        if (slopeVelocity.magnitude > .001f)
            this.slopeVelocity += Time.deltaTime
                * Mathf.Abs(gravity) * slopeVelocity;
        else
            this.slopeVelocity = Vector3.Lerp(this.slopeVelocity,
                Vector3.zero, Time.deltaTime * 20f);
    }

    //重力应用
    private void GravityApply()
    {
        if (isGrounded)
            verticalVelocity = Mathf.Lerp(
                gravity, -2f, Vector3.Dot(Vector3.up, groundNormal));
        else
            verticalVelocity += gravity * Time.deltaTime;
    }

    private void OnDrawGizmos()
    {
        if (mainCamera == null) return;
        Vector3 pos = mainCamera.transform.position;
        pos.y = 0f;
        Vector3 fpop = Vector3.ProjectOnPlane(
            mainCamera.transform.forward, Vector3.up);
        Vector3 rpop = Vector3.ProjectOnPlane(
            mainCamera.transform.right, Vector3.up);
        Gizmos.DrawLine(pos, pos + fpop);
        Gizmos.DrawLine(pos, pos + rpop);
#if UNITY_EDITOR
        UnityEditor.Handles.Label(pos + fpop, "前方");
        UnityEditor.Handles.Label(pos + rpop, "右方");
#endif
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + moveDirection);
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(hitInfo.point, hitInfo.point + hitInfo.normal);
#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + moveDirection, "移动方向");
        UnityEditor.Handles.Label(hitInfo.point + hitInfo.normal, "坡面法线方向");
#endif
        float radius = cc.radius;
        Vector3 origin = transform.position + (radius + groundCheckOffset) * Vector3.up;
        Vector3 end = transform.position + groundCheckOffset * 2f * Vector3.down;
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(origin, .02f);
        Gizmos.DrawSphere(end, .02f);
        Gizmos.DrawLine(origin, end);
#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.right * .3f,
            string.Format("处于{0}", isGrounded ? "地面" : "空中"));
#endif
    }
}