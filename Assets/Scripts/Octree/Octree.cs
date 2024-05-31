using UnityEngine;
using System.Collections.Generic;

public class Octree
{
    //根节点
    public OctreeNode rootNode;
    //叶节点集合（没有子节点的节点）
    private readonly List<OctreeNode> leavesNodes;

    public Octree(Vector3 center, float rootNodeSize, float minLeafSize)
    {
        rootNode = new OctreeNode(center, rootNodeSize, minLeafSize);
        leavesNodes = new List<OctreeNode>();
        GetLeavesNodes(rootNode);
        BuildNodeConnections();
    }

    //获取叶节点
    private void GetLeavesNodes(OctreeNode node)
    {
        //该节点没有子节点，那么它就是叶节点
        if (node.subNodes == null)
            leavesNodes.Add(node);
        else
        {
            //遍历子节点
            for (int i = 0; i < node.subNodes.Length; i++)
            {
                //递归获取叶节点
                GetLeavesNodes(node.subNodes[i]);
            }
        }
    }
    //构建叶节点间的链接
    private void BuildNodeConnections()
    {
        Vector3[] directionArray = new Vector3[22]
        {
            //前后左右上下
            new Vector3(0f, 0f, 1f), new Vector3(0f, 0f, -1f),
            new Vector3(-1f, 0f, 0f), new Vector3(1f, 0f, 0f),
            new Vector3(0f, 1f, 0f), new Vector3(0f, -1f, 0f),
            //前下、前上、后下、后上
            new Vector3(0f, -1f, 1f), new Vector3(0f, 1f, 1f),
            new Vector3(0f, -1f, -1f), new Vector3(0f, 1f, -1f),
            //右上、右下、左上、左下
            new Vector3(1f, 1f, 0f), new Vector3(1f, -1f, 0f),
            new Vector3(-1f, 1f, 0f), new Vector3(-1f, -1f, 0f),
            //右前、右后、左前、左后
            new Vector3(1f, 0f, 1f), new Vector3(1f, 0f, -1f),
            new Vector3(-1f, 0f, 1f), new Vector3(-1f, 0f, -1f),
            //斜向
            new Vector3(1f, 1f, 1f), new Vector3(-1f, 1f, 1f),
            new Vector3(1f, 1f, -1f), new Vector3(-1f, 1f, -1f),
        };
        for (int i = 0; i < leavesNodes.Count; i++)
        {
            OctreeNode currentNode = leavesNodes[i];
            List<OctreeNode> neighbourNodes = new List<OctreeNode>();
            //遍历其它叶节点，以寻找当前节点的邻节点
            for (int j = 0; j < leavesNodes.Count; j++)
            {
                if (i == j) continue;
                OctreeNode targetNode = leavesNodes[j];
                for (int k = 0; k < directionArray.Length; k++)
                {
                    Vector3 direction = directionArray[k];
                    //当前节点中心向各个方向的射线
                    Ray ray = new Ray(currentNode.center, direction);
                    //射线与以目标节点中心和大小形成的包围盒是否相交
                    if (new Bounds(targetNode.center, targetNode.nodeSize
                        * Vector3.one).IntersectRay(ray, out float distance))
                    {
                        //相交时判断距离是否小于当前节点大小的一半
                        //如果小于，将目标节点作为当前节点的邻节点
                        if (distance < currentNode.nodeSize 
                            * .5f * direction.magnitude + .01f)
                            neighbourNodes.Add(targetNode);
                    }
                }
            }
            //遍历找到的邻节点，构建链接
            for (int n = 0; n < neighbourNodes.Count; n++)
                currentNode.BuildConnection(neighbourNodes[n]);
        }
    }
    //节点间计价
    private float CalculateCost(OctreeNode node1, OctreeNode node2)
    {
        return (node2.center - node1.center).magnitude;
    }

    /// <summary>
    /// 通过A*算法获取路径
    /// </summary>
    /// <param name="startNode">起始节点</param>
    /// <param name="endNode">目标节点</param>
    /// <returns></returns>
    public List<OctreeNode> AStar(OctreeNode startNode, OctreeNode endNode)
    {
        if (endNode.isPassable == false) return null;
        //开放集合
        List<OctreeNode> openCollection = new List<OctreeNode>(); 
        //封闭集合
        List<OctreeNode> closeCollection = new List<OctreeNode>();
        //将起始节点放入开放集合
        openCollection.Add(startNode);
        //开放集合中数量为0时，寻路结束
        while (openCollection.Count > 0)
        {
            //当前节点
            OctreeNode currentNode = openCollection[0];
            //遍历查找是否有代价更小的节点
            //若代价相同，选择与目标节点代价更小的节点
            for (int i = 1; i < openCollection.Count; i++)
            {
                currentNode = (currentNode.fCost > openCollection[i].fCost
                    || (currentNode.fCost == openCollection[i].fCost
                        && currentNode.hCost > openCollection[i].hCost))
                    ? openCollection[i] : currentNode;
            }
            //将当前节点从开放集合移除并放入封闭集合
            openCollection.Remove(currentNode);
            closeCollection.Add(currentNode);
            //如果当前节点已经是目标节点，寻路结束
            if (currentNode == endNode) break;
            //遍历链接节点
            for (int i = 0; i < currentNode.connections.Count; i++)
            {
                OctreeNode connected = currentNode.connections[i];
                //判断该链接节点是否为可通信区域、该链接节点是否在封闭集合中
                if (!connected.isPassable
                    || closeCollection.Contains(connected))
                    continue;

                //经当前节点到达该链接节点的G值是否小于原来的G值
                //或者该链接节点还没有被放入开放集合，将其放入开放集合
                float gCost = currentNode.gCost 
                    + CalculateCost(currentNode, connected);
                if (gCost < connected.gCost
                    || !openCollection.Contains(connected))
                {
                    connected.gCost = gCost;
                    connected.hCost = CalculateCost(connected, endNode);
                    connected.pathParentNode = currentNode;
                    if (!openCollection.Contains(connected))
                        openCollection.Add(connected);
                }
            }
        }
        //倒序获取父节点
        List<OctreeNode> path = new List<OctreeNode>();
        OctreeNode currNode = endNode;
        while (currNode != startNode)
        {
            path.Add(currNode);
            currNode = currNode.pathParentNode;
        }
        path.Add(startNode);
        path.Reverse();
        return path;
    }
}
