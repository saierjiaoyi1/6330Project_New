using UnityEngine;
using System.Collections.Generic;

public class PlayerSkillController : MonoBehaviour
{
    [Tooltip("引用玩家角色对象")]
    public PlayerCharacter player;
    [Tooltip("引用场景中的 GridMap")]
    public GridMapManager gridMap;

    // 当前选中的技能资源（从玩家技能列表中选择）
    private SkillSO selectedSkill = null;
    // 标记是否正在技能选择模式下
    private bool isSkillSelectionActive = false;

    // 缓存原本显示的移动范围（即状态为 Movable 的格子），以便在取消技能选择时恢复
    private List<GridCell> cachedMoveRangeCells = new List<GridCell>();
    // 当前已高亮显示的技能生效区域格子（SkillRange）???
    private List<GridCell> currentEffectRangeCells = new List<GridCell>();
    // 现在保存的是目标信息列表
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
    /// 由 UI 按钮调用，根据传入的技能索引选择技能
    /// </summary>
    public void SelectSkill(int skillIndex)
    {
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
        // 将玩家状态设为 SelectingSkill，禁用其他操作
        player.currentState = CharacterState.SelectingSkill;

        // 缓存并清除当前移动范围高亮（状态为 Movable 的格子）
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

        // 若技能要求释放中心不固定为自身，则计算并高亮有效释放区域（SkillArea）
        if (selectedSkill.releaseType != SkillReleaseType.SelfCentered)
        {
            GridCell origin = player.currentCell;
            for (int x = 0; x < gridMap.gridWidth; x++)
            {
                for (int y = 0; y < gridMap.gridHeight; y++)
                {
                    GridCell cell = gridMap.gridCells[x, y];
                    // 这里简单采用曼哈顿距离作为判断依据
                    int manhattanDist = Mathf.Abs(cell.x - origin.x) + Mathf.Abs(cell.y - origin.y);
                    if (manhattanDist <= selectedSkill.releaseRange)
                    {
                        cell.cellState = GridCellState.SkillArea;
                        cell.RefreshVisual();
                        // 记录该格子为释放区域的一部分
                        releaseAreaCells.Add(cell);
                    }
                }
            }
        }
        else
        {
            // 对于 SelfCentered 技能，直接显示生效范围预览（由 UpdateSelfCenteredEffectRange 控制）
            currentReleaseCell = player.currentCell;
            Vector3 playerPos = player.currentCell.transform.position;
            Vector3 mousePos = GetMouseWorldPosition();
            Vector3 dir = mousePos - playerPos;
            Vector2Int cardinalDir = GetCardinalDirection(dir);
            UpdateSelfCenteredEffectRange(player.currentCell, cardinalDir);
        }
    }

    /// <summary>
    /// 取消技能选择：清除所有技能相关的高亮，恢复原先的移动范围显示
    /// </summary>
    private void CancelSkillSelection()
    {
        isSkillSelectionActive = false;
        selectedSkill = null;
        ClearEffectRangeHighlight();
        ClearSkillAreaHighlight();
        // 恢复之前缓存的移动范围高亮
        foreach (GridCell cell in cachedMoveRangeCells)
        {
            cell.cellState = GridCellState.Movable;
            cell.RefreshVisual();
        }
        cachedMoveRangeCells.Clear();
        // 恢复玩家状态为 Waiting
        player.currentState = CharacterState.Waiting;
    }

    /// <summary>
    /// 当玩家确认（左键点击）后执行技能
    /// </summary>
    /// <param name="direction">
    /// 对于 SelfCentered 类型技能，可传入一个方向（四个正方向）；对其他类型技能可传 null
    /// </param>
    private async void ExecuteSkill()
    {
        if (selectedSkill == null) return;
        // 对于 TargetUnitSelection，确保选中的释放中心格子内有单位
        if (selectedSkill.releaseType == SkillReleaseType.TargetUnitSelection)
        {
            if (currentReleaseCell == null || currentReleaseCell.occupant == null)
            {
                Debug.Log("请选择一个包含目标单位的格子！");
                return;
            }
        }
        //先roll骰子
        isSkillSelectionActive = false;
        var (dice1Value, dice2Value) = await GameManager.Instance.RollDice();
        int diceValue = dice1Value + dice2Value;

        // 直接传入当前高亮（SkillRange）的格子列表
        selectedSkill.Execute(diceValue, player, new List<SkillTargetInfo>(currentEffectTargets));

        // 执行后清除所有技能高亮，并结束玩家回合
        ClearEffectRangeHighlight();
        ClearSkillAreaHighlight();
        cachedMoveRangeCells.Clear();
        isSkillSelectionActive = false;
        selectedSkill = null;
        //player.EndTurn(); 在技能协程内结束回合
        releaseAreaCells.Clear();
    }

    /// <summary>
    /// 根据给定释放中心（及可选方向）计算并高亮技能生效范围（SkillRange）
    /// </summary>
    private void UpdateEffectRangeHighlight(GridCell releaseCell, Vector2Int? direction)
    {
        ClearEffectRangeHighlight();
        if (releaseCell == null || selectedSkill == null || selectedSkill.areaData == null) return;
        currentEffectTargets = SkillExecutor.GetAffectedTargets(releaseCell, selectedSkill.areaData, direction);
        foreach (SkillTargetInfo targetInfo in currentEffectTargets)
        {
            // 对应格子高亮为 SkillRange，并用区域数据中的颜色显示
            targetInfo.cell.cellState = GridCellState.SkillRange;
            targetInfo.cell.useTempColorOverride = true;
            targetInfo.cell.tempColorOverride = targetInfo.cellColor;
            targetInfo.cell.RefreshVisual();
        }
    }

    /// <summary>
    /// 新增：针对 SelfCentered 技能，根据玩家当前鼠标方向旋转 AreaData 更新生效范围高亮
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
    /// 清除当前生效范围高亮：
    /// 若某格子原本属于释放区域（SkillArea），则恢复为 SkillArea，否则设为 Default
    /// </summary>
    private void ClearEffectRangeHighlight()
    {
        foreach (GridCell cell in gridMap.GetComponentsInChildren<GridCell>())
        {
            if (cell.cellState == GridCellState.SkillRange)
            {
                // 如果该格子属于释放区域，则恢复为 SkillArea；否则设为 Default
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

    // 工具方法：通过摄像机射线获得鼠标在地面上的世界坐标（假设地图在 XZ 平面上，y=0）
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

    // 工具方法：获得鼠标当前射线下的 GridCell（需保证格子上有 Collider）
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

    /// <summary>
    /// 工具方法：将 AreaData 中的偏移量列表按给定的旋转方向转换（默认假定原始偏移以 (0,1) 为正向）
    /// </summary>
    private List<Vector2Int> RotateAreaOffsets(List<Vector2Int> offsets, Vector2Int rotationDir)
    {
        List<Vector2Int> rotated = new List<Vector2Int>();
        foreach (Vector2Int offset in offsets)
        {
            Vector2Int newOffset = offset;
            if (rotationDir == new Vector2Int(0, 1))
            {
                newOffset = offset; // 无旋转
            }
            else if (rotationDir == new Vector2Int(1, 0))
            {
                // 顺时针 90°: (x, y) -> (y, -x)
                newOffset = new Vector2Int(offset.y, -offset.x);
            }
            else if (rotationDir == new Vector2Int(0, -1))
            {
                // 180°: (x, y) -> (-x, -y)
                newOffset = new Vector2Int(-offset.x, -offset.y);
            }
            else if (rotationDir == new Vector2Int(-1, 0))
            {
                // 逆时针 90°: (x, y) -> (-y, x)
                newOffset = new Vector2Int(-offset.y, offset.x);
            }
            rotated.Add(newOffset);
        }
        return rotated;
    }
}