using UnityEngine;

[ExecuteAlways]
public class GridCell : MonoBehaviour
{
    [Header("格子坐标")]
    [Tooltip("X 坐标（列号）")]
    public int x;
    [Tooltip("Y 坐标（行号）")]
    public int y;

    [Header("格子配置")]
    [Tooltip("每个格子的尺寸（仅用于定位）")]
    public float cellSize = 1.0f;
    [Tooltip("该格子是否可通行，设为 false 时在 Scene 中显示为红色")]
    public bool isPassable = true;
    [Tooltip("当前格子的状态，用于决定显示哪个 Sprite")]
    public GridCellState cellState = GridCellState.Default;

    [Header("关联")]
    [Tooltip("所属的 GridMap，内部会根据此引用获取状态对应的 Sprite")]
    public GridMapManager parentMap;

    /// <summary>
    /// 当前格子中占用的角色（如果有的话）
    /// </summary>
    [Tooltip("当前占用者（玩家或敌人），为空时表示无人占用")]
    public BaseCharacter occupant;

    // 临时颜色覆盖（用于技能高亮）
    [HideInInspector] public bool useTempColorOverride = false;
    [HideInInspector] public Color tempColorOverride = Color.white;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();

        RefreshVisual();
    }

    /// <summary>
    /// 刷新格子的视觉显示（根据是否可通行以及状态设置 Sprite 和颜色）
    /// </summary>
    public void RefreshVisual()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        // 不可通行时始终显示红色
        if (!isPassable)
            spriteRenderer.color = new Color(1f, 0f, 0f, 0f);
        else
        {
            // 如果临时覆盖颜色开启，则使用，否则用白色
            if (useTempColorOverride)
                spriteRenderer.color = tempColorOverride;
            else
                spriteRenderer.color = Color.white;
        }

        // 根据状态设置 Sprite（如果父地图已配置）
        if (parentMap != null)
        {
            Sprite newSprite = parentMap.GetSpriteForState(cellState);
            if (newSprite != null)
                spriteRenderer.sprite = newSprite;
        }
    }

    private void OnValidate()
    {
        RefreshVisual();
    }
}