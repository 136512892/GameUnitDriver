using UnityEngine;

public class VectorCrossExample : MonoBehaviour
{
#if UNITY_EDITOR
    [SerializeField] private Transform a;
    [SerializeField] private Transform b;

    private void OnDrawGizmosSelected()
    {
        HandleHelper.DrawArrow(a.position,
            a.position + a.forward, Vector3.up, 3f);
        HandleHelper.DrawArrow(a.position,
            b.position, Vector3.up, 3f);
        Vector3 cross = Vector3.Cross(a.forward, (b.position - a.position).normalized);
        HandleHelper.DrawArrow(a.position,
            a.position + cross.normalized, Vector3.forward, 3f);
        Debug.Log(string.Format("B在A的{0}侧", cross.y > 0 ? "右" : "左"));
        float angle = Mathf.Asin(cross.magnitude) * Mathf.Rad2Deg;
        for (int i = 1; i < angle; i++)
        {
            Vector3 curr = Vector3.Slerp(a.forward,
                b.position - a.position, i / angle) * .1f;
            Vector3 last = Vector3.Slerp(a.forward,
                b.position - a.position, (i - 1) / angle) * .1f;
            UnityEditor.Handles.DrawLine(curr, last, 3f);
        }
        GUIStyle style = new GUIStyle(GUI.skin.label)
        {
            fontSize = 25,
            fontStyle = FontStyle.Bold
        };
        UnityEditor.Handles.Label(a.position + Vector3.left * .1f, "A", style);
        UnityEditor.Handles.Label(b.position + Vector3.left * .1f, "B", style);
        UnityEditor.Handles.Label(a.position + cross.normalized * 1.1f, 
            string.Format("{0}", cross), style);
    }
#endif
}