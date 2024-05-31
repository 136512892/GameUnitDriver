using System;
using UnityEngine;
using System.Collections.Generic;

public class FlowFieldPathFinding_Grid
{
    private readonly int x;
    private readonly int y;
    private readonly Dictionary<int, FlowFieldPathFinding_GridNode> nodesDic
        = new Dictionary<int, FlowFieldPathFinding_GridNode>();

    public FlowFieldPathFinding_Grid(int x, int y, bool[,] map)
    {
        this.x = x;
        this.y = y;
        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
            {
                int index = i * x + j;
                nodesDic.Add(index, 
                    new FlowFieldPathFinding_GridNode(i, j, map[i, j]));
            }
        }
    }
    public FlowFieldPathFinding_Grid(int x, int y, Texture2D map)
    {
        this.x = x;
        this.y = y;
        byte[] bytes = map.GetRawTextureData();
        if (bytes.Length != x * y)
            throw new ArgumentOutOfRangeException();
        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
            {
                int index = i * x + j;
                nodesDic.Add(index,
                    new FlowFieldPathFinding_GridNode(
                        i, j, bytes[index] == 255));
            }
        }
    }

    //计算两个节点之间的代价
    private int CalculateCost(FlowFieldPathFinding_GridNode node1,
        FlowFieldPathFinding_GridNode node2)
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
    public List<FlowFieldPathFinding_GridNode> GetNeighbouringNodes(
        FlowFieldPathFinding_GridNode node, FlowFieldPathFinding_SearchMode searchMode)
    {
        List<FlowFieldPathFinding_GridNode> neighbours
            = new List<FlowFieldPathFinding_GridNode>();
        switch (searchMode)
        {
            case FlowFieldPathFinding_SearchMode.Link4:
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
            case FlowFieldPathFinding_SearchMode.Link8:
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
    public FlowFieldPathFinding_GridNode GetNode(int x, int y)
    {
        return (x >= 0 && x < this.x && y >= 0 && y < this.y)
            ? nodesDic[x * this.y + y] : null;
    }
    /// <summary>
    /// 设置目标节点
    /// </summary>
    /// <param name="target">目标节点</param>
    public void SetTarget(FlowFieldPathFinding_GridNode target)
    {
        foreach (var node in nodesDic.Values)
        {
            node.cost = node.isWalkable ? 10 : int.MaxValue;
            node.fCost = int.MaxValue;
        }
        target.cost = 0;
        target.fCost = 0;
        target.direction = Vector3.zero;
        Queue<FlowFieldPathFinding_GridNode> queue 
            = new Queue<FlowFieldPathFinding_GridNode>();
        queue.Enqueue(target);
        while (queue.Count > 0)
        {
            FlowFieldPathFinding_GridNode currentNode = queue.Dequeue();
            List<FlowFieldPathFinding_GridNode> neighbourNodes = 
                GetNeighbouringNodes(currentNode, 
                    FlowFieldPathFinding_SearchMode.Link8);
            for (int i = 0; i < neighbourNodes.Count; i++)
            {
                FlowFieldPathFinding_GridNode neighbourNode = neighbourNodes[i];
                if (neighbourNode.cost == int.MaxValue) continue;
                neighbourNode.cost = CalculateCost(neighbourNode, currentNode);
                if (neighbourNode.cost + currentNode.fCost < neighbourNode.fCost)
                {
                    neighbourNode.fCost = neighbourNode.cost + currentNode.fCost;
                    queue.Enqueue(neighbourNode);
                }
            }
        }
    }
    /// <summary>
    /// 生成流场
    /// </summary>
    /// <param name="target">目标节点</param>
    public void GenerateFlowField(FlowFieldPathFinding_GridNode target)
    {
        foreach (var node in nodesDic.Values)
        {
            List<FlowFieldPathFinding_GridNode> neighbourNodes =
                GetNeighbouringNodes(node, 
                    FlowFieldPathFinding_SearchMode.Link8);
            int fCost = node.fCost;
            FlowFieldPathFinding_GridNode temp = null;
            for (int i = 0;i < neighbourNodes.Count;i++)
            {
                FlowFieldPathFinding_GridNode neighbourNode = neighbourNodes[i];
                if (neighbourNode.fCost < fCost)
                {
                    temp = neighbourNode;
                    fCost = neighbourNode.fCost;
                    node.direction = new Vector3(
                        neighbourNode.x - node.x, 0, neighbourNode.y - node.y);
                }
                else if (neighbourNode.fCost == fCost && temp != null)
                {
                    if (CalculateCost(neighbourNode, target) <
                        CalculateCost(temp, target))
                    {
                        temp = neighbourNode;
                        fCost = neighbourNode.fCost;
                        node.direction = new Vector3(
                            neighbourNode.x - node.x, 0, neighbourNode.y - node.y);
                    }
                }
            }
        }
    }
}
