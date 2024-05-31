using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PhysicsExample : MonoBehaviour
{
    [SerializeField]
    private Vector3 boxSize = new Vector3(.4f, .4f, .6f);
    [SerializeField]
    private float maxDistance = 1.5f;
    private Vector3 center;
    private bool flag;
    private RaycastHit hitInfo;

    [SerializeField] private float radius = .222f;

    private void Update()
    {
        center = transform.position + transform.up;
        //flag = Physics.BoxCast(center, boxSize * .5f, transform.forward,
        //    out hitInfo, transform.rotation, maxDistance);

        flag = Physics.SphereCast(center, radius,
            transform.forward, out hitInfo, maxDistance);
    }

    private void OnDrawGizmos()
    {
        //PhysicsGizmos.BoxCastWire(center, boxSize * .5f,
        //    transform.rotation, transform.worldToLocalMatrix,
        //    transform.forward, maxDistance);

        PhysicsGizmos.SphereCastWire(center, radius,
            transform.rotation, transform.worldToLocalMatrix,
            transform.forward, maxDistance);
        if (flag)
        {
            //Gizmos.DrawLine(hitInfo.point, 
            //    hitInfo.point + hitInfo.normal * hitInfo.distance);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(hitInfo.point, .05f);
#if UNITY_EDITOR
            Handles.Label(hitInfo.point,
                string.Format("碰撞点：{0} 碰撞距离：{1}", 
                    hitInfo.point, hitInfo.distance),
                new GUIStyle(GUI.skin.label) { fontSize = 15 });
#endif
        }
    }
}