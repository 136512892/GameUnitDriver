using UnityEngine;

public class VaultAction : AvatarAction
{
    public VaultAction(AvatarController controller,
        AvatarActionSettings settings) : base(controller, settings) { }

    private Vector3 leftHandIKPosition;
    private Quaternion leftHandIKRotation;

    public override bool ActionDoableCheck()
    {
        if (!controller.IsGrounded) return false;
        Vector3 origin = controller.transform.position
            + raycastHeight * Vector3.up;
        if (Physics.Raycast(origin, controller.transform.forward,
            out RaycastHit hitInfo, raycastDistance))
        {
            if (!hitInfo.collider.CompareTag(targetTag)) return false;
            if (hitInfo.distance <= .8f) return false;
            float dot = Vector3.Dot(-hitInfo.normal, 
                controller.transform.forward);
            float angle = Mathf.Acos(dot) * Mathf.Rad2Deg;
            if (angle > 25f) return false;
            dot = Vector3.Dot(hitInfo.normal, 
                hitInfo.transform.forward);
            if (Mathf.Abs(dot) < .8f) return false;
            Debug.DrawLine(hitInfo.point, 
                hitInfo.point + hitInfo.normal, Color.blue);
            
            Bounds bounds = new Bounds();
            var mf = hitInfo.collider.GetComponent<MeshFilter>();
            bounds.Encapsulate(mf.sharedMesh.bounds);
            float distance = bounds.size.z;
            if (distance > actionMaxDistance) return false;

            origin += hitInfo.point - origin + -hitInfo.normal * (distance + 1f);
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
                controller.animator.SetTrigger("Vault");

                //计算左手IK目标位置和旋转
                leftHandIKPosition = hitInfo.point + -hitInfo.normal
                    * bounds.size.z * .5f; //z轴上取中点
                //y轴上取最高点，并加一定偏移量，手掌越厚偏移值应越大
                leftHandIKPosition.y = bounds.center.y
                    + bounds.size.y * .5f + .08f;
                //根据叉乘求得左侧
                Vector3 left = Vector3.Cross(Vector3.up, hitInfo.normal);
                leftHandIKPosition += left * .6f; //向左侧偏移一定单位
                leftHandIKRotation = Quaternion.LookRotation(
                    -hitInfo.normal, Vector3.up);
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

    public override void OnAnimatorIK(int layerIndex)
    {
        float leftHandIK = controller.animator
            .GetFloat("LeftHandIK");
        controller.animator.SetIKPositionWeight(
            AvatarIKGoal.LeftHand, leftHandIK);
        controller.animator.SetIKPosition(
            AvatarIKGoal.LeftHand, leftHandIKPosition);
        controller.animator.SetIKRotationWeight(
            AvatarIKGoal.LeftHand, leftHandIK);
        controller.animator.SetIKRotation(
            AvatarIKGoal.LeftHand, leftHandIKRotation);
    }

    public override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        Color cacheColor = Gizmos.color;
        Gizmos.color = Color.magenta;
        //射线投射检测的起点
        Vector3 origin = controller.transform.position
            + raycastHeight * Vector3.up;
        Gizmos.DrawWireSphere(origin, .08f);
#if UNITY_EDITOR
        GUIStyle style = new GUIStyle(GUI.skin.label) { fontSize = 15 };
        UnityEditor.Handles.Label(origin + Vector3.right * .1f,
            "Vault Raycast Origin", style);
#endif
        if (targetPosition != Vector3.zero)
        {
            Gizmos.DrawSphere(targetPosition, .13f);
#if UNITY_EDITOR
            UnityEditor.Handles.Label(targetPosition + Vector3.right * .1f,
                "Vault Target Position", style);

            //在左手目标IK位置绘制箭头
            UnityEditor.Handles.ArrowHandleCap(0, leftHandIKPosition,
                Quaternion.LookRotation(Vector3.up), .5f, EventType.Repaint);
            UnityEditor.Handles.Label(leftHandIKPosition,
                "Left Hand IK Position", style);
#endif
        }
        Gizmos.color = cacheColor;
    }
}