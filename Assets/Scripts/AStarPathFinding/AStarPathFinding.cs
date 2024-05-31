using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class AStarPathFinding : MonoBehaviour
{
    //x、y形成地图节点网格
    [SerializeField] private int x = 10;
    [SerializeField] private int y = 10;
    //节点的大小
    [SerializeField] private float nodeSize = 1f;
    //起点偏移值，默认以原点为网格起点
    [SerializeField] private Vector3 offset;
    //存储可行走区域数据的地图
    [SerializeField] private Texture2D map;
    private AStarPathFinding_Grid grid;

    private void Start()
    {
        grid = new AStarPathFinding_Grid(x, y, map);
    }

    /// <summary>
    /// 根据坐标值获取节点
    /// </summary>
    /// <param name="position">坐标</param>
    /// <returns>节点</returns>
    public AStarPathFinding_GridNode GetClosestNode(Vector3 position)
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
    public Vector3 GetPosition(AStarPathFinding_GridNode node)
    {
        return new Vector3(
            (node.x + .5f) * nodeSize,
            0f,
            (node.y + .5f) * nodeSize) + offset;
    }
    /// <summary>
    /// 根据起始节点和终节点获取路径
    /// </summary>
    /// <param name="startNode">起始节点</param>
    /// <param name="endNode">终节点</param>
    /// <param name="searchMode">邻节点搜索方式</param>
    /// <returns>路径节点集合</returns>
    public List<AStarPathFinding_GridNode> GetPath(
        AStarPathFinding_GridNode startNode, 
        AStarPathFinding_GridNode endNode,
        AStarPathFinding_SearchMode searchMode 
            = AStarPathFinding_SearchMode.Link8)
    {
        return (startNode != null && endNode != null)
            ? grid?.GetPath(startNode, endNode, searchMode)
            : null;
    }

    /// <summary>
    /// 路径平滑
    /// </summary>
    /// <param name="path">原始路径</param>
    /// <returns>更为平滑的路径</returns>
    public List<AStarPathFinding_GridNode> Smooth(
        List<AStarPathFinding_GridNode> path)
    {
        if (path.Count <= 2) return path;
        List<AStarPathFinding_GridNode> smoothPath
            = new List<AStarPathFinding_GridNode>();
        int i, j;
        for (i = 0; i < path.Count - 1; i++)
        {
            for (j = i + 1;  j < path.Count; j++)
            {
                var currNode = path[i];
                var nextNode = path[j];
                Vector3 currPos = GetPosition(currNode);
                Vector3 nextPos = GetPosition(nextNode);
                Ray ray = new Ray(currPos, nextPos - currPos);
                if (Physics.SphereCast(ray, 0.15f,
                    (nextPos - currPos).magnitude))
                    break;
            }
            i = j - 1;
            smoothPath.Add(path[i]);
        }
        return smoothPath;
    }

#if UNITY_EDITOR
    private bool[,] walkableMap;
    [SerializeField] private bool drawIndex = true;
    private void OnDrawGizmosSelected()
    {
        if (map == null) return;
        walkableMap = new bool[x, y];
        byte[] bytes = map.GetRawTextureData();
        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
            {
                int index = i * x + j;
                walkableMap[i, j] = bytes[index] == 255;
            }
        }
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

        Handles.color = Color.red;
        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
            {
                Vector3 pos = new Vector3(i * nodeSize, 0f, j * nodeSize) + .5f * nodeSize *
                        (Vector3.forward + Vector3.right) + offset;
                if (drawIndex)
                    Handles.Label(pos, string.Format("({0},{1})", i, j));
                if (!walkableMap[i, j])
                {
                    Handles.DrawWireCube(pos, (Vector3.forward + Vector3.right) * nodeSize * .9f);
                }
            }
        }
    }
#endif
}