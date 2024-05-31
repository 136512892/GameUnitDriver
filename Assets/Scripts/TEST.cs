using UnityEngine;
using System.Collections.Generic;

public class TEST : MonoBehaviour
{
    private Animator animator;

    private void Start()
    {
        animator = GetComponentInParent<Animator>();
    }

    private void OnGUI()
    {
        if (GUILayout.Button("TEST", GUILayout.Width(200f), GUILayout.Height(50f)))
        {
            Destroy(animator);
            ApplyNewBindpose(GetNewBindposesMap());
        }
    }

    private Dictionary<string, Matrix4x4> GetNewBindposesMap()
    {
        Dictionary<string, Matrix4x4> bindposesMap = new Dictionary<string, Matrix4x4>();
        SkinnedMeshRenderer smr = GetComponent<SkinnedMeshRenderer>();
        Mesh mesh = Instantiate(smr.sharedMesh);
        Matrix4x4[] bindposes = mesh.bindposes;
        Transform[] bones = smr.bones;
        for (int i = 0; i < bones.Length; i++)
        {
            Matrix4x4 oldBindpose = bones[i].worldToLocalMatrix;
            if (bones[i].name == "Head")
                bones[i].localPosition += new Vector3(0f, 0f, 0.2f);
            Matrix4x4 newBindpose = oldBindpose * bones[i].localToWorldMatrix * oldBindpose;
            bindposesMap.Add(bones[i].name, newBindpose);
        }
        return bindposesMap;
    }

    private void ApplyNewBindpose(Dictionary<string, Matrix4x4> bindposesMap)
    {
        SkinnedMeshRenderer smr = GetComponent<SkinnedMeshRenderer>();
        Mesh mesh = Instantiate(smr.sharedMesh);
        Matrix4x4[] bindposes = mesh.bindposes;
        Transform[] bones = smr.bones;
        for (int i = 0; i < bones.Length; i++)
        {
            if (bindposesMap.ContainsKey(bones[i].name))
            {
                bindposes[i] = bindposesMap[bones[i].name];
            }
        }
        mesh.bindposes = bindposes;
        smr.sharedMesh = mesh;
    }

    private void OnDrawGizmos()
    {
        GUIStyle style = new GUIStyle(GUI.skin.label)
        {
            fontSize = 25,
            fontStyle = FontStyle.Bold
        };
        UnityEditor.Handles.Label(Vector3.zero, "起始点", style);
        UnityEditor.Handles.Label(Vector3.right * 10f, "目标点", style);
    }
}