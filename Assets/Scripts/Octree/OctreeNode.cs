using UnityEngine;
using System.Collections.Generic;

public class OctreeNode
{
    public Vector3 center; //节点中心
    public float nodeSize; //节点大小
    public OctreeNode[] subNodes; //子节点
    private readonly float minNodeSize;
    public bool isPassable; //该节点是否是可通行的
    public List<OctreeNode> connections; //链接节点集合

    public float fCost, gCost, hCost;
    public OctreeNode pathParentNode;

    public OctreeNode(Vector3 center, float nodeSize, float minNodeSize)
    {
        this.center = center;
        this.nodeSize = nodeSize;
        this.minNodeSize = minNodeSize;

        //子节点大小等于当前节点大小的1/2
        float subNodeSize = nodeSize * .5f;
        //盒体重叠检测，如果未检测到任何碰撞器，表示该节点是可通行的
        isPassable = Physics.OverlapBox(center, 
            nodeSize * .5f * Vector3.one).Length == 0;
        //如果该区域内没有任何碰撞器，不需要继续划分
        //如果有碰撞，且子节点大小大于最小节点大小，继续划分
        if (!isPassable && subNodeSize > minNodeSize)
        {
            //子节点中心坐标会在各坐标轴上偏移当前节点大小的1/4
            float quarter = nodeSize * .25f;
            subNodes = new OctreeNode[8];
            int index = -1;
            for (int x = -1; x <= 1; x += 2)
            {
                for (int y = -1; y <= 1; y += 2)
                {
                    for (int z = -1; z <= 1; z += 2)
                    {
                        subNodes[++index] = new OctreeNode(
                            center + quarter * new Vector3(x, y, z),
                            subNodeSize, minNodeSize);
                    }
                }
            }
        }
    }

    //构建与指定节点的链接
    public void BuildConnection(OctreeNode node)
    {
        connections ??= new List<OctreeNode>();
        if (node == null) return;
        if (connections.Contains(node)) return;
        if (!isPassable || !node.isPassable) return;
        //通过球体投射检测方法，检测两个节点间是否有碰撞器
        Vector3 direction = node.center - center;
        if (Physics.SphereCast(center, minNodeSize * .5f,
            direction, out _, direction.magnitude)) return;
        connections.Add(node);
    }

    public void Draw(bool drawCenter, bool drawPassable, bool drawConnections)
    {
        if (subNodes != null)
        {
            for (int i = 0; i < subNodes.Length; i++)
            {
                subNodes[i].Draw(drawCenter, drawPassable, drawConnections);
            }
        }
        else
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(center, nodeSize * Vector3.one);
            if (drawPassable && !isPassable)
            {
                Color color = Color.red;
                color.a = 0.35f;
                Gizmos.color = color;
                Gizmos.DrawCube(center, nodeSize * Vector3.one);
            }
            if (drawCenter)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(center, minNodeSize * .2f);
            }
            if (drawConnections)
            {
                if (connections != null)
                {
                    Gizmos.color = Color.cyan;
                    for (int i = 0; i < connections.Count; i++)
                        Gizmos.DrawLine(center, connections[i].center);
                }
            }
        }
    }
}