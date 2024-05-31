using UnityEngine;
using System.Collections.Generic;

public class OctreePathFindingExample : MonoBehaviour
{
    [SerializeField] private OctreePathFinding pathFinding;

    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float rotateSpeed = 10f;
    private List<OctreeNode> path;

    private void Update()
    {
        if (path != null && path.Count > 0)
        {
            var node = path[0];
            if (Vector3.Distance(transform.position,
                node.center) > .05f)
            {
                Vector3 direction = node.center - transform.position;
                transform.position += Time.deltaTime 
                    * moveSpeed * direction.normalized;
                transform.rotation = Quaternion.Lerp(transform.rotation,
                    Quaternion.LookRotation(direction.normalized),
                    Time.deltaTime * rotateSpeed);
            }
            else
            {
                path.RemoveAt(0);
            }
        }
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Path Finding Example", GUILayout.Width(200f), GUILayout.Height(50f)))
        {
            OctreeNode startNode = pathFinding
                .GetClosestNode(transform.position);
            Vector3 randomPosition = new Vector3(
                    Random.Range(-10f, 10f),
                    Random.Range(-10f, 10f),
                    Random.Range(-10f, 10f));
            OctreeNode endNode = pathFinding
                .GetClosestNode(randomPosition);
            Debug.Log(string.Format("Position：{0}", randomPosition));
            if (endNode != null)
                Debug.Log(string.Format("IsPassable：{0}", endNode.isPassable));
            path = pathFinding.GetAStarPath(startNode, endNode);
            if (path != null)
                Debug.Log(path.Count);
        }
    }

    private void OnDrawGizmos()
    {
        if (path != null && path.Count > 1)
        {
            Gizmos.color = Color.yellow;
            for (int i = 0; i < path.Count - 1; i++)
            {
                Gizmos.DrawLine(path[i].center, path[i + 1].center);
            }
        }
    }
}
