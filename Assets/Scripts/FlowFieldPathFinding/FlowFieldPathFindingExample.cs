using UnityEngine;

public class FlowFieldPathFindingExample : MonoBehaviour
{
    private FlowFieldPathFindingAgent agent;
    private Animator animator;
    private void Start()
    {
        agent = GetComponent<FlowFieldPathFindingAgent>();
        animator = GetComponent<Animator>();
    }
    private void Update()
    {
        animator.SetBool("Move", agent.isPathFinding);
    }
}
