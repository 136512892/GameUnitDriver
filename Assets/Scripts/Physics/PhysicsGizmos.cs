using System.Linq;
using System.Collections.Generic;

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PhysicsGizmos
{
    private static readonly GUIStyle m_style 
        = new GUIStyle(GUI.skin.label) { fontSize = 20, fontStyle = FontStyle.Bold };

    #region >> Box
    public static Vector3[] GetBoxVertices(Vector3 center,
    Vector3 halfExtents, Quaternion orientation, Matrix4x4 world2Local)
    {
        Vector3[] verts = new Vector3[8];
        int index = -1;
        for (int z = -1; z <= 1; z += 2)
        {
            for (int y = -1; y <= 1; y += 2)
            {
                for (int x = -1; x <= 1; x += 2)
                {
                    verts[++index] = center + new Vector3(
                      -Mathf.Sign(y) * x * halfExtents.x,
                      y * halfExtents.y, 
                      z * halfExtents.z);
                    verts[index] = orientation * world2Local
                        .MultiplyPoint(verts[index]);
                    verts[index] = world2Local.inverse
                        .MultiplyPoint(verts[index]);
                }
            }
        }
        return verts;
    }

    public static void OverlapBoxWire(Vector3 center, Vector3 halfExtents, 
        Quaternion orientation, Matrix4x4 world2Local)
    {
        Vector3[] verts = GetBoxVertices(
            center, halfExtents, orientation, world2Local);
#if UNITY_EDITOR
        //for (int i = 0; i < verts.Length; i++)
            //Handles.Label(verts[i], i.ToString(), m_style);
#endif
        OverlapBoxWire(verts);
    }

    public static void OverlapBoxWire(Vector3[] verts)
    {
        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                Vector3 curr = verts[j % 4 + i * 4];
                Vector3 next = verts[(j + 1) % 4 + i * 4];
                Gizmos.DrawLine(curr, next);
                if (i == 0)
                    Gizmos.DrawLine(curr, verts[j % 4 + (i + 1) * 4]);
            }
        }
    }

    public static void BoxCastWire(Vector3 center, Vector3 halfExtents, 
        Quaternion orientation, Matrix4x4 world2Local, 
        Vector3 direction, float maxDistance)
    {
        Vector3[] verts = GetBoxVertices(
            center, halfExtents, orientation, world2Local);
        Vector3[] verts2 = new Vector3[4];
        for (int i = 0; i < verts2.Length; i++)
            verts2[i] = verts[i + 4] + maxDistance * direction;
        verts = verts.Concat(verts2).ToArray();
#if UNITY_EDITOR
        //for (int i = 0; i < verts.Length; i++)
            //Handles.Label(verts[i], i.ToString(), m_style);
#endif
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                Vector3 curr = verts[j % 4 + i * 4];
                Vector3 next = verts[(j + 1) % 4 + i * 4];
                Gizmos.DrawLine(curr, next);
                if (i == 0)
                    Gizmos.DrawLine(curr, verts[j % 4 + (i + 2) * 4]);
            }
        }
    }
    #endregion

    #region >> Sphere
    private static Vector3[] GetSphereVertices(Vector3 center, 
        float radius, Quaternion orientation, Matrix4x4 world2Local)
    {
        List<Vector3> verts = new List<Vector3>();
        for (int z = 1; z >= -1; z -= 2)
            verts.Add(center + z * radius * Vector3.forward);
        for (int y = 1; y >= -1; y -= 2)
            verts.Add(center + y * radius * Vector3.up);
        for (int x = 1; x >= -1; x -= 2)
            verts.Add(center + x * radius * Vector3.right);
        for (int i = 0; i < verts.Count; i++)
        {
            verts[i] = orientation * world2Local.MultiplyPoint(verts[i]);
            verts[i] = world2Local.inverse.MultiplyPoint(verts[i]);
        }
        return verts.ToArray();
    }

    private static void SphereWire(Vector3 center, Vector3[] verts, float radius)
    {
#if UNITY_EDITOR
        //for (int i = 0; i < verts.Length; i++)
            //Handles.Label(verts[i], i.ToString(), m_style);
#endif
        for (int i = 0; i < 6; i += 2)
        {
            Vector3 dir1 = verts[(i + 2) % 6] - center;
            Vector3 dir2 = verts[(i + 4) % 6] - center;
            Vector3 normal = Vector3.Cross(dir1, dir2);
#if UNITY_EDITOR
            Color cache = Handles.color;
            Handles.color = Gizmos.color;
            Handles.DrawWireDisc(center, normal, radius);
            Handles.color = cache;
#endif
        }
    }
    public static Vector3[] SphereWire(Vector3 center,
        float radius, Quaternion orientation, Matrix4x4 world2Local)
    {
        Vector3[] verts = GetSphereVertices(
            center, radius, orientation, world2Local);
        SphereWire(center, verts, radius);
        return verts;
    }

    public static void SphereCastWire(Vector3 origin,
    float radius, Quaternion rotation, Matrix4x4 world2Local,
    Vector3 direction, float maxDistance)
    {
        Vector3[] verts1 = SphereWire(
            origin, radius, rotation, world2Local);
        Vector3[] verts2 = new Vector3[6];
        Vector3[] verts3 = new Vector3[6];
        for (int i = 0; i < 6; i++)
        {
            verts2[i] = verts1[i] + radius * 2f * direction;
            verts3[i] = verts1[i] + maxDistance * direction;
            if (i >= 2)
                Gizmos.DrawLine(verts2[i], verts3[i]);
        }
        SphereWire(origin + radius * 2f * direction, verts2, radius);
        SphereWire(origin + maxDistance * direction, verts3, radius);
    }
    #endregion
}