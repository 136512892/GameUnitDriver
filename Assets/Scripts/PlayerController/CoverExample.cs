using UnityEditor;
using UnityEngine;

public class CoverExample : MonoBehaviour
{
    //角色控制器组件
    private CharacterController cc;
    //动画组件
    private Animator animator;
    //检测层级
    [SerializeField] private LayerMask coverLayerMask = 1;
    //进入&退出掩体状态的快捷键
    [SerializeField] private KeyCode shortcutKey = KeyCode.E;
    //Box检测的尺寸
    [SerializeField] private Vector3 boxCastSize = Vector3.one * .4f;
    //Box检测的最大距离
    [SerializeField] private int boxCastNum = 2;
    //进入掩体状态的速度
    [SerializeField] private float stand2CoverSpeed = 5f;
    //退出掩体状态的速度
    [SerializeField] private float cover2StandSpeed = 1f;
    //切换至掩体状态所需的时长 具体看动画时长来定
    [SerializeField] private float stand2CoverTime = 1f;
    //退出掩体状态所需的时长 具体看动画时长来定
    [SerializeField] private float cover2StandTime = 1f;
    //切换至掩体状态计时器
    private float stand2CoverTimer;
    //切换至掩体状态计时器
    private float cover2StandTimer;
    //掩体状态行走速度
    [SerializeField] private float sneakSpeed = 1f;
    //碰撞信息
    private RaycastHit hit;
    //检测结果
    private bool castResult;
    //掩体状态方向 -1：左  1：右
    private float coverDirection;
    //目标方向
    private float targetCoverDirection = 1f;
    //掩体方向的插值速度
    [SerializeField] private float directionLerpSpeed = 3f;
    //头部
    private Transform headTransform;
    //头部初始Y坐标
    private float headOriginPosY;
    //头部的半径
    [Range(.05f, .3f), SerializeField] private float headRadius = .12f;
    //初始身体Y坐标
    private float originBodyPositionY;
    //目标身体Y坐标
    private float targetBodyPositionY;
    //头部物理检测结果
    private bool headCastResult;
    //头部下方物理检测的次数限制（每次下降一个半径的单位进行检测）
    [Range(1, 5), SerializeField] private int headDownCastCountLimit = 3;
    //头部球形检测的初始点
    private Vector3 headSphereCastOrigin;

    //是否启用脚部IK
    private bool enableFootIk = true;
    //检测的层级
    [SerializeField] private LayerMask groundLayerMask = 1;
    //使用该变量来纠正脚和根级的偏移量
    private float bodyYOffset = 0.02f;

    //身体坐标插值速度
    [Range(0f, 1f), SerializeField] private float bodyPositionLerpSpeed = 0.05f;
    //脚部坐标插值速度
    [Range(0f, 1f), SerializeField] private float footPositionLerpSpeed = 0.15f;

    //射线检测的长度
    [Range(0f, 1.3f), SerializeField] private float raycastDistance = 0.8f;
    //射线检测的高度
    [Range(0f, 1.5f), SerializeField] private float raycastOriginHeight = 1f;

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
    //身体Y坐标缓存
    private float lastBodyPositionY;
    //左脚Y坐标缓存
    private float lastLeftFootPositionY;
    //右脚Y坐标缓存
    private float lastRightFootPositionY;
    //左脚射线检测结果
    private bool leftFootRaycast;
    //右脚射线检测结果
    private bool rightFootRaycast;

    public enum State
    {
        None, //未在任何状态
        Stand2Cover, //正在切换至掩体状态
        IsCovering, //正处于掩体状态
        Cover2Stand, //正在退出掩体状态
    }
    //当前状态
    private State state = State.None;

    /// <summary>
    /// 当前状态
    /// </summary>
    public State CurrentState
    {
        get
        {
            return state;
        }
    }

    /// <summary>
    /// 是否可以进入掩体状态
    /// </summary>
    protected virtual bool CoverCanable
    {
        get
        {
            return true;
        }
    }

    //Animator动画参数
    private static class AnimParam
    {
        public static readonly int Cover = Animator.StringToHash("Cover");
        public static readonly int CoverDirection = Animator.StringToHash("Cover Direction");
        public static readonly int CoverSneak = Animator.StringToHash("Cover Sneak");
    }

    private void Awake()
    {
        //获取动画组件
        animator = GetComponentInChildren<Animator>();
        //获取角色控制器组件
        cc = GetComponentInChildren<CharacterController>();
        //获取头部位置
        headTransform = animator.GetBoneTransform(HumanBodyBones.Head);
        //记录头部初始Y坐标
        headOriginPosY = headTransform.position.y;
    }

    private void FixedUpdate()
    {
        //未启用FootIK or 动画组件为空
        if (!enableFootIk || animator == null) return;

        #region 计算左脚IK
        //左脚坐标
        leftFootPosition = animator.GetBoneTransform(HumanBodyBones.LeftFoot).position;
        leftFootPosition.y = transform.position.y + raycastOriginHeight;

        //左脚 射线检测
        leftFootRaycast = Physics.Raycast(leftFootPosition, Vector3.down, out RaycastHit hit, raycastDistance + raycastOriginHeight, groundLayerMask);
        if (leftFootRaycast)
        {
            leftFootIkPosition = leftFootPosition;
            leftFootIkPosition.y = hit.point.y + bodyYOffset;
            leftFootIkRotation = Quaternion.FromToRotation(transform.up, hit.normal);
#if UNITY_EDITOR
            //射线
            //Debug.DrawLine(leftFootPosition, leftFootPosition + Vector3.down * (raycastDistance + raycastOriginHeight), Color.yellow);
            //法线
            //Debug.DrawLine(hit.point, hit.point + hit.normal * .5f, Color.cyan);
#endif
        }
        else
        {
            leftFootIkPosition = Vector3.zero;
        }
        #endregion

        #region 计算右脚IK
        //右脚坐标
        rightFootPosition = animator.GetBoneTransform(HumanBodyBones.RightFoot).position;
        rightFootPosition.y = transform.position.y + raycastOriginHeight;
        //右脚 射线检测
        rightFootRaycast = Physics.Raycast(rightFootPosition, Vector3.down, out hit, raycastDistance + raycastOriginHeight, groundLayerMask);
        if (rightFootRaycast)
        {
            rightFootIkPosition = rightFootPosition;
            rightFootIkPosition.y = hit.point.y + bodyYOffset;
            rightFootIkRotation = Quaternion.FromToRotation(transform.up, hit.normal);

#if UNITY_EDITOR
            //射线
            //Debug.DrawLine(rightFootPosition, rightFootPosition + Vector3.down * (raycastDistance + raycastOriginHeight), Color.yellow);
            //法线
            //Debug.DrawLine(hit.point, hit.point + hit.normal * .5f, Color.cyan);
#endif
        }
        else
        {
            rightFootIkPosition = Vector3.zero;
        }
        #endregion
    }
    private void Update()
    {
        //不可进入
        if (!CoverCanable) return;
        //动画组件为空
        if (animator == null) return;

        //处理不同状态的逻辑
        switch (state)
        {
            //未处于掩体状态
            case State.None:
                {
                    //Box检测的中心点
                    Vector3 boxCastCenter = transform.position + transform.up;
                    //最大检测距离
                    float maxDistance = boxCastSize.z * boxCastNum;
                    //向身体前方进行Box检测 寻找掩体 
                    castResult = Physics.BoxCast(boxCastCenter, boxCastSize * .5f, transform.forward, out hit, transform.rotation, maxDistance, coverLayerMask);
                    //调试：法线方向
                    Debug.DrawLine(hit.point, hit.point + hit.normal * hit.distance, Color.magenta);

                    //检测到掩体
                    if (castResult)
                    {
                        //按下快捷键 进入掩体状态
                        if (Input.GetKeyDown(shortcutKey))
                        {
                            //正在切换至掩体状态
                            state = State.Stand2Cover;
                            //播放动画
                            animator.SetBool(AnimParam.Cover, true);
                            //禁用其他人物控制系统
                            GetComponent<JumpExample>().enabled = false;
                            //默认右方（动画Stand2Cover默认右方）
                            targetCoverDirection = 1f;
                            //启用脚部IK
                            enableFootIk = true;
                            bodyYOffset = 0.04f;
                        }
                    }
                }
                break;
            case State.Stand2Cover:
                {
                    //计时
                    stand2CoverTimer += Time.deltaTime;
                    if (stand2CoverTimer < stand2CoverTime)
                    {
                        //向法线反方向移动 到掩体前
                        cc.Move(-hit.normal * Time.deltaTime * stand2CoverSpeed);
                        //朝向 面向法线方向
                        transform.forward = Vector3.Lerp(transform.forward, -hit.normal, Time.deltaTime * stand2CoverSpeed);

                        //头部物理检测的初始点
                        headSphereCastOrigin = transform.position + Vector3.up * headOriginPosY + transform.right * targetCoverDirection * headRadius * 2f;
                        //向前方进行球形检测（掩体状态下前方就是后脑勺的方向）
                        headCastResult = Physics.SphereCast(headSphereCastOrigin, headRadius, transform.forward, out RaycastHit headHit, coverLayerMask);
                        int i = 0;
                        if (!headCastResult)
                        {
                            for (i = 0; i < headDownCastCountLimit; i++)
                            {
                                //每次下降一个半径的单位进行检测
                                headSphereCastOrigin -= Vector3.up * headRadius;
                                headCastResult = Physics.SphereCast(headSphereCastOrigin, headRadius, transform.forward, out headHit, coverLayerMask);
                                if (headCastResult) break;
                            }
                        }
                        if (headCastResult)
                        {
                            Debug.DrawLine(headSphereCastOrigin, headHit.point, Color.green);
                            float delta = headOriginPosY - headHit.point.y;
                            targetBodyPositionY = originBodyPositionY - delta - headRadius;
                            Debug.DrawLine(headSphereCastOrigin, headSphereCastOrigin - Vector3.up * (delta + i * headRadius), Color.red);
                        }
                    }
                    else
                    {
                        //重置计时器
                        stand2CoverTimer = 0f;
                        //切换完成 进入掩体状态
                        state = State.IsCovering;
                        bodyYOffset = 0.02f;
                    }
                }
                break;
            case State.IsCovering:
                {
                    //获取水平方向输入
                    float horizontal = Input.GetAxis("Horizontal");
                    //目标方向 输入为负取-1 为正取1
                    if (horizontal != 0f)
                    {
                        targetCoverDirection = horizontal < 0f ? -1f : 1f;
                        castResult = Physics.BoxCast(transform.position + transform.up, boxCastSize * .5f, transform.forward, out hit, Quaternion.identity, boxCastSize.z * boxCastNum, coverLayerMask);
                        Debug.DrawLine(hit.point, hit.point + hit.normal, Color.magenta);
                        cc.Move(-hit.normal * sneakSpeed * Time.deltaTime);
                        transform.forward = Vector3.Lerp(transform.forward, -hit.normal, Time.deltaTime * stand2CoverSpeed);
                    }
                    //方向插值运算
                    coverDirection = Mathf.Lerp(coverDirection, targetCoverDirection, Time.deltaTime * directionLerpSpeed);
                    //动画 方向
                    animator.SetFloat(AnimParam.CoverDirection, coverDirection);
                    //动画 掩体状态行走
                    animator.SetFloat(AnimParam.CoverSneak, Mathf.Abs(horizontal));
                    //通过输入控制移动
                    cc.Move(horizontal * sneakSpeed * Time.deltaTime * transform.right);

                    //头部物理检测的初始点
                    headSphereCastOrigin = transform.position + Vector3.up * headOriginPosY + transform.right * targetCoverDirection * headRadius * 2f;
                    //向前方进行球形检测（掩体状态下前方就是后脑勺的方向）
                    headCastResult = Physics.SphereCast(headSphereCastOrigin, headRadius, transform.forward, out RaycastHit headHit, coverLayerMask);
                    int i = 0;
                    if (!headCastResult)
                    {
                        for (i = 0; i < headDownCastCountLimit; i++)
                        {
                            //每次下降一个半径的单位进行检测
                            headSphereCastOrigin -= Vector3.up * headRadius;
                            headCastResult = Physics.SphereCast(headSphereCastOrigin, headRadius, transform.forward, out headHit, coverLayerMask);
                            if (headCastResult) break;
                        }
                    }
                    if (headCastResult)
                    {
                        Debug.DrawLine(headSphereCastOrigin, headHit.point, Color.green);
                        float delta = headOriginPosY - headHit.point.y;
                        targetBodyPositionY = originBodyPositionY - delta - headRadius;
                        Debug.DrawLine(headSphereCastOrigin, headSphereCastOrigin - Vector3.up * (delta + i * headRadius), Color.red);
                    }

                    //按下快捷键 退出掩体状态
                    if (Input.GetKeyDown(shortcutKey) || !headCastResult)
                    {
                        animator.SetBool(AnimParam.Cover, false);
                        state = State.Cover2Stand;
                        enableFootIk = false;
                    }
                }
                break;
            case State.Cover2Stand:
                //计时
                cover2StandTimer += Time.deltaTime;
                cover2StandTimer = Mathf.Clamp(cover2StandTimer, 0f, cover2StandTime);
                if (cover2StandTimer < cover2StandTime)
                {
                    //后移
                    cc.Move(cover2StandSpeed * Time.deltaTime * -transform.forward);

                    targetBodyPositionY = Mathf.Lerp(targetBodyPositionY, originBodyPositionY, cover2StandTimer / cover2StandTime);
                }
                else
                {
                    //重置计时器
                    cover2StandTimer = 0f;
                    state = State.None;
                    //启用其他人物控制脚本
                    GetComponent<JumpExample>().enabled = true;
                }
                break;
            default:
                break;
        }
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (animator == null) return;
        if (originBodyPositionY == 0)
        {
            originBodyPositionY = animator.bodyPosition.y;
            targetBodyPositionY = animator.bodyPosition.y;
            return;
        }
        Vector3 bodyPosition = animator.bodyPosition;
        bodyPosition.y = Mathf.Lerp(lastBodyPositionY, targetBodyPositionY, bodyPositionLerpSpeed);
        animator.bodyPosition = bodyPosition;
        lastBodyPositionY = animator.bodyPosition.y;

        //未启用FootIK
        if (!enableFootIk) return;

        #region 应用左脚IK
        //权重
        animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1f);
        animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1f);

        Vector3 targetIkPosition = animator.GetIKPosition(AvatarIKGoal.LeftFoot);
        if (leftFootRaycast)
        {
            //转局部坐标
            targetIkPosition = transform.InverseTransformPoint(targetIkPosition);
            Vector3 world2Local = transform.InverseTransformPoint(leftFootIkPosition);
            //插值计算
            float y = Mathf.Lerp(lastLeftFootPositionY, world2Local.y, footPositionLerpSpeed);
            targetIkPosition.y += y;
            lastLeftFootPositionY = y;
            //转全局坐标
            targetIkPosition = transform.TransformPoint(targetIkPosition);
            //当前旋转
            Quaternion currRotation = animator.GetIKRotation(AvatarIKGoal.LeftFoot);
            //目标旋转
            Quaternion nextRotation = leftFootIkRotation * currRotation;
            animator.SetIKRotation(AvatarIKGoal.LeftFoot, nextRotation);
        }
        animator.SetIKPosition(AvatarIKGoal.LeftFoot, targetIkPosition);
        #endregion

        #region 应用右脚IK
        //权重
        animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1f);
        animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1f);
        targetIkPosition = animator.GetIKPosition(AvatarIKGoal.RightFoot);
        if (rightFootRaycast)
        {
            //转局部坐标
            targetIkPosition = transform.InverseTransformPoint(targetIkPosition);
            Vector3 world2Local = transform.InverseTransformPoint(rightFootIkPosition);
            //插值计算
            float y = Mathf.Lerp(lastRightFootPositionY, world2Local.y, footPositionLerpSpeed);
            targetIkPosition.y += y;
            lastRightFootPositionY = y;
            //转全局坐标
            targetIkPosition = transform.TransformPoint(targetIkPosition);
            //当前旋转
            Quaternion currRotation = animator.GetIKRotation(AvatarIKGoal.RightFoot);
            //目标旋转
            Quaternion nextRotation = rightFootIkRotation * currRotation;
            animator.SetIKRotation(AvatarIKGoal.RightFoot, nextRotation);
        }
        animator.SetIKPosition(AvatarIKGoal.RightFoot, targetIkPosition);
        #endregion
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
#if UNITY_EDITOR
        Handles.Label(hit.point, string.Format("Hit Distance：{0}", hit.distance));
#endif
        //PhysicsGizmos.OverlapBoxWire(transform.position + transform.up, boxCastSize * .5f, transform.rotation, transform.worldToLocalMatrix);
        //PhysicsGizmos.OverlapBoxWire(transform.position + transform.up + transform.forward * boxCastSize.z, boxCastSize * .5f, transform.rotation, transform.worldToLocalMatrix);
        PhysicsGizmos.BoxCastWire(transform.position + transform.up, boxCastSize * .5f, transform.rotation, transform.worldToLocalMatrix, transform.forward, boxCastSize.z * boxCastNum);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(headSphereCastOrigin, headRadius);
    }
#endif
}
