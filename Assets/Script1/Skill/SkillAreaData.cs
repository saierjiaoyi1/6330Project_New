using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "SkillAreaData", menuName = "Skill/AreaData", order = 1)]
public class SkillAreaData : ScriptableObject
{
    [TextArea]
    public string description;

    [Tooltip("相对于释放中心的格子偏移量列表，例如： (0,0) 表示中心；(0,1),(0,-1),(1,0),(-1,0) 可构成十字形")]
    public List<SkillAreaCellData> cellDataList = new List<SkillAreaCellData>();
}

[System.Serializable]
public class SkillAreaCellData
{
    [Tooltip("相对于释放中心的格子偏移（例如 (0,1) 表示正上方）")]
    public Vector2Int offset;
    [Tooltip("该格子的显示颜色，比如火球为红色、治疗为绿色")]
    public Color cellColor = Color.white;
    [Tooltip("特征码，用于技能效果结算时区分不同效果（比如伤害、回复、附加不同状态）")]
    public int featureCode = 0;
}

public struct SkillTargetInfo
{
    public GridCell cell;        // 对应的格子引用
    public int featureCode;      // 从区域数据中读取的特征码
    public Color cellColor;      // 该格子应显示的颜色

    public SkillTargetInfo(GridCell cell, int featureCode, Color cellColor)
    {
        this.cell = cell;
        this.featureCode = featureCode;
        this.cellColor = cellColor;
    }
}