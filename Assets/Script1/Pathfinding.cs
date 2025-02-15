using System.Collections.Generic;
using UnityEngine;

public static class Pathfinding
{
    /// <summary>
    /// 计算从起点到终点的路径（返回一系列格子组成的路径）
    /// </summary>
    public static List<GridCell> FindPath(GridCell start, GridCell goal)
    {
        if (start == null || goal == null) return null;

        List<PathNode> openList = new List<PathNode>();
        HashSet<GridCell> closedSet = new HashSet<GridCell>();

        PathNode startNode = new PathNode(start, null, 0, GetHeuristic(start, goal));
        openList.Add(startNode);

        while (openList.Count > 0)
        {
            // 选择 fCost 最低的节点
            PathNode currentNode = openList[0];
            for (int i = 1; i < openList.Count; i++)
            {
                if (openList[i].fCost < currentNode.fCost ||
                   (openList[i].fCost == currentNode.fCost && openList[i].hCost < currentNode.hCost))
                {
                    currentNode = openList[i];
                }
            }

            openList.Remove(currentNode);
            closedSet.Add(currentNode.cell);

            if (currentNode.cell == goal)
                return RetracePath(currentNode);

            foreach (GridCell neighbor in currentNode.cell.parentMap.GetNeighbors(currentNode.cell))
            {
                if (neighbor == null || neighbor.cellState != GridCellState.Movable || (neighbor.occupant != null && neighbor != goal)) // 目标格除外
                    continue;

                if (closedSet.Contains(neighbor))
                    continue;

                int newCost = currentNode.gCost + 1; // 假设每步消耗 1
                PathNode neighborNode = openList.Find(n => n.cell == neighbor);
                if (neighborNode == null)
                {
                    neighborNode = new PathNode(neighbor, currentNode, newCost, GetHeuristic(neighbor, goal));
                    openList.Add(neighborNode);
                }
                else if (newCost < neighborNode.gCost)
                {
                    neighborNode.gCost = newCost;
                    neighborNode.parent = currentNode;
                }
            }
        }
        return null; // 无路径
    }

    private static List<GridCell> RetracePath(PathNode endNode)
    {
        List<GridCell> path = new List<GridCell>();
        PathNode current = endNode;
        while (current != null)
        {
            path.Add(current.cell);
            current = current.parent;
        }
        path.Reverse();
        return path;
    }

    /// <summary>
    /// 使用曼哈顿距离作为启发式函数
    /// </summary>
    private static int GetHeuristic(GridCell a, GridCell b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    /// <summary>
    /// A* 节点辅助类
    /// </summary>
    private class PathNode
    {
        public GridCell cell;
        public PathNode parent;
        public int gCost;
        public int hCost;
        public int fCost { get { return gCost + hCost; } }

        public PathNode(GridCell cell, PathNode parent, int gCost, int hCost)
        {
            this.cell = cell;
            this.parent = parent;
            this.gCost = gCost;
            this.hCost = hCost;
        }
    }
}