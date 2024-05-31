using UnityEngine;

public class AStarPathFindingExample : MonoBehaviour
{
    private Animator animator;
    private AStarPathFindingAgent agent;
    private void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<AStarPathFindingAgent>();
    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hitInfo))
                agent.SetDestination(hitInfo.point);
        }
        animator.SetBool("Move", !agent.IsStopped);
    }
}