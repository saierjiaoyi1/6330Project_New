using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public abstract class BaseCharacter : MonoBehaviour
{
    [Header("基础属性")]
    [Tooltip("血量")]
    public int health = 100;
    public int currentHealth = 100;
    [Tooltip("攻击力")]
    public int attack = 10;
    [Tooltip("移动范围（单位：格子数）")]
    public int movementRange = 3;
    [Tooltip("移动速度，单位：格子/秒")]
    public float moveSpeed = 3f;

    [Header("状态机")]
    public CharacterState currentState = CharacterState.Idle;
    [Tooltip("角色朝向")]
    public Orientation orientation = Orientation.Down;

    [Header("技能列表")]
    [Tooltip("技能列表，可在编辑器中配置，也可在游戏中动态添加")]
    public List<SkillSO> skillList = new List<SkillSO>();

    [Header("所属格子")]
    [Tooltip("角色当前所在的格子")]
    public GridCell currentCell;

    [Header("模型设置")]
    [Tooltip("用于显示角色模型的子对象。移动时只调整该对象的旋转，不影响父对象的位置。")]
    public Transform modelTransform;

    [Header("阵营")]
    [Tooltip("设定角色的阵营是玩家还是敌人")]
    public Team team = Team.Player;

    // 新增：各类伤害抗性（0～1 之间，小数表示百分比减免）
    [Header("抗性配置")]
    [Tooltip("火焰伤害减免比例，例如 0.2 表示减少 20% 火焰伤害==>20点抗性意味着减少20%伤害")]
    public float fireResistance = 0;
    [Tooltip("冰霜伤害减免比例")]
    public float iceResistance = 0;
    [Tooltip("Cut伤害减免比例")]
    public float CutResistance = 0;
    [Tooltip("Blunt伤害减免比例")]
    public float BluntResistance = 0;

    [Header("伤害数字预制体")]
    public GameObject damageTextPrefab;
    public Canvas uiCanvas;

    /// <summary>
    /// 游戏开始时将角色吸附到离其最近的格子中心
    /// </summary>
    protected virtual void Start()
    {
        // 如果策划在 Scene 中直接拖放了角色 prefab，但未手动指定所属格子，则自动寻找最近的格子
        if (currentCell == null)
            SnapToNearestGridCell();
    }

    /// <summary>
    /// 寻找距离角色最近的格子，并吸附到该格子上
    /// </summary>
    protected void SnapToNearestGridCell()
    {
        GridMapManager gridMap = FindObjectOfType<GridMapManager>();
        if (gridMap == null || gridMap.gridCells == null) return;

        GridCell nearest = null;
        float minDist = float.MaxValue;
        foreach (GridCell cell in gridMap.gridCells)
        {
            if (cell == null) continue;
            float dist = Vector3.Distance(transform.position, cell.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = cell;
            }
        }
        if (nearest != null)
        {
            SetCurrentCell(nearest);
            transform.position = nearest.transform.position;
        }
    }

    /// <summary>
    /// 设置角色当前所在的格子，并更新格子中 occupant 的引用
    /// </summary>
    public void SetCurrentCell(GridCell newCell)
    {
        if (currentCell != null && currentCell.occupant == this)
        {
            currentCell.occupant = null;
        }
        currentCell = newCell;
        if (newCell != null)
            newCell.occupant = this;
    }

    /// <summary>
    /// 当轮到该角色回合时调用，子类可在此扩展行为
    /// </summary>
    public virtual void OnTurnStart()
    {
        currentState = CharacterState.Waiting;
        // 高亮显示移动范围
        MarkMovableCells();
    }

    /// <summary>
    /// 使用广度优先搜索标记出从当前格子出发在 movementRange 内可移动的格子（设置状态为 Movable）
    /// </summary>
    protected void MarkMovableCells()
    {
        if (currentCell == null) return;
        GridMapManager gridMap = currentCell.parentMap;
        if (gridMap == null || gridMap.gridCells == null) return;

        // 清除全图上之前的移动范围标记（也可以只清除上次标记的区域）
        for (int x = 0; x < gridMap.gridWidth; x++)
        {
            for (int y = 0; y < gridMap.gridHeight; y++)
            {
                gridMap.gridCells[x, y].cellState = GridCellState.Default;
                gridMap.gridCells[x, y].RefreshVisual();
            }
        }

        // BFS 标记
        Queue<GridCell> queue = new Queue<GridCell>();
        Dictionary<GridCell, int> visited = new Dictionary<GridCell, int>();
        queue.Enqueue(currentCell);
        visited[currentCell] = 0;

        while (queue.Count > 0)
        {
            GridCell cell = queue.Dequeue();
            int distance = visited[cell];
            if (distance > movementRange) continue;
            // 如果不是当前格子，并且格子可通行且无人占用，则标记为可移动
            if (cell.isPassable && (cell.occupant == null || cell.occupant == this))
            {
                cell.cellState = GridCellState.Movable;
                cell.RefreshVisual();
            }
            // 将相邻格子加入队列
            foreach (GridCell neighbor in gridMap.GetNeighbors(cell))
            {
                if (neighbor == null) continue;
                if (!visited.ContainsKey(neighbor) && neighbor.isPassable && neighbor.occupant == null)
                {
                    visited[neighbor] = distance + 1;
                    queue.Enqueue(neighbor);
                }
            }
        }
    }

    /// <summary>
    /// 角色走向目标格子的入口方法，传入 A* 算法计算得到的路径
    /// </summary>
    /// <param name="path">路径格子列表</param>
    public virtual void MoveAlongPath(List<GridCell> path)
    {
        if (path == null || path.Count == 0)
            return;
        // 启动协程平滑移动
        StartCoroutine(MoveAlongPathCoroutine(path));
    }

    /// <summary>
    /// 协程：沿着路径逐步移动，每一步更新位置与朝向
    /// </summary>
    protected IEnumerator MoveAlongPathCoroutine(List<GridCell> path)
    {
        // 禁用鼠标输入
        GameManager.Instance.mouseInputEnabled = false;
        currentState = CharacterState.Moving;

        // 如果路径第一个格子就是当前格子，移除它
        //if (path[0] == currentCell)
        //    path.RemoveAt(0);

        for (int i = 0; i < path.Count; i++)
        {
            GridCell targetCell = path[i];
            Vector3 startPos = transform.position;
            Vector3 endPos = targetCell.transform.position;
            float journey = Vector3.Distance(startPos, endPos);
            float progress = 0f;
            Vector3 moveDir = (endPos - startPos).normalized;
            UpdateOrientation(moveDir);

            while (progress < journey)
            {
                progress += moveSpeed * Time.deltaTime;
                float fraction = progress / journey;
                transform.position = Vector3.Lerp(startPos, endPos, fraction);
                yield return null;
            }
            transform.position = endPos;
            SetCurrentCell(targetCell);
            yield return null;
        }

        // 移动结束后调用 OnMovementComplete()，由子类决定后续处理
        OnMovementComplete();
    }

    /// <summary>
    /// 移动完成后调用的方法，默认行为为结束回合
    /// </summary>
    protected virtual void OnMovementComplete()
    {
        EndTurn();
    }

    /// <summary>
    /// 根据移动方向更新角色朝向，建议只旋转子物体（模型），以便保持根对象位置对齐格子
    /// </summary>
    /// <param name="dir">移动方向</param>
    public void UpdateOrientation(Vector3 dir)
    {
        // 根据 x 与 z 分量判断朝向
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.z))
        {
            orientation = (dir.x > 0) ? Orientation.Right : Orientation.Left;
        }
        else
        {
            orientation = (dir.z > 0) ? Orientation.Up : Orientation.Down;
        }

        // 只旋转模型（如果设置了 modelTransform），否则旋转根对象
        Transform target = modelTransform != null ? modelTransform : transform;
        // 可根据实际模型调整旋转角度，这里采用简单方案：使正前方（Z轴）指向移动方向
        if (dir != Vector3.zero)
        {
            Quaternion lookRot = Quaternion.LookRotation(dir, Vector3.up);
            target.rotation = lookRot;
        }
    }

    /// <summary>
    /// 回合结束时调用，清除移动范围高亮，并通知回合管理器进行下一回合
    /// </summary>
    public virtual void EndTurn()
    {
        currentState = CharacterState.Idle;
        ClearMovableCells();
        TurnBasedManager.Instance.NextTurn();
    }

    /// <summary>
    /// 清除全图上所有标记为 Movable 的格子
    /// </summary>
    protected void ClearMovableCells()
    {
        GridMapManager gridMap = currentCell?.parentMap;
        if (gridMap == null || gridMap.gridCells == null) return;
        for (int x = 0; x < gridMap.gridWidth; x++)
        {
            for (int y = 0; y < gridMap.gridHeight; y++)
            {
                if (gridMap.gridCells[x, y].cellState == GridCellState.Movable)
                {
                    gridMap.gridCells[x, y].cellState = GridCellState.Default;
                    gridMap.gridCells[x, y].RefreshVisual();
                }
            }
        }
    }

    /// <summary>
    /// 受到伤害的方法  
    /// 参数 rawDamage 为原始伤害值，damageType 表示伤害类型  
    /// 方法内部根据角色抗性计算实际伤害，然后扣除血量，并显示伤害数字
    /// </summary>
    public virtual void ReceiveDamage(int rawDamage, DamageType damageType)
    {
        float resistance = 0f;
        Color damageColor = Color.white;

        // 根据伤害类型确定抗性和伤害数字颜色
        switch (damageType)
        {
            case DamageType.Fire:
                resistance = fireResistance;
                damageColor = Color.red;
                break;
            case DamageType.Ice:
                resistance = iceResistance;
                damageColor = Color.cyan;
                break;
            case DamageType.Blunt:
                resistance = BluntResistance;
                damageColor = Color.yellow;
                break;
            case DamageType.Cut:
                resistance = CutResistance;
                damageColor = Color.black;
                break;
            default:
                resistance = 0f;
                damageColor = Color.white;
                break;
        }

        // 计算实际伤害（可以根据需要取整、上/下取整）
        int effectiveDamage = Mathf.RoundToInt(rawDamage * (100 - resistance)/100);
        // 扣除血量
        currentHealth -= effectiveDamage;
        if (currentHealth < 0)
        {
            currentHealth = 0;
            //死亡
        }
            

        // 显示伤害数字（你可以改为实例化浮动文本 prefab 等）
        ShowDamageText(effectiveDamage, damageColor);

        // 可添加其它逻辑，比如播放受击动画、检查是否死亡、触发事件等
        Debug.Log($"{name} 受到了 {effectiveDamage} 点 {damageType} 伤害，剩余血量：{health}");
    }

    /// <summary>
    /// 显示伤害数字的方法  
    /// 你可以在此方法内实例化一个预制体，在角色上方显示伤害数字，并设置相应的颜色
    /// 这里简单使用 Debug.Log 模拟显示
    /// </summary>
    protected virtual void ShowDamageText(int damage, Color color)
    {
        // 实例化预制件到 UI Canvas 下（这样预制件就会作为 Canvas 的子对象显示）
        GameObject go = Instantiate(damageTextPrefab, uiCanvas.transform);
        DamageText fd = go.GetComponent<DamageText>();
        // 用角色当前的世界坐标作为数字显示的起点
        fd.Init(damage, color, transform.position, 100, 1);
    }
}