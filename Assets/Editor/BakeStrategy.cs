using UnityEngine;
using UnityEditor;

using System.Linq;
using System.Collections;
using System.Collections.Generic;

public abstract class BakeStrategy
{
    public abstract IEnumerator Execute(
        Animator animator, AnimationClip clip);

    public Keyframe[] KeyFramesFilter(Keyframe[] keyframes, int frames)
    {
        keyframes = Enumerable.Range(0, frames)
            .Where(i =>
            {
                bool sameWithPrev = (i - 1) >= 0
                    && keyframes[i - 1].value
                    == keyframes[i].value;
                bool sameWithLast = (i + 1) < frames
                    && keyframes[i + 1].value
                    == keyframes[i].value;
                return !sameWithPrev || !sameWithLast;
            })
            .Select(i => keyframes[i])
            .ToArray();
        return keyframes;
    }

    public Keyframe[] TangentSmooth(Keyframe[] keyframes)
    {
        for (int i = 1; i < keyframes.Length - 1; i++)
        {
            var prev = keyframes[i - 1];
            var curr = keyframes[i];
            var next = keyframes[i + 1];
            float tangent = ((curr.value - prev.value) / (curr.time - prev.time)
                + (next.value - curr.value) / (next.time - curr.time)) * .5f;
            keyframes[i].inTangent = tangent;
            keyframes[i].outTangent = tangent;
        }
        return keyframes;
    }
}

public class FootIKCurve : BakeStrategy
{
    public override IEnumerator Execute(
        Animator animator, AnimationClip clip)
    {
        int taskId = Progress.Start("Animation Curve Bake",
            "Baking AxisZMovement Curve", Progress.Options.None, -1);
        //获取资产路径进而获取资产导入器
        ModelImporter importer = AssetImporter.GetAtPath(
            AssetDatabase.GetAssetPath(clip)) as ModelImporter;
        //在该资产导入器中根据名称找到目标动画剪辑
        ModelImporterClipAnimation[] clipAnimations
            = importer.clipAnimations;
        ModelImporterClipAnimation target = clipAnimations
            .FirstOrDefault(m => m.name == clip.name);
        //如果已有同名曲线则将其筛除
        target.curves = target.curves.Where(m 
            => m.name != "LeftFootIK"
            && m.name != "RightFootIK").ToArray();
        //保存并重新导入
        importer.SaveAndReimport();
        yield return null;
        //帧数等于动画剪辑的时长乘以采样率并向上取整
        int samplingRate = 30;
        int frames = Mathf.CeilToInt(clip.length * samplingRate);
        Keyframe[] lKeyframes = new Keyframe[frames];
        Keyframe[] rKeyframes = new Keyframe[frames];
        //采样之前记录初始姿态
        Dictionary<Transform, (Vector3, Quaternion)> pose
            = new Dictionary<Transform, (Vector3, Quaternion)>();
        Transform[] children = animator
            .GetComponentsInChildren<Transform>();
        for (int i = 0; i < children.Length; i++)
        {
            Transform child = children[i];
            pose.Add(child, (child.position, child.rotation));
            Progress.Report(taskId, (float)i / children.Length, 
                "Record origin pose...");
            yield return null;
        }
        //开始采样
        for (int i = 0; i < frames; i++)
        {
            clip.SampleAnimation(animator.gameObject, 
                (float)i / samplingRate);
            bool isFootGroundedLeft = IsFootGrounded(
                animator, HumanBodyBones.LeftFoot);
            bool isFootGroundedRight = IsFootGrounded(
                animator, HumanBodyBones.RightFoot);
            //在地面时权重为1，不在地面时权重为0
            lKeyframes[i] = new Keyframe((float)i / frames,
                isFootGroundedLeft ? 1f : 0f);
            rKeyframes[i] = new Keyframe((float)i / frames,
                isFootGroundedRight ? 1f : 0f);
            Progress.Report(taskId, (float)i / frames,
                string.Format("Sampling the frame {0}", i));
            yield return null;
        }
        //恢复初始姿态
        foreach (var kv in pose)
            kv.Key.SetPositionAndRotation(
                kv.Value.Item1, kv.Value.Item2);
        //过滤（当帧值与前帧、后帧值都一样）
        lKeyframes = KeyFramesFilter(lKeyframes, frames);
        rKeyframes = KeyFramesFilter(rKeyframes, frames);
        //添加曲线并保存、重新导入
        target.curves = target.curves.Concat(new ClipAnimationInfoCurve[2]
        {
            new ClipAnimationInfoCurve()
            {
                name = "LeftFootIK",
                curve = new AnimationCurve(lKeyframes)
            },
            new ClipAnimationInfoCurve()
            {
                name = "RightFootIK",
                curve = new AnimationCurve(rKeyframes)
            }
        }).ToArray();
        importer.clipAnimations = clipAnimations;
        importer.SaveAndReimport();
        Progress.Remove(taskId);
    }

    private bool IsFootGrounded(
        Animator animator, HumanBodyBones bone)
    {
        Transform transform = animator.GetBoneTransform(bone);
        return transform.position.y <= .15f;
    }
}

public class AxisXMovement : BakeStrategy
{
    public override IEnumerator Execute(Animator animator, AnimationClip clip)
    {
        int taskId = Progress.Start("Animation Curve Bake",
            "Baking AxisXMovement Curve", Progress.Options.None, -1);
        //获取资产路径进而获取资产导入器
        ModelImporter importer = AssetImporter.GetAtPath(
            AssetDatabase.GetAssetPath(clip)) as ModelImporter;
        //在该资产导入器中根据名称找到目标动画剪辑
        ModelImporterClipAnimation[] clipAnimations
            = importer.clipAnimations;
        ModelImporterClipAnimation target = clipAnimations
            .FirstOrDefault(m => m.name == clip.name);
        //以类名为曲线命名
        string curveName = GetType().Name;
        //如果已有同名曲线则将其筛除
        target.curves = target.curves
            .Where(m => m.name != curveName).ToArray();
        //保存并重新导入
        importer.SaveAndReimport();
        yield return null;
        //帧数等于动画剪辑的时长乘以采样率并向上取整
        int samplingRate = 30;
        int frames = Mathf.CeilToInt(clip.length * samplingRate);
        Keyframe[] keyframes = new Keyframe[frames];
        //采样之前记录初始姿态
        Dictionary<Transform, (Vector3, Quaternion)> pose
            = new Dictionary<Transform, (Vector3, Quaternion)>();
        Transform[] children = animator.GetComponentsInChildren<Transform>();
        for (int i = 0; i < children.Length; i++)
        {
            Transform child = children[i];
            pose.Add(child, (child.position, child.rotation));
            Progress.Report(taskId, (float)i / children.Length, "Record origin pose...");
            yield return null;
        }
        //开始采样
        float x = animator.GetBoneTransform(HumanBodyBones.Hips).position.x;
        for (int i = 0; i < frames; i++)
        {
            clip.SampleAnimation(animator.gameObject, (float)i / samplingRate);
            float value = animator.GetBoneTransform(HumanBodyBones.Hips).position.x;
            keyframes[i] = new Keyframe((float)i / frames, value - x);
            x = value;
            Progress.Report(taskId, (float)i / frames,
                string.Format("Sampling the frame {0}", i));
            yield return null;
        }
        //恢复初始姿态
        foreach (var kv in pose)
            kv.Key.SetPositionAndRotation(
                kv.Value.Item1, kv.Value.Item2);
        //过滤（当帧值与前帧、后帧值都一样）
        keyframes = KeyFramesFilter(keyframes, frames);
        //调整切线使曲线平滑
        //keyframes = TangentSmooth(keyframes);
        //添加曲线并保存、重新导入
        target.curves = target.curves.Concat(new ClipAnimationInfoCurve[1]
        {
            new ClipAnimationInfoCurve()
            {
                name = curveName,
                curve = new AnimationCurve(keyframes)
            }
        }).ToArray();
        importer.clipAnimations = clipAnimations;
        importer.SaveAndReimport();
        Progress.Remove(taskId);
    }
}