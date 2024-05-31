using UnityEngine;

public class FlowFieldPathFinding_GridNode
{
    public int x;
    public int y;
    public int cost;
    public int fCost;
    public Vector3 direction;
    public bool isWalkable;

    public FlowFieldPathFinding_GridNode(int x, int y, bool isWalkable)
    {
        this.x = x;
        this.y = y;
        this.isWalkable = isWalkable;
        cost = isWalkable ? 10 : int.MaxValue;
        fCost = int.MaxValue;
    }
}