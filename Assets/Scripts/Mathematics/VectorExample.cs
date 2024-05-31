using UnityEngine;

public class VectorExample : MonoBehaviour
{
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        //VectorPlusGizmos();
        //VectorPlusGizmos2();
        //VectorMinusGizmos();
        LerpExample();
    }

    [SerializeField] private Transform a;
    [SerializeField] private Transform b;

    //平行四边形法则
    private void VectorPlusGizmos()
    {
        HandleHelper.DrawArrow(Vector3.zero, a.position, Vector3.up);
        HandleHelper.DrawArrow(Vector3.zero, b.position, Vector3.up);

        Vector3 c = a.position + b.position;
        UnityEditor.Handles.DrawDottedLine(a.position, c, 10f);
        UnityEditor.Handles.DrawDottedLine(b.position, c, 10f);

        HandleHelper.DrawArrow(Vector3.zero, c, Vector3.up);

        GUIStyle style = new GUIStyle(GUI.skin.label) { fontSize = 25, fontStyle = FontStyle.Bold };
        UnityEditor.Handles.Label(Vector3.zero + Vector3.left * .1f, "O", style);
        UnityEditor.Handles.Label(a.position + Vector3.left * .1f + Vector3.forward * .1f, "A", style);
        UnityEditor.Handles.Label(b.position, "B", style);
        UnityEditor.Handles.Label(c + Vector3.forward * .1f, "C", style);
    }
    //三角形法则
    private void VectorPlusGizmos2()
    {
        HandleHelper.DrawArrow(Vector3.zero, a.position, Vector3.up);
        HandleHelper.DrawArrow(a.position, b.position, Vector3.up);
        HandleHelper.DrawArrow(Vector3.zero, b.position, Vector3.up);


        GUIStyle style = new GUIStyle(GUI.skin.label) { fontSize = 25, fontStyle = FontStyle.Bold };
        UnityEditor.Handles.Label(Vector3.zero + Vector3.left * .1f, "O", style);
        UnityEditor.Handles.Label(a.position, "A", style);
        UnityEditor.Handles.Label(b.position + Vector3.forward * .1f, "B", style);
    }
    //向量相减
    private void VectorMinusGizmos()
    {
        HandleHelper.DrawArrow(Vector3.zero, a.position, Vector3.up);
        HandleHelper.DrawArrow(a.position, b.position, Vector3.up);
        HandleHelper.DrawArrow(Vector3.zero, b.position, Vector3.up);


        GUIStyle style = new GUIStyle(GUI.skin.label) { fontSize = 25, fontStyle = FontStyle.Bold };
        UnityEditor.Handles.Label(Vector3.zero + Vector3.left * .1f, "O", style);
        UnityEditor.Handles.Label(a.position + Vector3.forward * .1f, "A", style);
        UnityEditor.Handles.Label(b.position, "B", style);
    }

    private void LerpExample()
    {
        GUIStyle style = new GUIStyle(GUI.skin.label) { fontSize = 25, fontStyle = FontStyle.Bold };
        UnityEditor.Handles.Label(transform.position, "O", style);
        UnityEditor.Handles.Label(a.position + Vector3.left * .05f, "A", style);
        UnityEditor.Handles.Label(b.position, "B", style);
        //UnityEditor.Handles.color = Color.cyan;
        UnityEditor.Handles.DrawLine(transform.position, a.position, 3f);
        UnityEditor.Handles.DrawLine(transform.position, b.position, 3f);

        for (int i = 1; i < 10; i++)
        {
            //插值点
            //Vector3 l = Vector3.Lerp(a.position, b.position, i * .1f);
            Vector3 l = Vector3.Slerp(a.position, b.position, i * .1f);
            //UnityEditor.Handles.color = Color.red;
            //绘制点O到插值点的线段
            UnityEditor.Handles.DrawLine(transform.position, l, 3f);
            //UnityEditor.Handles.color = Color.yellow;
            //绘制插值点之间的线段
            //UnityEditor.Handles.DrawLine(l, Vector3.Lerp(a.position, b.position, (i - 1) * .1f), 3f);
            UnityEditor.Handles.DrawLine(l, Vector3.Slerp(a.position, b.position, (i - 1) * .1f), 3f);
            UnityEditor.Handles.Label(l, $"{i}", style);
        }
        //UnityEditor.Handles.DrawLine(b.position, Vector3.Lerp(a.position, b.position, .9f), 3f);
        UnityEditor.Handles.DrawLine(b.position, Vector3.Slerp(a.position, b.position, .9f), 3f);
    }
#endif
}