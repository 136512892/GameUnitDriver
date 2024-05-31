using UnityEngine;
using System.Collections.Generic;

public class AStarPathFindingAgent : MonoBehaviour
{
    [SerializeField] private AStarPathFinding aStar;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float rotateSpeed = 10f;
    [Tooltip("应小于网格节点大小")]
    [SerializeField] private float stopDistance = .1f;
    private List<AStarPathFinding_GridNode> path;
    private bool isStopped = true;
    public bool smooth;

    public bool IsStopped
    {
        get { return isStopped; }
        set
        {
            isStopped = value;
            if (!isStopped)
            {
                path.Clear();
                path = null;
            }
        }
    }
    public Vector3 Destination { get; private set; }

    /// <summary>
    /// 设置寻路目的地
    /// </summary>
    /// <param name="destination">目的地</param>
    public void SetDestination(Vector3 destination)
    {
        var targetNode = aStar.GetClosestNode(destination);
        if (targetNode.isWalkable)
        {
            isStopped = false;
            Destination = destination;
            var startNode = aStar.GetClosestNode(transform.position);
            path = aStar.GetPath(startNode, targetNode);
            if (smooth)
                path = aStar.Smooth(path);
        }
    }

    private void Update()
    {
        if (!isStopped && path != null && path.Count > 0)
        {
            var node = path[0];
            //最终节点移动至目的地
            //不是最终节点移动至节点中心
            Vector3 des = path.Count != 1
                ? aStar.GetPosition(node)
                : Destination;
            //与目的地的距离大于停止距离
            if (Vector3.Distance(transform.position, des) > stopDistance)
            {
                //移动方向
                Vector3 direction = (des - transform.position).normalized;
                transform.position += Time.deltaTime * moveSpeed * direction;
                transform.rotation = Quaternion.Lerp(transform.rotation,
                    Quaternion.LookRotation(direction),
                    Time.deltaTime * rotateSpeed);
            }
            else
            {
                path.RemoveAt(0);
                isStopped = path.Count == 0;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
#if UNITY_EDITOR
        if (path != null && path.Count > 1)
        {
            UnityEditor.Handles.DrawLine(transform.position,
                    aStar.GetPosition(path[0]), 3f);
            for (int i = 0; i < path.Count - 1; i++)
            {
                UnityEditor.Handles.DrawLine(aStar.GetPosition(path[i]),
                    aStar.GetPosition(path[i + 1]), 3f);
            }
            UnityEditor.Handles.Label(Destination, 
                "目的地", new GUIStyle(GUI.skin.label)
            {
                fontSize = 20,
                fontStyle = FontStyle.Bold
            });
#endif
        }
    }
}