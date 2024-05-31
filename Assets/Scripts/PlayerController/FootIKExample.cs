using UnityEngine;

public class FootIKExample : MonoBehaviour
{
    #region >> Movement
    private CharacterController cc;
    private Vector3 moveDirection;
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
            leftFootIkPosition = hit.point;
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
            rightFootIkPosition = hit.point;
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
        cc.Move(Time.deltaTime * verticalVelocity * Vector3.up
            + Time.deltaTime * slopeVelocity);
    }

    private void OnAnimatorIK(int layerIndex)
    {
        //未启用FootIK或者动画组件为空
        if (!enableFootIk || animator == null) return;

        //身体高度
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
        //应用左脚IK目标
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
        //应用右脚IK目标
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
            verticalVelocity = Mathf.Lerp(
                gravity, -2f, Vector3.Dot(Vector3.up, groundNormal));
            animator.SetBool(AnimParam.Jump, false);
            animator.SetBool(AnimParam.Fall, false);
        }
        else
        {
            animator.SetBool(AnimParam.Fall, true);
            verticalVelocity += gravity * Time.deltaTime;
            animator.SetBool(AnimParam.Land, false);
        }
    }
    #endregion
}