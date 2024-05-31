using UnityEngine;

public class FlowFieldPathFindingAgent : MonoBehaviour
{
    [SerializeField] private FlowFieldPathFinding flowField;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float rotateSpeed = 10f;

    public bool isPathFinding;

    private void Update()
    {
        if (flowField != null && flowField.TargetNode != null)
        {
            var node = flowField.GetClosestNode(transform.position);
            if (node != null && node.direction != Vector3.zero)
            {
                isPathFinding = true;

                transform.position += Time.deltaTime
                        * moveSpeed * node.direction;
                    transform.rotation = Quaternion.Lerp(
                        transform.rotation,
                        Quaternion.LookRotation(node.direction),
                        Time.deltaTime * rotateSpeed);
            }
            else isPathFinding = false;
        }
    }
}