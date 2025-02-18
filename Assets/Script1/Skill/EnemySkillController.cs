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

    // 缓存用于显示的释放区域和生效区域（逻辑类似玩家）
    private List<GridCell> releaseAreaCells = new List<GridCell>();
    private List<SkillTargetInfo> currentEffectTargets = new List<SkillTargetInfo>();

    void Start()
    {
        if (enemy == null)
            enemy = GetComponent<EnemyCharacter>();
        if (gridMap == null)
            gridMap = FindObjectOfType<GridMapManager>();

        // 启动 AI 选择技能（此处仅作示例，实际应根据敌人 AI 策略选择技能和释放位置）
        StartCoroutine(SelectAndExecuteSkillByAI());
    }

    IEnumerator SelectAndExecuteSkillByAI()
    {
        yield return new WaitForSeconds(1f); // 假设延时后选择技能
        // TODO: 根据 AI 策略选择技能，此处简单选择技能列表中的第一个
        if (enemy.skillList.Count > 0)
            selectedSkill = enemy.skillList[0];
        else
            yield break;

        // 进入技能释放状态
        isSkillSelectionActive = true;
        enemy.currentState = CharacterState.Acting;  // 或新定义一个 AI 专用状态

        // TODO: 根据 AI 策略确定释放中心，下面示例直接以离最近玩家的格子为释放中心
        GridCell releaseCell = DetermineAIReleaseCell();
        // 高亮释放区域（可选，便于调试）
        // 此处使用 SkillExecutor 计算生效区域（不做旋转处理，或者你也可扩展 AI 中的方向决策）
        currentEffectTargets = SkillExecutor.GetAffectedTargets(releaseCell, selectedSkill.areaData, null);
        foreach (SkillTargetInfo targetInfo in currentEffectTargets)
        {
            targetInfo.cell.cellState = GridCellState.SkillRange;
            targetInfo.cell.useTempColorOverride = true;
            targetInfo.cell.tempColorOverride = targetInfo.cellColor;
            targetInfo.cell.RefreshVisual();
        }

        // 启动技能动画协程，由技能自身 Execute 处理
        selectedSkill.Execute(6, enemy, new List<SkillTargetInfo>(currentEffectTargets));
        ClearSkillRangeHighlight();
        isSkillSelectionActive = false;
        selectedSkill = null;
        enemy.EndTurn();
    }

    /// <summary>
    /// 示例：简单选择一个释放中心，实际应由 AI 策略确定
    /// </summary>
    private GridCell DetermineAIReleaseCell()
    {
        // 此处示例返回敌人当前所在格子
        return enemy.currentCell;
    }

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