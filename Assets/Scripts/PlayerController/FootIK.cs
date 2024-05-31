using UnityEngine;

public class FootIK : MonoBehaviour
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

    #region >> Foot IK
    //是否启用
    [SerializeField] private bool enableFootIk = true;

    //身体坐标插值速度
    [Range(0f, 1f), SerializeField]
    private float bodyPositionLerpSpeed = 1f;
    //脚部坐标插值速度
    [Range(0f, 1f), SerializeField]
    private float footPositionLerpSpeed = 0.5f;

    //射线检测的长度
    [SerializeField] private float raycastDistance = 0.81f;
    //射线检测的高度
    [SerializeField] private float raycastOriginHeight = 0.5f;

    //左脚坐标
    private Vector3 leftFootPosition;
    //右脚坐标
    private Vector3 rightFootPosition;
    //左脚IK坐标
    private Vector3 leftFootIkPosition;
    //右脚IK坐标
    private Vector3 rightFootIkPosition;
    //左脚IK旋转
    private Quaternion leftFootIkRotation;
    //右脚IK旋转
    private Quaternion rightFootIkRotation;
    //左脚射线检测结果
    private bool leftFootRaycast;
    //右脚射线检测结果
    private bool rightFootRaycast;

    //左脚Y坐标缓存
    private float lastLeftFootPositionY;
    //右脚Y坐标缓存
    private float lastRightFootPositionY;
    #endregion

    #region >> MonoBehaviour
    private void Start()
    {
        cc = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        mainCamera = Camera.main != null
            ? Camera.main
            : FindObjectOfType<Camera>();
    }

    private void FixedUpdate()
    {
        //未启用FootIK or 动画组件为空
        if (!enableFootIk || animator == null) return;

        #region 计算左脚IK目标
        //左脚坐标
        leftFootPosition = animator.GetBoneTransform(
            HumanBodyBones.LeftFoot).position;
        leftFootPosition.y = transform.position.y + raycastOriginHeight;
        //左脚射线投射检测
        leftFootRaycast = Physics.Raycast(leftFootPosition,
            Vector3.down, out RaycastHit hit, raycastDistance, groundLayer);
        if (leftFootRaycast)
        {
            leftFootIkPosition.y = hit.point.y;
            leftFootIkRotation = Quaternion.FromToRotation(
                transform.up, hit.normal);
#if UNITY_EDITOR
            Debug.DrawLine(leftFootPosition, leftFootPosition
                + Vector3.down * raycastDistance, Color.yellow); //射线
            Debug.DrawLine(hit.point, hit.point + hit.normal * .5f,
                Color.cyan); //法线
#endif
        }
        else leftFootIkPosition = Vector3.zero;
        #endregion

        #region 计算右脚IK目标
        //右脚坐标
        rightFootPosition = animator.GetBoneTransform(
            HumanBodyBones.RightFoot).position;
        rightFootPosition.y = transform.position.y + raycastOriginHeight;
        //右脚射线投射检测
        rightFootRaycast = Physics.Raycast(rightFootPosition,
            Vector3.down, out hit, raycastDistance, groundLayer);
        if (rightFootRaycast)
        {
            rightFootIkPosition.y = hit.point.y;
            rightFootIkRotation = Quaternion.FromToRotation(
                transform.up, hit.normal);
#if UNITY_EDITOR
            Debug.DrawLine(rightFootPosition, rightFootPosition
                + Vector3.down * raycastDistance, Color.yellow); //射线
            Debug.DrawLine(hit.point, hit.point + hit.normal * .5f,
                Color.cyan); //法线
#endif
        }
        else rightFootIkPosition = Vector3.zero;
        #endregion
    }

    private void Update()
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

    private void OnAnimatorIK(int layerIndex)
    {
        //未启用FootIK or 动画组件为空
        if (!enableFootIk || animator == null) return;

        #region 身体
        if (leftFootRaycast && rightFootRaycast)
        {
            //左脚坐标Y差值
            float leftPosYDelta = leftFootIkPosition.y - transform.position.y;
            //右脚坐标Y差值
            float rightPosYDelta = rightFootIkPosition.y - transform.position.y;
            //身体坐标Y差值取二者最小值
            float bodyPosYDelta = Mathf.Min(leftPosYDelta, rightPosYDelta);
            //目标身体坐标
            Vector3 targetBodyPosition = animator.bodyPosition
                + Vector3.up * bodyPosYDelta;
            //插值运算
            targetBodyPosition.y = Mathf.Lerp(animator.bodyPosition.y,
                targetBodyPosition.y, bodyPositionLerpSpeed);
            //设置身体坐标
            animator.bodyPosition = targetBodyPosition;
        }
        #endregion

        #region 应用左脚IK目标
        animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1f);
        animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1f);
        Vector3 targetIkPosition = animator.GetIKPosition(
            AvatarIKGoal.LeftFoot);
        if (leftFootRaycast)
        {
            Vector3 world2Local = transform.InverseTransformPoint(
                leftFootIkPosition);
            float y = Mathf.Lerp(lastLeftFootPositionY,
                world2Local.y, footPositionLerpSpeed);
            targetIkPosition = transform.InverseTransformPoint(
                targetIkPosition);
            targetIkPosition.y += y;
            lastLeftFootPositionY = y;
            targetIkPosition = transform.TransformPoint(targetIkPosition);
            Quaternion currRotation = animator.GetIKRotation(
                AvatarIKGoal.LeftFoot);
            Quaternion nextRotation = leftFootIkRotation * currRotation;
            animator.SetIKRotation(AvatarIKGoal.LeftFoot, nextRotation);
        }
        animator.SetIKPosition(AvatarIKGoal.LeftFoot, targetIkPosition);
        #endregion

        #region 应用右脚IK目标
        animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1f);
        animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1f);
        targetIkPosition = animator.GetIKPosition(
            AvatarIKGoal.RightFoot);
        if (rightFootRaycast)
        {
            Vector3 world2Local = transform.InverseTransformPoint(
                rightFootIkPosition);
            float y = Mathf.Lerp(lastRightFootPositionY,
                world2Local.y, footPositionLerpSpeed);
            targetIkPosition = transform.InverseTransformPoint(
                targetIkPosition);
            targetIkPosition.y += y;
            lastRightFootPositionY = y;
            targetIkPosition = transform.TransformPoint(targetIkPosition);
            Quaternion currRotation = animator.GetIKRotation(
                AvatarIKGoal.RightFoot);
            Quaternion nextRotation = rightFootIkRotation * currRotation;
            animator.SetIKRotation(AvatarIKGoal.RightFoot, nextRotation);
        }
        animator.SetIKPosition(AvatarIKGoal.RightFoot, targetIkPosition);
        #endregion
    }
    #endregion

    #region >> NonPublic Methods
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
                jumpTimer = jumpCD;  //重置跳跃计时
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
    #endregion
}