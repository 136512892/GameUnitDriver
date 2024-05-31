/// <summary>
/// 网格节点
/// </summary>
public class AStarPathFinding_GridNode
{
    public int x;
    public int y;
    /// <summary>
    /// 父节点
    /// </summary>
    public AStarPathFinding_GridNode parent;
    /// <summary>
    /// 是否为可行走区域
    /// </summary>
    public bool isWalkable;
    /// <summary>
    /// 起始节点到当前节点的代价
    /// </summary>
    public int gCost;
    /// <summary>
    /// 当前节点到终节点的代价
    /// </summary>
    public int hCost;
    /// <summary>
    /// 代价
    /// </summary>
    public int Cost
    {
        get
        {
            return gCost + hCost;
        }
    }

    public AStarPathFinding_GridNode(int x, int y, bool isWalkable)
    {
        this.x = x;
        this.y = y;
        this.isWalkable = isWalkable;
    }
}