using UnityEngine;

[CreateAssetMenu]
public class AvatarActionSettings : ScriptableObject
{
    public float raycastHeight;
    public float raycastDistance;
    public string targetTag;
    public float actionDuration;
    public float actionMaxDistance;
}