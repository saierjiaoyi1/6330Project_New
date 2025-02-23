using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerSkillController2 : MonoBehaviour
{
    [Tooltip("引用玩家角色对象")]
    public PlayerCharacter player;
    [Tooltip("引用场景中的 GridMap")]
    public GridMapManager gridMap;

    // 当前选中的技能资源（从玩家技能列表中选择）
    private SkillSO2 selectedSkill = null;
    // 标记是否正在技能选择模式下
    private bool isSkillSelectionActive = false;

    // 缓存原本显示的移动范围（即状态为 Movable 的格子），以便在取消技能选择时恢复
    private List<GridCell> cachedMoveRangeCells = new List<GridCell>();
    // 当前已高亮显示的技能生效区域格子
    private List<GridCell> currentEffectRangeCells = new List<GridCell>();
    // 当前保存的目标信息列表（SkillTargetInfo 结构体，包含格子、特征码等信息）
    private List<SkillTargetInfo> currentEffectTargets = new List<SkillTargetInfo>();

    // 当前鼠标下有效的释放中心格子（仅对非 SelfCentered 技能有效）
    private GridCell currentReleaseCell = null;
    // 缓存释放区域（SkillArea）的格子
    public List<GridCell> releaseAreaCells = new List<GridCell>();

    void Start()
    {
        if (player == null)
        {
            player = GetComponent<PlayerCharacter>();
        }
        if (gridMap == null)
        {
            gridMap = FindObjectOfType<GridMapManager>();
        }
    }

    void Update()
    {
        if (!isSkillSelectionActive) return;

        // 如果玩家右键点击，则取消技能选择
        if (Input.GetMouseButtonDown(1))
        {
            CancelSkillSelection();
            releaseAreaCells.Clear();
            return;
        }

        if (selectedSkill != null)
        {
            Vector3 playerPos = player.currentCell.transform.position;
            Vector3 mousePos = GetMouseWorldPosition();
            Vector3 dir = mousePos - playerPos;
            player.UpdateOrientation(dir);
            // 对于 SelfCentered 技能：释放中心固定为玩家当前格子
            if (selectedSkill.releaseType == SkillReleaseType.SelfCentered)
            {
                currentReleaseCell = player.currentCell;
                //Vector3 playerPos = player.currentCell.transform.position;
                //Vector3 mousePos = GetMouseWorldPosition();
                //Vector3 dir = mousePos - playerPos;
                Vector2Int cardinalDir = GetCardinalDirection(dir);
                UpdateSelfCenteredEffectRange(player.currentCell, cardinalDir);

                if (Input.GetMouseButtonDown(0))
                {
                    ExecuteSkill();
                }
            }
            else
            {
                // 对于 FreeSelection 或 TargetUnitSelection：先要求玩家在“释放区域”中选取一个格子
                GridCell hoveredCell = GetHoveredGridCell();
                if (hoveredCell != null && releaseAreaCells.Contains(hoveredCell))
                {
                    // 如果鼠标停在某个有效释放中心上，则更新当前释放中心及生效范围高亮
                    if (currentReleaseCell != hoveredCell)
                    {
                        currentReleaseCell = hoveredCell;

                        UpdateEffectRangeHighlight(currentReleaseCell, null);
                    }

                    // 当左键点击时确认释放技能
                    if (Input.GetMouseButtonDown(0))
                    {
                        ExecuteSkill();
                    }
                }
                else
                {
                    // 如果鼠标不在有效释放中心上，则清除生效范围高亮
                    ClearEffectRangeHighlight();
                    currentReleaseCell = null;
                }
            }
        }
    }

    /// <summary>
    /// UI 按钮调用，根据技能索引选择技能
    /// </summary>
    public void SelectSkill(int skillIndex)
    {
        if (player.currentState == CharacterState.Acting || player.currentState == CharacterState.SelectingSkill)
            return;
        if (skillIndex < 0 || skillIndex >= player.skillList.Count)
        {
            Debug.LogError("无效的技能索引！");
            return;
        }
        selectedSkill = player.skillList[skillIndex];
        EnterSkillSelectionMode();
    }

    /// <summary>
    /// 进入技能选择模式：缓存移动范围高亮，清除移动高亮，并显示技能释放区域
    /// </summary>
    private void EnterSkillSelectionMode()
    {
        if (player == null || gridMap == null || selectedSkill == null) return;

        isSkillSelectionActive = true;
        player.currentState = CharacterState.SelectingSkill;

        // 缓存并清除当前移动范围高亮
        cachedMoveRangeCells.Clear();
        for (int x = 0; x < gridMap.gridWidth; x++)
        {
            for (int y = 0; y < gridMap.gridHeight; y++)
            {
                GridCell cell = gridMap.gridCells[x, y];
                if (cell.cellState == GridCellState.Movable)
                {
                    cachedMoveRangeCells.Add(cell);
                    cell.cellState = GridCellState.Default;
                    cell.RefreshVisual();
                }
            }
        }

        // 若技能释放中心不固定为自身，则计算并高亮有效释放区域（SkillArea）
        if (selectedSkill.releaseType != SkillReleaseType.SelfCentered)
        {
            GridCell origin = player.currentCell;
            for (int x = 0; x < gridMap.gridWidth; x++)
            {
                for (int y = 0; y < gridMap.gridHeight; y++)
                {
                    GridCell cell = gridMap.gridCells[x, y];
                    int manhattanDist = Mathf.Abs(cell.x - origin.x) + Mathf.Abs(cell.y - origin.y);
                    if (manhattanDist <= selectedSkill.releaseRange)
                    {
                        cell.cellState = GridCellState.SkillArea;
                        cell.RefreshVisual();
                        releaseAreaCells.Add(cell);
                    }
                }
            }
        }
        else
        {
            // SelfCentered 技能直接显示预览效果范围
            currentReleaseCell = player.currentCell;
            Vector3 playerPos = player.currentCell.transform.position;
            Vector3 mousePos = GetMouseWorldPosition();
            Vector3 dir = mousePos - playerPos;
            Vector2Int cardinalDir = GetCardinalDirection(dir);
            UpdateSelfCenteredEffectRange(player.currentCell, cardinalDir);
        }
    }

    /// <summary>
    /// 取消技能选择：清除所有技能相关高亮并恢复原先移动范围显示
    /// </summary>
    private void CancelSkillSelection()
    {
        isSkillSelectionActive = false;
        selectedSkill = null;
        ClearEffectRangeHighlight();
        ClearSkillAreaHighlight();
        foreach (GridCell cell in cachedMoveRangeCells)
        {
            cell.cellState = GridCellState.Movable;
            cell.RefreshVisual();
        }
        cachedMoveRangeCells.Clear();
        player.currentState = CharacterState.Waiting;
    }

    /// <summary>
    /// 玩家确认（左键点击）后执行技能
    /// </summary>
    private void ExecuteSkill()
    {
        Debug.Log("SkillController2");
        StartCoroutine(ExecuteSkillCoroutine());
    }

    private IEnumerator ExecuteSkillCoroutine()
    {
        if (selectedSkill == null) yield break;

        // 对于 TargetUnitSelection 技能，必须确保选中目标格子上有目标单位
        if (selectedSkill.releaseType == SkillReleaseType.TargetUnitSelection)
        {
            if (currentReleaseCell == null || currentReleaseCell.occupant == null)
            {
                Debug.Log("请选择一个包含目标单位的格子！");
                yield break;
            }
        }

        isSkillSelectionActive = false;

        // 执行骰子掷点逻辑（若技能需要随机数，可利用 diceValue 影响 Task 行为）
        bool rollCompleted = false;
        (int, int) diceResult = (0, 0);
        yield return StartCoroutine(GameManager.Instance.RollDice(result =>
        {
            diceResult = result;
            rollCompleted = true;
        }));
        while (!rollCompleted) yield return null;
        int diceValue = diceResult.Item1 + diceResult.Item2;

        // 创建 SkillContext，将释放者、目标信息（以及需要时骰子结果）传入
        SkillContext context = new SkillContext();
        context.caster = player; // 假设 player 继承自 BaseCharacter
        context.targetInfos = new List<SkillTargetInfo>(currentEffectTargets);
        // 如有需要，可在 SkillContext 中加入 diceValue 字段： context.diceValue = diceValue;

        // 使用 selectedSkill 中配置的 Task 列表顺序执行技能逻辑
        if (selectedSkill.taskList != null)
        {
            foreach (Task task in selectedSkill.taskList)
            {
                yield return StartCoroutine(task.Execute(context));
            }
        }
        else
        {
            Debug.LogWarning("Skill 没有配置 Task 列表！");
        }

        // 技能执行完毕后，清除所有高亮并重置状态
        ClearEffectRangeHighlight();
        ClearSkillAreaHighlight();
        cachedMoveRangeCells.Clear();
        isSkillSelectionActive = false;
        selectedSkill = null;
        releaseAreaCells.Clear();
    }

    /// <summary>
    /// 根据给定释放中心及可选方向计算并高亮技能生效范围（SkillRange）
    /// </summary>
    private void UpdateEffectRangeHighlight(GridCell releaseCell, Vector2Int? direction)
    {
        ClearEffectRangeHighlight();
        if (releaseCell == null || selectedSkill == null || selectedSkill.areaData == null) return;
        currentEffectTargets = SkillExecutor.GetAffectedTargets(releaseCell, selectedSkill.areaData, direction);
        foreach (SkillTargetInfo targetInfo in currentEffectTargets)
        {
            targetInfo.cell.cellState = GridCellState.SkillRange;
            targetInfo.cell.useTempColorOverride = true;
            targetInfo.cell.tempColorOverride = targetInfo.cellColor;
            targetInfo.cell.RefreshVisual();
        }
    }

    /// <summary>
    /// 针对 SelfCentered 技能，根据鼠标方向旋转 AreaData 更新生效范围高亮
    /// </summary>
    private void UpdateSelfCenteredEffectRange(GridCell center, Vector2Int rotationDir)
    {
        ClearEffectRangeHighlight();
        if (center == null || selectedSkill == null || selectedSkill.areaData == null) return;
        currentEffectTargets = SkillExecutor.GetAffectedTargets(center, selectedSkill.areaData, rotationDir);
        foreach (SkillTargetInfo targetInfo in currentEffectTargets)
        {
            targetInfo.cell.cellState = GridCellState.SkillRange;
            targetInfo.cell.useTempColorOverride = true;
            targetInfo.cell.tempColorOverride = targetInfo.cellColor;
            targetInfo.cell.RefreshVisual();
        }
    }

    /// <summary>
    /// 清除当前生效范围高亮：若格子原本为 SkillArea 则恢复之，否则设为 Default
    /// </summary>
    private void ClearEffectRangeHighlight()
    {
        foreach (GridCell cell in gridMap.GetComponentsInChildren<GridCell>())
        {
            if (cell.cellState == GridCellState.SkillRange)
            {
                if (releaseAreaCells.Contains(cell))
                    cell.cellState = GridCellState.SkillArea;
                else
                    cell.cellState = GridCellState.Default;
                cell.useTempColorOverride = false;
                cell.RefreshVisual();
            }
        }
        currentEffectTargets.Clear();
    }

    /// <summary>
    /// 清除所有释放区域（SkillArea）高亮
    /// </summary>
    private void ClearSkillAreaHighlight()
    {
        for (int x = 0; x < gridMap.gridWidth; x++)
        {
            for (int y = 0; y < gridMap.gridHeight; y++)
            {
                GridCell cell = gridMap.gridCells[x, y];
                if (cell.cellState == GridCellState.SkillArea)
                {
                    cell.cellState = GridCellState.Default;
                    cell.RefreshVisual();
                }
            }
        }
    }

    // 以下为工具方法

    // 通过射线获得鼠标在地面上的世界坐标（假设地图在 XZ 平面，y=0）
    private Vector3 GetMouseWorldPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.up, Vector3.zero);
        float dist;
        if (plane.Raycast(ray, out dist))
        {
            return ray.GetPoint(dist);
        }
        return Vector3.zero;
    }

    // 获取鼠标当前射线下的 GridCell（要求格子上有 Collider）
    private GridCell GetHoveredGridCell()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100f))
        {
            return hit.collider.GetComponent<GridCell>();
        }
        return null;
    }

    // 将任意方向向量转换为最接近的四个正方向（以 Vector2Int 表示）
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

    /// <summary>
    /// 将 AreaData 中的偏移量列表根据旋转方向转换（默认原始偏移以 (0,1) 为正向）
    /// </summary>
    private List<Vector2Int> RotateAreaOffsets(List<Vector2Int> offsets, Vector2Int rotationDir)
    {
        List<Vector2Int> rotated = new List<Vector2Int>();
        foreach (Vector2Int offset in offsets)
        {
            Vector2Int newOffset = offset;
            if (rotationDir == new Vector2Int(0, 1))
                newOffset = offset;
            else if (rotationDir == new Vector2Int(1, 0))
                newOffset = new Vector2Int(offset.y, -offset.x);
            else if (rotationDir == new Vector2Int(0, -1))
                newOffset = new Vector2Int(-offset.x, -offset.y);
            else if (rotationDir == new Vector2Int(-1, 0))
                newOffset = new Vector2Int(-offset.y, offset.x);
            rotated.Add(newOffset);
        }
        return rotated;
    }
}
