using UnityEngine;

public class BlendShapeExample : MonoBehaviour
{
    private SkinnedMeshRenderer smr;
    private void Start()
    {
        smr = GetComponent<SkinnedMeshRenderer>();
    }
    private void OnGUI()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("Jaw Open");
        //根据Blend Shape名称获取索引值
        int index = smr.sharedMesh.GetBlendShapeIndex("jawOpen");
        float weight = GUILayout.HorizontalSlider(
            smr.GetBlendShapeWeight(index), //获取当前权重值
            0f, 100f, GUILayout.Width(200f));
        //根据滑动条设置Blend Shape权重值
        smr.SetBlendShapeWeight(index, weight);
        GUILayout.EndHorizontal();
    }
}