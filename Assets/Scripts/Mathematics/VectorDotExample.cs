using UnityEngine;

public class VectorDotExample : MonoBehaviour
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
        float dot = Vector3.Dot(a.forward, 
            (b.position - a.position).normalized);
        float angle = Mathf.Acos(dot) * Mathf.Rad2Deg;
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
        UnityEditor.Handles.Label(a.position + Vector3.left * .05f, "A", style);
        UnityEditor.Handles.Label(b.position, "B", style);
        UnityEditor.Handles.Label(a.position + Vector3.left * .45f, $"{angle}°", style);
    }
#endif
}