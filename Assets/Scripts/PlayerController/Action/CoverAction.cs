using UnityEngine;

public class CoverAction : AvatarAction
{
    public CoverAction(AvatarController controller,
        AvatarActionSettings settings) : base(controller, settings) 
    {
        cc = controller.GetComponent<CharacterController>();
        CoverActionSettings s = settings as CoverActionSettings;
        boxCastSize = s.boxCastSize;
        sneakSpeed = s.sneakSpeed;
        dirLerpSpeed = s.directionLerpSpeed;
        headRadius = s.headRadius;
        headDownCastCountLimit = s.headDownCastCountLimit;
        bodyPositionLerpSpeed = s.bodyPositionLerpSpeed;
        footPositionLerpSpeed = s.footPositionLerpSpeed;
        headOriginPosY = controller.animator
            .GetBoneTransform(HumanBodyBones.Head).position.y;
    }

    public enum State { StandToCover, Covering, CoverToStand };
    private State state = State.StandToCover;
    private readonly Vector3 boxCastSize;
    private readonly CharacterController cc;
    private readonly float sneakSpeed;
    private RaycastHit headHitInfo;
    private float coverDirection;
    private float hInputSign = 1f;
    private readonly float dirLerpSpeed;
    private readonly float headOriginPosY;
    private readonly float headRadius;
    private float originBodyPositionY;
    private float targetBodyPositionY;
    private readonly int headDownCastCountLimit;
    private Vector3 headSphereCastOrigin;
    private readonly float bodyPositionLerpSpeed;
    private readonly float footPositionLerpSpeed;
    private Vector3 leftFootIkPosition;
    private Vector3 rightFootIkPosition;
    private Quaternion leftFootIkRotation;
    private Quaternion rightFootIkRotation;
    private float lastBodyPositionY;
    private float lastLeftFootPositionY;
    private float lastRightFootPositionY;

    public override bool ActionDoableCheck()
    {
        if (!controller.IsGrounded) return false;
        Vector3 origin = controller.transform.position
            + raycastHeight * Vector3.up;
        if (Physics.BoxCast(origin, boxCastSize * .5f, 
            controller.transform.forward, out RaycastHit hitInfo,
            controller.transform.rotation, raycastDistance))
        {
            if (!hitInfo.collider.CompareTag(targetTag)) return false;
            float dot = Vector3.Dot(-hitInfo.normal, 
                controller.transform.forward);
            float angle = Mathf.Acos(dot) * Mathf.Rad2Deg;
            if (angle > 45f) return false;

            origin = hitInfo.point + hitInfo.normal * .3f;
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hitInfo2,
                raycastHeight + boxCastSize.y * .5f + .01f,
                1 << LayerMask.NameToLayer("Ground")))
            {
                timer = 0f;
                state = State.StandToCover; //进入掩体状态
                startPosition = controller.transform.position;
                startRotation = controller.transform.rotation;
                targetPosition = hitInfo2.point;
                targetRotation = Quaternion.LookRotation(-hitInfo.normal);
                controller.EnableCollider(false);
                controller.animator.SetBool("Cover", true);
                originBodyPositionY = 0f;
                return true;
            }
        }
        targetPosition = Vector3.zero;
        return false;
    }

    public override bool ActionExecute()
    {
        switch(state)
        {
            case State.StandToCover:
                timer += Time.deltaTime;
                if (timer > actionDuration)
                    timer = actionDuration;
                float t = timer / actionDuration;
                controller.transform.SetPositionAndRotation(
                    Vector3.Lerp(startPosition, targetPosition, t),
                    Quaternion.Lerp(startRotation, targetRotation, t));
                if (t == 1)
                    state = State.Covering;
                break;
            case State.Covering:
                controller.EnableCollider(true);
                /***************************************************************
                 * 根据水平方向的输入左右移动
                 * 在移动的过程中向身体后方进行射线投射检测
                 * 目的是获取碰撞的法线方向，使角色移动时始终背靠掩体对象
                 ***************************************************************/
                float horizontal = Input.GetAxis("Horizontal");
                if (horizontal != 0f)
                {
                    hInputSign = horizontal > 0f ? 1f : -1f;
                    Physics.Raycast(controller.transform.position + Vector3.up 
                        * raycastHeight, controller.transform.forward, 
                        out RaycastHit hit, 0.5f);
                    Debug.DrawLine(hit.point, hit.point + hit.normal, 
                        Color.magenta);
                    cc.Move(-hit.normal * sneakSpeed * Time.deltaTime);
                    controller.transform.rotation = Quaternion.Lerp(
                        controller.transform.rotation,
                        Quaternion.LookRotation(-hit.normal),
                        Time.deltaTime * 5f);
                }

                /***************************************************************
                 * 向头的左后或右后方进行球体投射检测
                 * 目的是当检测不到碰撞体时向下调整身体高度并使用脚部IK
                 * 如果在头部初始高度检测不到则每次下降一个直径单位再检测
                 ***************************************************************/
                bool headCastResult = false;
                for (int i = 0; i < headDownCastCountLimit; i++)
                {
                    headSphereCastOrigin = controller.transform.position
                        + Vector3.up * (headOriginPosY - i * headRadius * 2)
                        + controller.transform.right * hInputSign * headRadius * 2;
                    headCastResult = Physics.SphereCast(headSphereCastOrigin,
                        headRadius, controller.transform.forward, out headHitInfo);
                    if (headCastResult) break;
                }
                if (headCastResult)
                {
                    targetBodyPositionY = originBodyPositionY 
                        - (headOriginPosY - headHitInfo.point.y) - headRadius;
                    cc.Move(horizontal * sneakSpeed * Time.deltaTime
                        * controller.transform.right);
                }

                /***************************************************************
                 * 从脚部上方一定单位处向下进行射线投射检测
                 * 目的是获取脚部IK目标位置和旋转
                 ***************************************************************/
                Vector3 leftFootPosition = controller.animator
                    .GetBoneTransform(HumanBodyBones.LeftFoot).position;
                leftFootPosition.y = controller.transform.position.y + raycastHeight;
                if (Physics.Raycast(leftFootPosition, Vector3.down,
                    out RaycastHit footHitInfo, raycastHeight + .01f))
                {
                    leftFootIkPosition = footHitInfo.point;
                    leftFootIkRotation = Quaternion.FromToRotation(
                        controller.transform.up, footHitInfo.normal);
                }
                else leftFootIkPosition = Vector3.zero;
                Vector3 rightFootPosition = controller.animator
                    .GetBoneTransform(HumanBodyBones.RightFoot).position;
                rightFootPosition.y = controller.transform.position.y + raycastHeight;
                if (Physics.Raycast(rightFootPosition, Vector3.down,
                    out footHitInfo, raycastHeight + .01f))
                {
                    rightFootIkPosition = footHitInfo.point;
                    rightFootIkRotation = Quaternion.FromToRotation(
                        controller.transform.up, footHitInfo.normal);
                }
                else rightFootIkPosition = Vector3.zero;

                //设置动画参数值
                coverDirection = Mathf.Lerp(coverDirection,
                    hInputSign, Time.deltaTime * dirLerpSpeed);
                controller.animator.SetFloat("Cover Direction",
                    coverDirection);
                controller.animator.SetFloat("Cover Sneak",
                    Mathf.Abs(horizontal));

                //当垂直方向上的输入小于0时退出掩体状态
                float vertical = Input.GetAxis("Vertical");
                if (vertical < 0f)
                {
                    timer = 0f;
                    startPosition = controller.transform.position;
                    startRotation = controller.transform.rotation;
                    Physics.Raycast(controller.transform.position + Vector3.up 
                        * raycastHeight, controller.transform.forward,
                        out RaycastHit hit, 0.5f);
                    targetPosition = controller.transform.position 
                        + hit.normal * (raycastDistance + .1f);
                    targetRotation = Quaternion.LookRotation(-hit.normal);
                    state = State.CoverToStand;
                    controller.animator.SetBool("Cover", false);
                    controller.EnableCollider(false);
                }
                break;
            case State.CoverToStand:
                timer += Time.deltaTime;
                if (timer > actionDuration)
                    timer = actionDuration;
                t = timer / actionDuration;
                controller.transform.SetPositionAndRotation(
                    Vector3.Lerp(startPosition, targetPosition, t),
                    Quaternion.Lerp(startRotation, targetRotation, t));
                return timer < actionDuration;
        }
        return true;
    }

    public override void OnAnimatorIK(int layerIndex)
    {
        if (state != State.Covering) return;
        if (originBodyPositionY == 0)
        {
            originBodyPositionY = controller.animator.bodyPosition.y;
            targetBodyPositionY = controller.animator.bodyPosition.y;
            lastBodyPositionY = originBodyPositionY;
            return;
        }
        Vector3 bodyPosition = controller.animator.bodyPosition;
        bodyPosition.y = Mathf.Lerp(lastBodyPositionY, 
            targetBodyPositionY, bodyPositionLerpSpeed);
        controller.animator.bodyPosition = bodyPosition;
        lastBodyPositionY = controller.animator.bodyPosition.y;
        //左脚IK
        controller.animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1f);
        controller.animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1f);
        Vector3 targetIkPosition = controller.animator
            .GetIKPosition(AvatarIKGoal.LeftFoot);
        if (leftFootIkPosition != Vector3.zero)
        {
            targetIkPosition = controller.transform
                .InverseTransformPoint(targetIkPosition);
            Vector3 world2Local = controller.transform
                .InverseTransformPoint(leftFootIkPosition);
            float y = Mathf.Lerp(lastLeftFootPositionY,
                world2Local.y, footPositionLerpSpeed);
            targetIkPosition.y += y;
            lastLeftFootPositionY = y;
            targetIkPosition = controller.transform
                .TransformPoint(targetIkPosition);
            Quaternion currRotation = controller.animator
                .GetIKRotation(AvatarIKGoal.LeftFoot);
            Quaternion nextRotation = leftFootIkRotation * currRotation;
            controller.animator.SetIKRotation(
                AvatarIKGoal.LeftFoot, nextRotation);
        }
        controller.animator.SetIKPosition(
            AvatarIKGoal.LeftFoot, targetIkPosition);
        //右脚IK
        controller.animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1f);
        controller.animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1f);
        targetIkPosition = controller.animator
            .GetIKPosition(AvatarIKGoal.RightFoot);
        if (rightFootIkPosition != Vector3.zero)
        {
            targetIkPosition =  controller.transform
                .InverseTransformPoint(targetIkPosition);
            Vector3 world2Local = controller.transform
                .InverseTransformPoint(rightFootIkPosition);
            float y = Mathf.Lerp(lastRightFootPositionY, 
                world2Local.y, footPositionLerpSpeed);
            targetIkPosition.y += y;
            lastRightFootPositionY = y;
            targetIkPosition = controller.transform
                .TransformPoint(targetIkPosition);
            Quaternion currRotation = controller.animator
                .GetIKRotation(AvatarIKGoal.RightFoot);
            Quaternion nextRotation = rightFootIkRotation * currRotation;
            controller.animator.SetIKRotation(
                AvatarIKGoal.RightFoot, nextRotation);
        }
        controller.animator.SetIKPosition(
            AvatarIKGoal.RightFoot, targetIkPosition);
    }

    public override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        Color cacheColor = Gizmos.color;
        Gizmos.color = Color.magenta;
        if (targetPosition != Vector3.zero)
        {
            Gizmos.DrawSphere(targetPosition, .13f);
#if UNITY_EDITOR
            GUIStyle style = new GUIStyle(GUI.skin.label) { fontSize = 15 };

            UnityEditor.Handles.Label(targetPosition + Vector3.right * .1f,
                "To Cover Target Position", style);
#endif
        }

        if (state != State.Covering)
        {
            Gizmos.color = Color.white;
            PhysicsGizmos.BoxCastWire(controller.transform.position
                + raycastHeight * Vector3.up, boxCastSize * .5f,
                controller.transform.rotation,
                controller.transform.worldToLocalMatrix,
                controller.transform.forward, raycastDistance);
        }
        else
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(headSphereCastOrigin, headRadius);
        }
        Gizmos.color = cacheColor;
    }
}