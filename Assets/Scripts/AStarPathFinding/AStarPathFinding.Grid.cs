using System;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 网格
/// </summary>
public class AStarPathFinding_Grid
{
    private readonly int x;
    private readonly int y;
    private readonly Dictionary<int, AStarPathFinding_GridNode> nodesDic;

    public AStarPathFinding_Grid(int x, int y, bool[,] map)
    {
        this.x = x;
        this.y = y;
        nodesDic = new Dictionary<int, AStarPathFinding_GridNode>();
        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
            {
                int index = i * x + j;
                nodesDic.Add(index, new AStarPathFinding_GridNode(
                    i, j, map[i, j]));
            }
        }
    }
    public AStarPathFinding_Grid(int x, int y, Texture2D map)
    {
        this.x = x;
        this.y = y;
        nodesDic = new Dictionary<int, AStarPathFinding_GridNode>();
        byte[] bytes = map.GetRawTextureData();
        if (bytes.Length != x * y)
            throw new ArgumentOutOfRangeException();
        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
            {
                int index = i * x + j;
                nodesDic.Add(index, new AStarPathFinding_GridNode(
                    i, j, bytes[index] == 255));
            }
        }
    }

    //计算两个节点之间的代价
    private int CalculateCost(AStarPathFinding_GridNode node1,
        AStarPathFinding_GridNode node2)
    {
        //取绝对值
        int deltaX = node1.x - node2.x;
        if (deltaX < 0) deltaX = -deltaX;
        int deltaY = node1.y - node2.y;
        if (deltaY < 0) deltaY = -deltaY;
        int delta = deltaX - deltaY;
        if (delta < 0) delta = -delta;
        //每向上、下、左、右方向移动一个节点代价增加10
        //每斜向移动一个节点代价增加14（勾股定理，精确来说是近似14.14~）
        return 14 * (deltaX > deltaY ? deltaY : deltaX) + 10 * delta;
    }
    //获取指定节点的邻节点
    public List<AStarPathFinding_GridNode> GetNeighbouringNodes(
        AStarPathFinding_GridNode node, AStarPathFinding_SearchMode searchMode)
    {
        List<AStarPathFinding_GridNode> neighbours = 
            new List<AStarPathFinding_GridNode>();
        switch (searchMode)
        {
            case AStarPathFinding_SearchMode.Link4:
                for (int i = -1; i <= 1; i++)
                {
                    if (i == 0) continue;
                    int x = node.x + i;
                    if (x >= 0 && x < this.x)
                        neighbours.Add(nodesDic[x * this.x + node.y]);
                    int y = node.y + i;
                    if (y >= 0 && y < this.y)
                        neighbours.Add(nodesDic[node.x * this.x + y]);
                }
                break;
            case AStarPathFinding_SearchMode.Link8:
                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        if (i == 0 && j == 0) continue;
                        int x = node.x + i;
                        int y = node.y + j;
                        if (x >= 0 && x < this.x && y >= 0 && y < this.y)
                            neighbours.Add(nodesDic[x * this.x + y]);
                    }
                }
                break;
        }
        return neighbours;
    }

    /// <summary>
    /// 根据索引获取节点
    /// </summary>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    /// <returns>节点</returns>
    public AStarPathFinding_GridNode GetNode(int x, int y)
    {
        return (x >= 0 && x < this.x && y >= 0 && y < this.y)
            ? nodesDic[x * this.y + y] : null;
    }

    /// <summary>
    /// 根据起始节点和终节点获取路径
    /// </summary>
    /// <param name="startNode">起始节点</param>
    /// <param name="endNode">终节点</param>
    /// <param name="searchMode">邻节点的搜索方式 四连通/八连通</param>
    /// <returns>路径节点集合</returns>
    public List<AStarPathFinding_GridNode> GetPath(
        AStarPathFinding_GridNode startNode, 
        AStarPathFinding_GridNode endNode, 
        AStarPathFinding_SearchMode searchMode)
    {
        if (!endNode.isWalkable) return null;
        //开放集合
        List<AStarPathFinding_GridNode> openCollection 
            = new List<AStarPathFinding_GridNode>();
        //封闭集合
        List<AStarPathFinding_GridNode> closeCollection
            = new List<AStarPathFinding_GridNode>();
        //起始节点放入开放集合
        openCollection.Add(startNode);
        //开放集合中数量为0时 寻路结束
        while (openCollection.Count > 0)
        {
            //当前节点
            AStarPathFinding_GridNode currentNode = openCollection[0];
            //遍历查找是否有代价更小的节点
            //若代价相同，选择移动到终点代价更小的节点
            for (int i = 1; i < openCollection.Count; i++)
            {
                currentNode = (currentNode.Cost > openCollection[i].Cost
                    || (currentNode.Cost == openCollection[i].Cost
                        && currentNode.hCost > openCollection[i].hCost))
                    ? openCollection[i] : currentNode;
            }
            //将获取到的当前节点从开放集合移除并放入封闭集合
            openCollection.Remove(currentNode);
            closeCollection.Add(currentNode);
            //当前节点已经是终节点，寻路结束
            if (currentNode == endNode)
                break;
            //获取邻节点
            List<AStarPathFinding_GridNode> neighbourNodes 
                = GetNeighbouringNodes(currentNode, searchMode);
            //在当前节点向邻节点继续搜索
            for (int i = 0; i < neighbourNodes.Count; i++)
            {
                AStarPathFinding_GridNode neighbourNode = neighbourNodes[i];
                //判断邻节点是否为不可行走区域(障碍)或者邻节点已经在封闭集合中
                if (!neighbourNode.isWalkable 
                    || closeCollection.Contains(neighbourNode))
                    continue;

                //经当前节点到达该邻节点的G值是否小于原来的G值
                //或者该邻节点还没有放入开放集合，将其放入开放集合
                int gCost = currentNode.gCost 
                    + CalculateCost(currentNode, neighbourNode);
                if (gCost < neighbourNode.gCost 
                    || !openCollection.Contains(neighbourNode))
                {
                    neighbourNode.gCost = gCost;
                    neighbourNode.hCost = CalculateCost(neighbourNode, endNode);
                    neighbourNode.parent = currentNode;
                    if (!openCollection.Contains(neighbourNode))
                        openCollection.Add(neighbourNode);
                }
            }
        }
        //倒序获取父节点
        List<AStarPathFinding_GridNode> path 
            = new List<AStarPathFinding_GridNode>();
        AStarPathFinding_GridNode currNode = endNode;
        while (currNode != startNode)
        {
            path.Add(currNode);
            currNode = currNode.parent;
        }
        //再次倒序后得到完整路径
        path.Reverse();
        return path;
    }
}
