using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 地图系统：管理整个格子地图，包括生成、数据管理、邻域查找等。
/// </summary>
[ExecuteAlways]
public class GridMapManager : MonoBehaviour
{
    [Header("地图参数")]
    [Tooltip("地图宽度（格子数）")]
    public int gridWidth = 10;
    [Tooltip("地图高度（格子数）")]
    public int gridHeight = 10;
    [Tooltip("每个格子的尺寸")]
    public float cellSize = 1.0f;

    [Header("格子状态对应的 Sprite 配置")]
    [Tooltip("配置各状态下所用的 Sprite（例如：Default, Movable, Selected, SkillRange）")]
    public List<GridCellSpriteMapping> cellSpriteMappings;

    // 保存所有格子的二维数组
    //[HideInInspector]
    [Header("鸽子们")]
    public GridCell[,] gridCells;


    [Header("可选：格子预制体")]
    [Tooltip("如果提供预制体，则在生成格子时会以此预制体为模板；否则系统会自动创建 GameObject 并添加 SpriteRenderer")]
    public GameObject gridCellPrefab;

    // 内部字典（运行时根据映射列表初始化）
    private Dictionary<GridCellState, Sprite> spriteMappingDict;


    private void Awake()
    {
        InitializeSpriteMapping();
        // 尝试通过查找子对象的 GridCell 来重建 gridCells 数组
        GridCell[] cells = GetComponentsInChildren<GridCell>();
        if (cells != null && cells.Length > 0)
        {
            gridCells = new GridCell[gridWidth, gridHeight];
            foreach (GridCell cell in cells)
            {
                // 假设 cell.x 和 cell.y 已经正确设置
                if (cell.x < gridWidth && cell.y < gridHeight)
                    gridCells[cell.x, cell.y] = cell;
            }
        }
        else
        {
            // 如果没有找到子对象，则生成格子地图
            GenerateGrid();
        }
    }

    private void OnValidate()
    {
        InitializeSpriteMapping();

        // 如果已生成格子，则更新每个格子的显示（例如修改了 isPassable 后自动显示红色）
        if (gridCells != null)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    if (gridCells[x, y] != null)
                        gridCells[x, y].RefreshVisual();
                }
            }
        }
    }

    /// <summary>
    /// 根据 cellSpriteMappings 初始化内部字典
    /// </summary>
    void InitializeSpriteMapping()
    {
        spriteMappingDict = new Dictionary<GridCellState, Sprite>();
        if (cellSpriteMappings != null)
        {
            foreach (var mapping in cellSpriteMappings)
            {
                if (!spriteMappingDict.ContainsKey(mapping.cellState))
                    spriteMappingDict.Add(mapping.cellState, mapping.sprite);
            }
        }
    }

    /// <summary>
    /// 根据状态获取对应的 Sprite
    /// </summary>
    public Sprite GetSpriteForState(GridCellState state)
    {
        if (spriteMappingDict != null && spriteMappingDict.ContainsKey(state))
            return spriteMappingDict[state];
        return null;
    }

    /// <summary>
    /// 生成地图（包括生成各个格子）。可通过 Inspector 按钮调用此方法。
    /// </summary>
    [ContextMenu("Generate Grid Map")]
    public void GenerateGrid()
    {
        // 先清空已有的子对象（旧的格子）
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                DestroyImmediate(transform.GetChild(i).gameObject);
            else
                Destroy(transform.GetChild(i).gameObject);
#else
            Destroy(transform.GetChild(i).gameObject);
#endif
        }

        gridCells = new GridCell[gridWidth, gridHeight];

        // 根据配置的宽高生成格子
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                GameObject cellObj = null;
                if (gridCellPrefab != null)
                {
                    cellObj = Instantiate(gridCellPrefab, transform);
                    cellObj.name = $"GridCell_{x}_{y}";
                }
                else
                {
                    cellObj = new GameObject($"GridCell_{x}_{y}");
                    cellObj.transform.parent = transform;
                    // 自动添加 SpriteRenderer 组件
                    cellObj.AddComponent<SpriteRenderer>();
                }

                // 设置格子位置（按 cellSize 间隔排列）
                cellObj.transform.position = new Vector3(x * cellSize, 0, y * cellSize);
                cellObj.transform.rotation = Quaternion.Euler(90, 0, 0);

                // 添加或获取 GridCell 组件
                GridCell gridCell = cellObj.GetComponent<GridCell>();
                if (gridCell == null)
                    gridCell = cellObj.AddComponent<GridCell>();

                // 初始化格子数据
                gridCell.x = x;
                gridCell.y = y;
                gridCell.cellSize = cellSize;
                gridCell.parentMap = this;
                gridCell.cellState = GridCellState.Default;
                gridCell.isPassable = true;

                // 更新视觉显示
                gridCell.RefreshVisual();

                gridCells[x, y] = gridCell;
            }
        }
    }

    /// <summary>
    /// 根据坐标获取格子（边界内返回对应格子，否则返回 null）
    /// </summary>
    public GridCell GetCell(int x, int y)
    {
        if (x >= 0 && x < gridWidth && y >= 0 && y < gridHeight)
            return gridCells[x, y];
        return null;
    }

    /// <summary>
    /// 获取某个格子（或坐标）的4邻域格子（上、下、左、右），可供后续寻路使用
    /// </summary>
    public List<GridCell> GetNeighbors(GridCell cell)
    {
        List<GridCell> neighbors = new List<GridCell>();
        int x = cell.x;
        int y = cell.y;

        // 上
        GridCell up = GetCell(x, y + 1);
        if (up != null) neighbors.Add(up);
        // 下
        GridCell down = GetCell(x, y - 1);
        if (down != null) neighbors.Add(down);
        // 左
        GridCell left = GetCell(x - 1, y);
        if (left != null) neighbors.Add(left);
        // 右
        GridCell right = GetCell(x + 1, y);
        if (right != null) neighbors.Add(right);

        return neighbors;
    }
    public void ClearCellHighlights()
    {
        if (gridCells == null) return;
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (gridCells[x, y].cellState == GridCellState.Movable)
                {
                    gridCells[x, y].cellState = GridCellState.Default;
                    gridCells[x, y].RefreshVisual();
                }
            }
        }
    }
}