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
    private SkillSO2 selectedSkill = null;
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
        enemy.EndTurn();
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

        bool ifAttack = false;

        List<GridCell> effectGrids = gridMap.GetNeighbors(enemy.currentCell);
        foreach(GridCell grid in effectGrids)
        {
            if (grid.occupant != null && grid.occupant.GetComponent<PlayerCharacter>() != null)
            {
                // 敌人执行技能（这里直接传入固定骰子值6，敌人不 roll 骰子）
                selectedSkill.Execute(6, enemy, new List<SkillTargetInfo> { new SkillTargetInfo(grid, 0, Color.white) });
                ifAttack = true;
                break;
            }
        }
        if(ifAttack == false)
        {
            enemy.EndTurn();
        }

        

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

    // 工具方法：将任意方向向量转换为最接近的四个正方向（以 Vector2Int 表示）
    private Vector2Int GetCardinalDirection(Vector3 dir)
    {
        if (dir == Vector3.zero) return Vector2Int.zero;
        float angle = Mathf.Atan2(dir.z, dir.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360;
        if (angle >= 45 && angle < 135)
            return Vector2Int.up;
        else if (angle >= 135 && angle < 225)
            return Vector2Int.left;
        else if (angle >= 225 && angle < 315)
            return Vector2Int.down;
        else
            return Vector2Int.right;
    }

}
