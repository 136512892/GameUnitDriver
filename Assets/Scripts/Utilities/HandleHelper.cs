#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class HandleHelper
{
    public static void DrawArrow(Vector3 from, Vector3 to, Vector3 planeNormal, float thickness = 2f)
    {
        Handles.DrawLine(from, to, thickness);
        Vector3 direction = (to - from).normalized;
        Vector3 cross = Vector3.Cross(direction, planeNormal).normalized;
        Handles.DrawLine(to, to + Vector3.Lerp(-direction,
            cross, .3f) * .1f, thickness);
        Handles.DrawLine(to, to + Vector3.Lerp(-direction,
            -cross, .3f) * .1f, thickness);
    }

    public static void DrawArrowWithDottedLine(Vector3 from, Vector3 to, Vector3 planeNormal, float screenSpaceSize = 2f)
    {
        Handles.DrawDottedLine(from, to, screenSpaceSize);
        Vector3 direction = (to - from).normalized;
        Vector3 cross = Vector3.Cross(direction, planeNormal).normalized;
        Handles.DrawDottedLine(to, to + Vector3.Lerp(-direction,
            cross, .3f) * .1f, screenSpaceSize);
        Handles.DrawDottedLine(to, to + Vector3.Lerp(-direction,
            -cross, .3f) * .1f, screenSpaceSize);
    }
}
#endif