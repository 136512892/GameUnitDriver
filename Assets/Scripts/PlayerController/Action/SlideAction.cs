using UnityEngine;

public class SlideAction : AvatarAction
{
    public SlideAction(AvatarController controller, 
        AvatarActionSettings settings) : base(controller, settings) { }

    public override bool ActionDoableCheck()
    {
        if (!controller.IsGrounded) return false;
        //在指定高度处向前进行射线投射检测
        Vector3 origin = controller.transform.position 
            + raycastHeight * Vector3.up;
        if (Physics.Raycast(origin, controller.transform.forward,
            out RaycastHit hitInfo, raycastDistance))
        {
            if (!hitInfo.collider.CompareTag(targetTag)) return false;
            if (hitInfo.distance <= 1f) return false; //距离过小
            //法线反方向和角色前方的向量叉积
            float dot = Vector3.Dot(-hitInfo.normal,
                controller.transform.forward);
            float angle = Mathf.Acos(dot) * Mathf.Rad2Deg;
            if (angle > 45f) return false; //角度过大
            //法线方向和碰撞物体前方的向量叉积
            dot = Vector3.Dot(hitInfo.normal, 
                hitInfo.transform.forward);
            if (Mathf.Abs(dot) < .8f) return false; //与碰撞物体z轴方向角度相差过大
            Debug.DrawLine(hitInfo.point, 
                hitInfo.point + hitInfo.normal, Color.blue);

            //碰撞体在z轴上的大小
            Bounds bounds = new Bounds();
            var mf = hitInfo.collider.GetComponent<MeshFilter>();
            bounds.Encapsulate(mf.sharedMesh.bounds);
            float distance = bounds.size.z;
            if (distance > actionMaxDistance) return false; //距离过大
            
            //沿法线反方向在碰撞体z轴大小加一定偏移量处向下进行射线投射检测
            origin += hitInfo.point - origin + -hitInfo.normal * (distance + .8f);
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hitInfo2, 
                raycastHeight + .01f, 1 << LayerMask.NameToLayer("Ground")))
            {
                timer = 0f;
                startPosition = controller.transform.position;
                startRotation = controller.transform.rotation;
                targetPosition = hitInfo2.point;
                targetRotation = Quaternion.LookRotation(
                    targetPosition - startPosition);
                controller.EnableCollider(false);
                controller.animator.SetTrigger("Slide");
                return true;
            }
        }
        targetPosition = Vector3.zero;
        return false;
    }

    public override bool ActionExecute()
    {
        timer += Time.deltaTime;
        if (timer > actionDuration)
            timer = actionDuration;
        float t = timer / actionDuration;
        controller.transform.SetPositionAndRotation(
            Vector3.Lerp(startPosition, targetPosition, t),
            Quaternion.Lerp(startRotation, targetRotation, t));
        return timer < actionDuration;
    }

    public override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        Color cacheColor = Gizmos.color;
        Gizmos.color = Color.magenta;
        Vector3 origin = controller.transform.position
            + raycastHeight * Vector3.up;
        Gizmos.DrawWireSphere(origin, .08f);
#if UNITY_EDITOR
        GUIStyle style = new GUIStyle(GUI.skin.label) { fontSize = 15 };
        UnityEditor.Handles.Label(origin + Vector3.right * .1f,
            "Slide Raycast Origin", style);
#endif
        if (targetPosition != Vector3.zero)
        {
            Gizmos.DrawSphere(targetPosition, .13f);
#if UNITY_EDITOR
            UnityEditor.Handles.Label(targetPosition + Vector3.right * .1f, 
                "Slide Target Position", style);
#endif
        }
        Gizmos.color = cacheColor;
    }
}
