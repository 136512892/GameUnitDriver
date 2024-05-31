using UnityEngine;
using UnityEngine.Playables;

// A behaviour that is attached to a playable
public class MovementPlayableBehaviour : PlayableBehaviour
{
    //要移动的对象
    public Transform actor;
    //移动的目标点
    public Vector3 targetPosition;
    //起始点
    private Vector3 beginPosition;

    // Called when the owning graph starts playing
    public override void OnGraphStart(Playable playable) 
    {

    }
    // Called when the owning graph stops playing
    public override void OnGraphStop(Playable playable) 
    {

    }

    public override void OnPlayableCreate(Playable playable)
    {

    }

    public override void OnPlayableDestroy(Playable playable)
    {

    }

    // Called when the state of the playable is set to Play
    public override void OnBehaviourPlay(Playable playable, FrameData info) 
    {
        //开始播放 记录起始点
        if (actor != null)
            beginPosition = actor.position;
    }

    // Called when the state of the playable is set to Paused
    public override void OnBehaviourPause(Playable playable, FrameData info) 
    {
        //播放结束 回到起始点
        if (actor != null)
            actor.position = beginPosition;
    }

    public override void PrepareData(Playable playable, FrameData info)
    {

    }

    // Called each frame while the state is set to Play
    public override void PrepareFrame(Playable playable, FrameData info) 
    {

    }

    public override void ProcessFrame(
        Playable playable, FrameData info, object playerData)
    {
        //播放过程插值计算当前位置
        if (actor != null)
        {
            float duration = (float)playable.GetDuration();
            float time = (float)playable.GetTime();
            float lerpPct = time / duration;
            actor.position = Vector3.Lerp(
                beginPosition, targetPosition, lerpPct);
        }
    }
}
