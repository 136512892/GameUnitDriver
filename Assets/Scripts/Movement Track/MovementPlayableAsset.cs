using UnityEngine;
using UnityEngine.Playables;

[System.Serializable]
public class MovementPlayableAsset : PlayableAsset
{
    public ExposedReference<Transform> actor;
    public Vector3 targetPosition;

    // Factory method that generates a playable based on this asset
    public override Playable CreatePlayable(
        PlayableGraph graph, GameObject go)
    {
        var behaviour = new MovementPlayableBehaviour()
        {
            actor = actor.Resolve(graph.GetResolver()),
            targetPosition = targetPosition
        };
        return ScriptPlayable<MovementPlayableBehaviour>
            .Create(graph, behaviour);
    }
}