using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class FlowFieldPathFinding : MonoBehaviour
{
    [SerializeField] private int x = 10;
    [SerializeField] private int y = 10;
    [SerializeField] private float nodeSize = 1f;
    //起点偏移值 默认以原点为网格起点
    [SerializeField] private Vector3 offset;
    //存储可行走区域数据的地图
    [SerializeField] private Texture2D map;
    private FlowFieldPathFinding_Grid grid;
    public FlowFieldPathFinding_GridNode TargetNode { get; private set; }

    private void Start()
    {
        grid = new FlowFieldPathFinding_Grid(x, y, map);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
                SetDestination(hit.point);
        }
    }

    /// <summary>
    /// 根据坐标值获取节点
    /// </summary>
    /// <param name="position">坐标</param>
    /// <returns>节点</returns>
    public FlowFieldPathFinding_GridNode GetClosestNode(Vector3 position)
    {
        return grid?.GetNode(
            Mathf.FloorToInt((position.x - offset.x) / nodeSize),
            Mathf.FloorToInt((position.z - offset.z) / nodeSize));
    }
    /// <summary>
    /// 根据节点获取坐标值（节点中心坐标）
    /// </summary>
    /// <param name="node">节点</param>
    /// <returns>节点对应的坐标点</returns>
    public Vector3 GetPosition(FlowFieldPathFinding_GridNode node)
    {
        return new Vector3(
            (node.x + .5f) * nodeSize,
            0f,
            (node.y + .5f) * nodeSize) + offset;
    }
    /// <summary>
    /// 设置目的地
    /// </summary>
    /// <param name="destination"></param>
    public void SetDestination(Vector3 destination)
    {
        var node = GetClosestNode(destination);
        if (node.isWalkable)
        {
            TargetNode = node;
            grid.SetTarget(node);
            grid.GenerateFlowField(node);
        }
    }

    public FlowFieldPathFinding_GridNode GetNode(int x, int y)
    {
        return grid.GetNode(x, y);
    }

#if UNITY_EDITOR
    [SerializeField] private bool drawGrid = true;
    [SerializeField] private bool drawCost = true;
    [SerializeField] private bool drawFlowField = true;
    private void OnDrawGizmos()
    {
        if (drawGrid)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i <= x; i++)
            {
                Vector3 start = i * nodeSize * Vector3.right;
                Vector3 end = start + y * nodeSize * Vector3.forward;
                Gizmos.DrawLine(start + offset, end + offset);
            }
            for (int i = 0; i <= y; i++)
            {
                Vector3 start = i * nodeSize * Vector3.forward;
                Vector3 end = start + x * nodeSize * Vector3.right;
                Gizmos.DrawLine(start + offset, end + offset);
            }
        }

        if (grid != null)
        {
            for (int i = 0; i < x; i++) 
            {
                for (int j = 0; j < y; j++)
                {
                    var node = grid.GetNode(i, j);
                    if (node.isWalkable)
                    {
                        Vector3 pos = GetPosition(node);
                        if (drawCost)
                            Handles.Label(pos, node.fCost.ToString());
                        //Handles.Label(pos, node.direction.ToString());
                        if (drawFlowField)
                        {
                            if (node.direction != Vector3.zero)
                            {
                                Handles.ArrowHandleCap(
                                    0,
                                    pos,
                                    Quaternion.LookRotation(node.direction),
                                    nodeSize * .75f,
                                    EventType.Repaint);
                            }
                        }
                    }
                }
            }
        }
        if (TargetNode != null)
            Handles.Label(GetPosition(TargetNode), "目的地");
    }
#endif
}