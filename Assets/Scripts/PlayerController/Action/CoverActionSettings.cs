using UnityEngine;

[CreateAssetMenu]
public class CoverActionSettings : AvatarActionSettings
{
    public Vector3 boxCastSize = Vector3.one * .4f;
    public float sneakSpeed = 1f;
    public float directionLerpSpeed = 3f;
    public float headRadius = .12f;
    public int headDownCastCountLimit = 3;
    public float bodyPositionLerpSpeed = 0.05f;
    public float footPositionLerpSpeed = 0.15f;
}