using UnityEngine;
using System.Collections.Generic;

public class OctreePathFinding : MonoBehaviour
{
    //区域大小
    [SerializeField] private float size = 10f;
    //最小节点大小
    [SerializeField] private float minLeafSize = 1f;
    //八叉树
    private Octree octree;

    [SerializeField] private bool drawCenter = true;
    [SerializeField] private bool drawPassable = true;
    [SerializeField] private bool drawConnections = true;

    private void Start()
    {
        octree = new Octree(transform.position, size, minLeafSize);
    }

    //根据坐标值获取所在节点
    public OctreeNode GetClosestNode(Vector3 position)
    {
        return GetClosestNodeInternal(octree.rootNode, position);
    }
    private OctreeNode GetClosestNodeInternal(
        OctreeNode node, Vector3 position)
    {
        if (node.subNodes == null)
        {
            if (new Bounds(node.center, node.nodeSize 
                * Vector3.one).Contains(position))
                return node;
        }
        else
        {
            for (int i = 0; i < node.subNodes.Length; i++)
            {
                OctreeNode leafNode = node.subNodes[i];
                if (new Bounds(leafNode.center, leafNode.nodeSize
                    * Vector3.one).Contains(position))
                {
                    OctreeNode target = GetClosestNodeInternal(
                        leafNode, position);
                    if (target != null) return target;
                    break;
                }
            }
        }
        return null;
    }

    public List<OctreeNode> GetAStarPath(
        OctreeNode startNode, OctreeNode endNode)
    {
        return (startNode != null && endNode != null)
            ? octree.AStar(startNode, endNode) : null;
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            octree.rootNode.Draw(drawCenter, drawPassable, drawConnections);
        }
    }
}