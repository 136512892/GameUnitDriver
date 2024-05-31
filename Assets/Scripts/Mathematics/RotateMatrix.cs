/* =======================================================
 *  Unity版本：2020.3.16f1c1
 *  作 者：张寿昆
 *  邮 箱：136512892@qq.com
 *  创建时间：2024-03-14 09:26:56
 *  当前版本：1.0.0
 *  主要功能：
 *  详细描述：
 *  修改记录：
 * =======================================================*/

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class RotateMatrix : MonoBehaviour
{
#if UNITY_EDITOR
    [SerializeField] private Transform a;
    [SerializeField] private Transform b;

    private void OnDrawGizmos()
    {
        GUIStyle style = new GUIStyle(GUI.skin.label)
        {
            fontSize = 25,
            fontStyle = FontStyle.Bold
        };

        HandleHelper.DrawArrow(Vector3.back * .5f, Vector3.forward * 1.5f, Vector3.up, 3f);
        HandleHelper.DrawArrow(Vector3.left * .3f, Vector3.right * 1.6f, Vector3.up, 3f);

        HandleHelper.DrawArrow(Vector3.zero, a.position.normalized, Vector3.up, 3f);
        HandleHelper.DrawArrow(Vector3.zero, b.position.normalized, Vector3.up, 3f);


        float angle1 = Mathf.Acos(Vector3.Dot(Vector3.right, a.position.normalized)) * Mathf.Rad2Deg;
        float angle2 = Mathf.Acos(Vector3.Dot(Vector3.right, b.position.normalized)) * Mathf.Rad2Deg;
        for (int i = 1; i < angle1; i++)
        {
            Vector3 curr = Vector3.Slerp(Vector3.right, a.position.normalized, i / angle1) * .2f;
            Vector3 last = Vector3.Slerp(Vector3.right, a.position.normalized, (i - 1) / angle1) * .2f;
            Handles.DrawLine(curr, last, 3f);
            if (i == (int)angle1 / 2)
                Handles.Label(curr + Vector3.right * .05f + Vector3.forward * .08f, "α", style);
        }
        for (int i = 1; i < angle2; i++)
        {
            Vector3 curr = Vector3.Slerp(a.position.normalized, b.position.normalized, i / angle1) * .15f;
            Vector3 last = Vector3.Slerp(a.position.normalized, b.position.normalized, (i - 1) / angle1) * .15f;
            Handles.DrawLine(curr, last, 3f);
            if (i == (int)angle2 / 2)
                Handles.Label(curr + Vector3.right * .08f + Vector3.forward * .1f, "β", style);
        }
        float y = Mathf.Sin(angle1 * Mathf.Deg2Rad) * a.position.normalized.magnitude;
        Handles.DrawDottedLine(a.position.normalized, a.position.normalized + Vector3.back * y, 2f);
        Handles.DrawDottedLine(a.position.normalized, Vector3.forward * y, 2f);
        y = Mathf.Sin(angle2 * Mathf.Deg2Rad) * a.position.normalized.magnitude;
        Handles.DrawDottedLine(b.position.normalized, b.position.normalized + Vector3.back * y, 2f);
        Handles.DrawDottedLine(b.position.normalized, Vector3.forward * y, 2f);

        Handles.Label(Vector3.left * .1f, "o", style);
        Handles.Label(Vector3.forward * 1.5f + Vector3.left * .1f, "y", style);
        Handles.Label(Vector3.right * 1.6f, "x", style);

        Handles.Label(a.position.normalized + Vector3.right * .05f, "P(x,y)", style);
        Handles.Label(b.position.normalized + Vector3.right * .05f, "P'(x',y')", style);
    }
#endif
}