using System;
using System.Linq;

using UnityEngine;
using UnityEditor;
using Unity.EditorCoroutines.Editor;

public class AnimationCurveBaker : EditorWindow
{
    [MenuItem("Example/Animation Curve Baker")]
    public static void Open()
    {
        GetWindow<AnimationCurveBaker>().Show();
    }

    private Vector2 scroll;
    private int currentClipIndex;
    private string[] strategyNames;
    private int currentStrategyIndex;

    private void OnEnable()
    {
        strategyNames = typeof(BakeStrategy).Assembly
            .GetTypes()
            .Where(m => m.IsSubclassOf(typeof(BakeStrategy)))
            .Select(m => m.Name)
            .ToArray();
    }

    private void OnSelectionChange()
    {
        currentClipIndex = 0;
        Repaint();
    }

    private void OnGUI()
    {
        GameObject selectedGo = Selection.activeGameObject;
        if (selectedGo == null)
        {
            EditorGUILayout.HelpBox("未选中任何对象", MessageType.Warning);
            return;
        }
        Animator animator = selectedGo.GetComponent<Animator>();
        if (animator == null || animator.runtimeAnimatorController == null)
        {
            EditorGUILayout.HelpBox("动画状态机不存在", MessageType.Warning);
            return;
        }
        AnimationClip[] clips = animator
            .runtimeAnimatorController.animationClips;
        if (clips.Length == 0)
        {
            EditorGUILayout.HelpBox("Animator中的动画片段数量为0",
                MessageType.Warning);
            return;
        }
        scroll = GUILayout.BeginScrollView(scroll);
        for (int i = 0; i < clips.Length; i++)
        {
            AnimationClip clip = clips[i];
            GUILayout.Label(clip.name, currentClipIndex == i
                ? "MeTransitionSelectHead" : "ProjectBrowserHeaderBgTop",
                GUILayout.Height(22f));
            if (Event.current.type == EventType.MouseDown &&
                GUILayoutUtility.GetLastRect()
                    .Contains(Event.current.mousePosition))
            {
                Event.current.Use();
                currentClipIndex = i;
            }
        }
        GUILayout.EndScrollView();

        GUILayout.FlexibleSpace();
        GUILayout.BeginHorizontal();
        currentStrategyIndex = EditorGUILayout.Popup(
            currentStrategyIndex, strategyNames);
        //if (GUILayout.Button("Bake"))
        if (GUILayout.Button("Bake", GUILayout.Width(50f)))
            {
            Type targetType = Type.GetType(
                strategyNames[currentStrategyIndex], true);
            var strategy = Activator.CreateInstance(
                targetType) as BakeStrategy;
            EditorCoroutineUtility.StartCoroutine(
                strategy.Execute(animator, clips[currentClipIndex]), this);
        }
        GUILayout.EndHorizontal();
    }
}