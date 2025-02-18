using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemySkillController : MonoBehaviour
{
    [Tooltip("引用敌人角色对象")]
    public EnemyCharacter enemy;
    [Tooltip("引用场景中的 GridMap")]
    public GridMapManager gridMap;

    // 当前选中的技能（由 AI 决定）
    private SkillSO selectedSkill = null;
    // 标记是否处于技能释放状态
    private bool isSkillSelectionActive = false;

    void Start()
    {
        if (enemy == null)
            enemy = GetComponent<EnemyCharacter>();
        if (gridMap == null)
            gridMap = FindObjectOfType<GridMapManager>();

        
    }
    public void EnemyStartAttack()
    {
        // 启动 AI 选择技能
        StartCoroutine(SelectAndExecuteSkillByAI());
    }

    IEnumerator SelectAndExecuteSkillByAI()
    {
        Debug.Log("敌人开始选技能");
        yield return new WaitForSeconds(1f); // 延时1秒后执行

        // 选用技能列表中的第一个技能
        if (enemy.skillList.Count > 0)
            selectedSkill = enemy.skillList[0];
        else
            yield break;

        isSkillSelectionActive = true;
        enemy.currentState = CharacterState.Acting;

        // 以敌人当前所在格子作为释放中心
        GridCell releaseCell = enemy.currentCell;

        // 定义四个正方向（注意：这里假定上为(0,1)，右为(1,0)，下为(0,-1)，左为(-1,0)）
        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(0, 1),   // 上
            new Vector2Int(1, 0),   // 右
            new Vector2Int(0, -1),  // 下
            new Vector2Int(-1, 0)   // 左
        };

        // 用于记录最佳方向的数据
        List<SkillTargetInfo> bestTargets = null;
        Vector2Int bestDirection = Vector2Int.zero;
        int bestHitCount = -1;
        float bestTotalHP = int.MaxValue;

        // 遍历四个方向，计算每个方向命中玩家的数量和总血量
        foreach (Vector2Int dir in directions)
        {
            List<SkillTargetInfo> targets = SkillExecutor.GetAffectedTargets(releaseCell, selectedSkill.areaData, dir);

            int hitCount = 0;
            float totalHP = 0;

            // 遍历所有目标，统计命中的玩家单位
            foreach (SkillTargetInfo targetInfo in targets)
            {
                // 假定目标格子的 occupant 存在时表示有单位，且通过 Tag 或组件判断是否为玩家
                if (targetInfo.cell.occupant != null)
                {
                    // 例如：通过 Tag 判断（确保玩家单位的 Tag 设置为 "Player"）
                    if (targetInfo.cell.occupant.CompareTag("Player"))
                    {
                        hitCount++;

                        // 通过组件获取玩家血量（请根据实际属性名称调整）
                        PlayerCharacter pc = targetInfo.cell.occupant.GetComponent<PlayerCharacter>();
                        if (pc != null)
                        {
                            totalHP += pc.currentHealth;
                        }
                    }
                }
            }

            // 根据命中数量和总血量判断是否为最佳方向
            if (hitCount > bestHitCount || (hitCount == bestHitCount && totalHP < bestTotalHP))
            {
                bestHitCount = hitCount;
                bestTotalHP = totalHP;
                bestDirection = dir;
                bestTargets = targets;
            }
        }

        // 若所有方向都没有命中玩家，则可以选择默认方向（例如：上）
        if (bestTargets == null)
        {
            bestDirection = new Vector2Int(0, 1);
            bestTargets = SkillExecutor.GetAffectedTargets(releaseCell, selectedSkill.areaData, bestDirection);
        }

        // 可选：调试时高亮最佳方向的生效区域（实际游戏中可能不显示）
        foreach (SkillTargetInfo targetInfo in bestTargets)
        {
            targetInfo.cell.cellState = GridCellState.SkillRange;
            targetInfo.cell.useTempColorOverride = true;
            targetInfo.cell.tempColorOverride = targetInfo.cellColor;
            targetInfo.cell.RefreshVisual();
        }

        // 敌人执行技能（这里直接传入固定骰子值6，敌人不 roll 骰子）
        selectedSkill.Execute(6, enemy, new List<SkillTargetInfo>(bestTargets));

        // 清除高亮效果
        ClearSkillRangeHighlight();

        isSkillSelectionActive = false;
        selectedSkill = null;
    }

    /// <summary>
    /// 清除所有技能范围的高亮
    /// </summary>
    private void ClearSkillRangeHighlight()
    {
        for (int x = 0; x < gridMap.gridWidth; x++)
        {
            for (int y = 0; y < gridMap.gridHeight; y++)
            {
                GridCell cell = gridMap.gridCells[x, y];
                if (cell.cellState == GridCellState.SkillRange)
                {
                    cell.cellState = GridCellState.Default;
                    cell.useTempColorOverride = false;
                    cell.RefreshVisual();
                }
            }
        }
    }
}
