using UnityEngine;
using UnityEngine.Timeline;

[TrackColor(1f, 0f, 1f)]
[TrackBindingType(typeof(GameObject))]
[TrackClipType(typeof(MovementPlayableAsset))]
public class MovementTrack : TrackAsset { }