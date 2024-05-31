using UnityEngine;

public class ClimbAction : AvatarAction
{
    public ClimbAction(AvatarController controller, 
        AvatarActionSettings settings) : base(controller, settings) 
    {
        ClimbActionSettings s = settings as ClimbActionSettings;
        boxCastSize = s.boxCastSize;
        boxCastCountLimit = s.boxCastCountLimit;
        disBetweenArms = s.disBetweenArms;
        handSize = s.handSize;
        bodyOffset = s.bodyOffset;
        idle2HangCurve = s.idle2HangCurve;
        dropHangCurve = s.dropCurve;
        dropDuration = s.dropDuration;
        hopUpDuration = s.hopUpDuration;
        hopDownDuration = s.hopDownDuration;
        hopHorizontalDuration = s.hopHorizontalDuration;
    }

    public class ClimbableObject
    {
        public Transform transform;
        public Bounds bounds;

        public ClimbableObject(Transform transform, Bounds bounds)
        {
            this.transform = transform;
            this.bounds = bounds;
        }
    }

    private readonly Vector3 boxCastSize;
    private readonly int boxCastCountLimit;
    private Vector3 boxCastOrigin;
    private readonly float disBetweenArms;
    private readonly float handSize;
    private Vector3 leftHandIKPos;
    private Vector3 rightHandIKPos;
    //通过两手目标IK位置的中点加该偏移值得到角色目标位置
    private Vector3 bodyOffset;
    private readonly AnimationCurve idle2HangCurve;
    private readonly AnimationCurve dropHangCurve;
    private readonly float dropDuration, hopUpDuration, 
        hopDownDuration, hopHorizontalDuration;
    public enum State { None, Idle2Hang, Hanging, Drop, Hop };
    private State state = State.None;
    private ClimbableObject current;

    public override bool ActionDoableCheck()
    {
        if (!controller.IsGrounded) return false;
        if (ClimbableObjectCheck(0f, 1f, out RaycastHit hitInfo))
        {
            if (Input.GetAxis("Vertical") > 0f && Input.GetKeyDown(KeyCode.Space))
            {
                current = ClimbableObjectChange(hitInfo);
                controller.EnableCollider(false);
                controller.animator.SetBool("Hang", true);
                state = State.Idle2Hang;
                return true;
            }
        }
        targetPosition = Vector3.zero;
        return false;
    }

    public override bool ActionExecute()
    {
        switch (state)
        {
            case State.Idle2Hang:
            case State.Hop:
                timer += Time.deltaTime;
                int i = controller.animator.GetInteger("HopIndex");
                float duration = i == 1 || i == 2 ? hopHorizontalDuration
                    : i == 3 ? hopUpDuration : hopDownDuration;
                if (timer > duration)
                    timer = duration;
                float t = timer / duration;
                controller.transform.SetPositionAndRotation(
                    Vector3.Lerp(startPosition, targetPosition, idle2HangCurve.Evaluate(t)),
                    Quaternion.Lerp(startRotation, targetRotation, t));
                if (t >= 1f)
                {
                    state = State.Hanging;
                    controller.animator.SetInteger("HopIndex", 0);
                }
                break;
            case State.Hanging:
                float horizontal = Input.GetAxis("Horizontal");
                float vertical = Input.GetAxis("Vertical");
                bool result = ClimbableObjectCheck(horizontal, vertical, out RaycastHit hitInfo);
                var climbableObject = result ? ClimbableObjectChange(hitInfo) : null;
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    if (horizontal == 0 && vertical == 0)
                    {
                        timer = 0f;
                        startPosition = controller.transform.position;
                        startRotation = controller.transform.rotation;
                        Physics.Raycast(controller.transform.position,
                            Vector3.down, out RaycastHit hit);
                        targetPosition = hit.point
                            - controller.transform.forward * .5f;
                        targetRotation = controller.transform.rotation;
                        state = State.Drop;
                        controller.animator.SetBool("Hang", false);
                        current = null;
                    }
                    else
                    {
                        if (result)
                        {
                            current = climbableObject;
                            Vector3 direction = targetPosition - startPosition;
                            float h = Vector3.Dot(direction, controller.transform.right);
                            float v = Vector3.Dot(direction, controller.transform.up);
                            int index = Mathf.Abs(h) >= Mathf.Abs(v)
                                ? (horizontal < 0 ? 1 : 2)
                                : (vertical > 0 ? 3 : 4);
                            controller.animator.SetInteger("HopIndex", index);
                            state = State.Hop;
                        }
                    }
                }
                controller.animator.SetFloat("Hang Shimmy", horizontal);
                Vector3 delta = controller.transform.right
                   * Mathf.Abs(controller.animator.GetFloat("AxisXMovement"))
                   * horizontal;
                controller.transform.position += delta;
                leftHandIKPos += delta;
                rightHandIKPos += delta;
                break;
            case State.Drop:
                timer += Time.deltaTime;
                if (timer > dropDuration)
                    timer = dropDuration;
                t = timer / dropDuration;
                controller.transform.SetPositionAndRotation(
                    Vector3.Lerp(startPosition, targetPosition, dropHangCurve.Evaluate(t)),
                    Quaternion.Lerp(startRotation, targetRotation, t));
                return timer < dropDuration;
        }
        return true;
    }

    private bool ClimbableObjectCheck(float horizontal, float vertical,
    out RaycastHit hitInfo)
    {
        hitInfo = default;
        vertical = vertical > 0f ? 1f : vertical < 0f ? -1f : 0f;
        horizontal = horizontal > 0f ? 1f : horizontal < 0f ? -1f : 0f;
        for (int i = 0; i < boxCastCountLimit; i++)
        {
            for (int j = 0; j < boxCastCountLimit; j++)
            {
                boxCastOrigin = current == null
                    ? controller.transform.position
                        + raycastHeight * controller.transform.up
                    : controller.transform.position
                        + .9f * controller.transform.up;
                boxCastOrigin = boxCastOrigin
                    + j * boxCastSize.y
                        * vertical
                        * controller.transform.up
                    + i * boxCastSize.x
                        * horizontal
                        * controller.transform.right;

                bool result = Physics.BoxCast(boxCastOrigin, boxCastSize * .5f,
                    controller.transform.forward, out hitInfo,
                    controller.transform.rotation, raycastDistance);
                if (result)
                {
                    result = hitInfo.collider.CompareTag(targetTag);
                    if (current != null && current.transform
                        == hitInfo.transform)
                        result = false;
                }
                if (result)
                    return true;
            }
        }
        boxCastOrigin = Vector3.zero;
        return false;
    }

    private ClimbableObject ClimbableObjectChange(RaycastHit hitInfo)
    {
        Bounds bounds = new Bounds();
        var mf = hitInfo.collider.GetComponent<MeshFilter>();
        bounds.Encapsulate(mf.sharedMesh.bounds);
        //两只手目标IK位置的中点
        Vector3 center = new Vector3(
            controller.transform.position.x,
            hitInfo.transform.localToWorldMatrix.MultiplyPoint3x4(
                bounds.center).y + bounds.size.y * .5f,
            (hitInfo.point + .5f * bounds.size.z * -hitInfo.normal).z);
        //中心点向左侧偏移得到左手目标IK位置
        leftHandIKPos = center - .5f * disBetweenArms
            * hitInfo.transform.right;
        //中心点向右侧偏移得到右手目标IK位置
        rightHandIKPos = center + .5f * disBetweenArms
            * hitInfo.transform.right;
        //根据碰撞体调整两手目标IK位置
        AdjustHandIKPosition(bounds, hitInfo.transform,
            ref leftHandIKPos, ref rightHandIKPos);
        //移动到外边缘
        leftHandIKPos += hitInfo.normal * bounds.size.z * .5f;
        rightHandIKPos += hitInfo.normal * bounds.size.z * .5f;

        timer = 0f;
        startPosition = controller.transform.position;
        startRotation = controller.transform.rotation;
        center = (leftHandIKPos + rightHandIKPos) * .5f;
        targetPosition = center + Vector3.down
            * bodyOffset.y + hitInfo.normal * bodyOffset.z;
        targetRotation = Quaternion.LookRotation(-hitInfo.normal);
        return new ClimbableObject(hitInfo.transform, bounds);
    }

    private void AdjustHandIKPosition(Bounds bounds, Transform transform,
        ref Vector3 leftHandIKPosition, ref Vector3 rightHandIKPosition)
    {
        //通过局部转全局的转换矩阵得到中心点的全局坐标
        Vector3 center = transform.localToWorldMatrix
            .MultiplyPoint3x4(bounds.center);
        Vector3 cross = Vector3.Cross(transform.forward,
            controller.transform.position - center);
        //如果左手目标IK位置超出了左边缘，将其设置为左边缘
        if ((leftHandIKPosition - center).magnitude > bounds.size.x * .5f)
        {
            if (cross.y < 0f)
            {
                //中心点向左侧偏移x轴向上大小的一半得到左侧边缘坐标点
                leftHandIKPosition = center - .5f
                    * bounds.size.x * transform.right;
                //向上移动到上边缘
                leftHandIKPosition += .5f * bounds.size.y * transform.up;
                //向右偏移一个手的大小
                leftHandIKPosition += transform.right * handSize;
                //通过两臂之间距离得到右手目标IK位置
                rightHandIKPosition = leftHandIKPosition
                    + transform.right * disBetweenArms;
            }
            else
            {
                rightHandIKPosition = center + .5f
                    * bounds.size.x * transform.right;
                rightHandIKPosition += .5f * bounds.size.y * transform.up;
                rightHandIKPosition -= transform.right * handSize;
                leftHandIKPosition = rightHandIKPosition
                    - transform.right * disBetweenArms;
            }
        }
        //同理，如果右手目标IK位置超出了右边缘，将其设置为右边缘
        else if ((rightHandIKPosition - center).magnitude > bounds.size.x * .5f)
        {
            if (cross.y > 0f)
            {
                //中心点向右侧偏移x轴向上大小的一半得到右侧边缘坐标点
                rightHandIKPosition = center + .5f
                    * bounds.size.x * transform.right;
                //向上移动到上边缘
                rightHandIKPosition += .5f * bounds.size.y * transform.up;
                //向左偏移一个手的大小
                rightHandIKPosition -= transform.right * handSize;
                //通过两臂之间距离得到左手目标IK位置
                leftHandIKPosition = rightHandIKPosition
                    - transform.right * disBetweenArms;
            }
            else
            {
                leftHandIKPosition = center - .5f
                    * bounds.size.x * transform.right;
                leftHandIKPosition += .5f * bounds.size.y * transform.up;
                leftHandIKPosition += transform.right * handSize;
                rightHandIKPosition = leftHandIKPosition
                    + transform.right * disBetweenArms;
            }
        }
    }

    public override void OnAnimatorIK(int layerIndex)
    {
        switch (state)
        {
            case State.None:
                break;
            case State.Idle2Hang:
            case State.Hanging:
            case State.Hop:
                float leftHandIK = controller.animator.GetFloat("LeftHandIK");
                float rightHandIK = controller.animator.GetFloat("RightHandIK");
                controller.animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, leftHandIK);
                controller.animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandIKPos);
                controller.animator.SetIKPositionWeight(AvatarIKGoal.RightHand, rightHandIK);
                controller.animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandIKPos);
                break;
            default:
                break;
        }
    }

    public override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        Color cacheColor = Gizmos.color;
        Gizmos.color = Color.white;
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 origin = current == null
            ? controller.transform.position
                + raycastHeight * controller.transform.up
            : controller.transform.position
                + .9f * controller.transform.up;
        Vector3[] verts = PhysicsGizmos.GetBoxVertices(origin, boxCastSize * .5f,
            controller.transform.rotation, controller.transform.worldToLocalMatrix);
        PhysicsGizmos.OverlapBoxWire(verts);
        for (int i = 0; i < boxCastCountLimit; i++)
        {
            for (int j = 0; j < boxCastCountLimit; j++)
            {
                Vector3[] vs = new Vector3[verts.Length];
                for (int k = 0; k < verts.Length; k++)
                {
                    vs[k] = verts[k]
                        + j * boxCastSize.y
                        * vertical
                        * controller.transform.up
                    + i * boxCastSize.x
                        * horizontal
                        * controller.transform.right;
                }
                PhysicsGizmos.OverlapBoxWire(vs);
            }
        }
        Gizmos.color = Color.cyan;
        PhysicsGizmos.BoxCastWire(boxCastOrigin, boxCastSize * .5f,
            controller.transform.rotation, controller.transform.worldToLocalMatrix,
            controller.transform.forward, raycastDistance);

        Gizmos.color = new Color(0f, 1f, 1f, .5f);
        Gizmos.DrawSphere(leftHandIKPos, .05f);
        Gizmos.DrawSphere(rightHandIKPos, .05f);
        Gizmos.DrawSphere(boxCastOrigin, .05f);

#if UNITY_EDITOR
        GUIStyle style = new GUIStyle(GUI.skin.label) { fontSize = 15 };
#endif

        if (targetPosition != Vector3.zero)
        {
            Gizmos.DrawSphere(targetPosition, .13f);
#if UNITY_EDITOR
            UnityEditor.Handles.Label(targetPosition + Vector3.right * .1f,
                "Climb Target Position", style);
#endif
        }
        Gizmos.color = cacheColor;
    }
}