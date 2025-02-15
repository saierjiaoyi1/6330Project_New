using UnityEngine;
using System.Collections.Generic;

public class EnemyCharacter : BaseCharacter
{
    public override void OnTurnStart()
    {
        base.OnTurnStart();
        Debug.Log("敌人回合开始。");

        // 敌人简单逻辑：寻找最近的玩家，并向其靠近
        PlayerCharacter target = FindNearestPlayer();
        if (target != null)
        {
            // 在已标记为 Movable 的格子中，选出一个离目标玩家最近的格子
            GridCell destination = FindBestDestinationTowards(target);
            if (destination != null)
            {
                List<GridCell> path = Pathfinding.FindPath(currentCell, destination);
                if (path != null && path.Count > 0)
                {
                    MoveAlongPath(path);
                    return; // 在移动协程结束后会自动结束回合
                }
            }
        }

        // 移动结束后直接结束回合
        EndTurn();
    }

    /// <summary>
    /// 寻找场景中距离本敌人最近的玩家
    /// </summary>
    private PlayerCharacter FindNearestPlayer()
    {
        PlayerCharacter[] players = FindObjectsOfType<PlayerCharacter>();
        PlayerCharacter nearest = null;
        float minDist = float.MaxValue;
        foreach (PlayerCharacter player in players)
        {
            float dist = Vector3.Distance(currentCell.transform.position, player.currentCell.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = player;
            }
        }
        return nearest;
    }

    /// <summary>
    /// 在可移动范围内寻找一个离目标玩家最近的格子作为目的地
    /// </summary>
    private GridCell FindBestDestinationTowards(PlayerCharacter target)
    {
        GridMapManager gridMap = currentCell?.parentMap;
        if (gridMap == null || gridMap.gridCells == null) return null;

        GridCell best = null;
        float bestDist = float.MaxValue;
        // 遍历所有格子，选择那些状态为 Movable 的
        for (int x = 0; x < gridMap.gridWidth; x++)
        {
            for (int y = 0; y < gridMap.gridHeight; y++)
            {
                GridCell cell = gridMap.gridCells[x, y];
                if (cell.cellState == GridCellState.Movable)
                {
                    float dist = Vector3.Distance(cell.transform.position, target.currentCell.transform.position);
                    if (dist < bestDist)
                    {
                        bestDist = dist;
                        best = cell;
                    }
                }
            }
        }
        return best;
    }
}