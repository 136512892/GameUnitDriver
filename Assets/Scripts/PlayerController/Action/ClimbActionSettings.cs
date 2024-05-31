using UnityEngine;

[CreateAssetMenu]
public class ClimbActionSettings : AvatarActionSettings
{
    public Vector3 boxCastSize = new Vector3(.45f, .1f, .1f);
    public int boxCastCountLimit = 4;
    public float disBetweenArms = .52f;
    public float handSize = .1f;
    public Vector3 bodyOffset = new Vector3(0f, .55f, -.1f);
    public AnimationCurve idle2HangCurve = new AnimationCurve();
    public AnimationCurve dropCurve = new AnimationCurve();
    public float dropDuration, hopUpDuration, 
        hopDownDuration, hopHorizontalDuration;

}