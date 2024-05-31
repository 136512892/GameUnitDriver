using UnityEngine;

public abstract class AvatarAction
{
    protected AvatarController controller;
    protected float raycastHeight;
    protected float raycastDistance;
    protected float actionMaxDistance;
    protected string targetTag;
    protected Vector3 startPosition, targetPosition;
    protected Quaternion startRotation, targetRotation;
    protected float timer;
    protected float actionDuration;

    public AvatarAction(AvatarController controller, 
        AvatarActionSettings settings)
    {
        this.controller = controller;
        raycastHeight = settings.raycastHeight;
        raycastDistance = settings.raycastDistance;
        targetTag = settings.targetTag;
        actionDuration = settings.actionDuration;
        actionMaxDistance = settings.actionMaxDistance;
    }

    public abstract bool ActionDoableCheck();
    public abstract bool ActionExecute();
    public virtual void OnAnimatorIK(int layerIndex) { }
    public virtual void OnDrawGizmos() { }
}