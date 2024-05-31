using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

[RequireComponent(typeof(CharacterController))]
public class SitExample : MonoBehaviour
{
    #region >> Movement
    private CharacterController cc;
    private Camera mainCamera;

    private Vector3 moveDirection;
    private float moveSpeed;
    [SerializeField] private float walkSpeed = 2f;
    [SerializeField] private float runSpeed = 5.35f;
    [SerializeField] private float rotateSpeed = 10f;
    #endregion

    #region >> Ground
    [SerializeField] private float groundCheckOffset = .03f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float gravity = -9.81f;

    private bool isGrounded;
    private RaycastHit hitInfo;
    private Vector3 groundNormal;
    private float verticalVelocity;
    private Vector3 slopeVelocity;
    #endregion

    #region >> Jump
    //跳跃动力值，值越大跳跃高度越高
    [SerializeField] private float jumpPower = 1.2f;
    //跳跃冷却时长
    [SerializeField] private float jumpCD = 1f;
    //在空中经过该时长进入下降状态
    [SerializeField] private float fallTime = .2f;
    //跳跃和下降的计时器
    private float jumpTimer, fallTimer;
    #endregion

    #region >> Animation
    private Animator animator;

    private class AnimParam
    {
        public static readonly int Speed =
            Animator.StringToHash("Speed");
        public static readonly int Jump =
            Animator.StringToHash("Jump");
        public static readonly int Fall =
            Animator.StringToHash("Fall");
        public static readonly int Land =
            Animator.StringToHash("Land");
    }
    #endregion

    #region >> PathFinding
    [SerializeField] private NavMeshAgent agent;
    private bool isPathFinding;
    private UnityEvent onPathFindingCompleted;
    #endregion

    #region >> MonoBehaviour
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
        if (!isPathFinding)
            Movement();

        PathFinding();
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
        if (!isGrounded)
            UnityEditor.Handles.Label(transform.position + Vector3.right * .8f,
            string.Format("VerticalVel：{0}  PosY：{1}", verticalVelocity, transform.position.y));
#endif
    }
    #endregion

    #region >> NonPublic Methods
    private void Movement()
    {
        //获取用户输入
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        //目标移动速度
        float targetMoveSpeed = horizontal == 0f && vertical == 0f ? 0f
            : Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
        moveSpeed = Mathf.Lerp(moveSpeed, targetMoveSpeed, Time.deltaTime * 10f);
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
        animator.SetFloat(AnimParam.Speed, moveSpeed);
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
        {
            animator.SetBool(AnimParam.Land, true);
            fallTimer = fallTime;

            verticalVelocity = Mathf.Lerp(
                gravity, -2f, Vector3.Dot(Vector3.up, groundNormal));

            animator.SetBool(AnimParam.Jump, false);
            animator.SetBool(AnimParam.Fall, false); //下降动画

            //触发跳跃
            if (Input.GetKeyDown(KeyCode.Space) && jumpTimer <= 0f)
            {
                jumpTimer = jumpCD;  //重置跳跃即时
                verticalVelocity = Mathf.Sqrt(jumpPower * -2f * gravity);
                animator.SetBool(AnimParam.Jump, true); //起跳动画
            }
            if (jumpTimer >= 0f)
                jumpTimer -= Time.deltaTime;
        }
        else
        {
            //下降
            if (fallTimer > 0f)
                fallTimer -= Time.deltaTime;
            else
                animator.SetBool(AnimParam.Fall, true);

            verticalVelocity += gravity * Time.deltaTime;
            //落地动画
            animator.SetBool(AnimParam.Land, false);
        }
    }
    //寻路
    private void PathFinding()
    {
        if (isGrounded && Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition),
                out RaycastHit hitInfo))
            {
                if (hitInfo.collider.CompareTag("Ground"))
                {
                    isPathFinding = true;
                    agent.enabled = true;
                    agent.SetDestination(hitInfo.point);
                }
            }
        }

        if (isPathFinding)
        {
            if (Vector3.Distance(transform.position, agent.destination) >= .1f)
            {
                agent.speed = walkSpeed;
                animator.SetFloat(AnimParam.Speed, agent.speed);
                transform.rotation = Quaternion.Lerp(transform.rotation,
                    Quaternion.LookRotation(agent.destination - transform.position),
                    Time.deltaTime * 5f);
            }
            else
            {
                isPathFinding = false;
                agent.enabled = false;
                moveSpeed = walkSpeed;
                onPathFindingCompleted?.Invoke();
            }
        }
    }
    #endregion

    public void SetPathFindingCompletedEvent(UnityAction action)
    {
        onPathFindingCompleted.RemoveAllListeners();
        onPathFindingCompleted.AddListener(action);
    }
}