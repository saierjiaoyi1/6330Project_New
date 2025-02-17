using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public abstract class SkillSO : ScriptableObject
{
    [Header("基本配置")]
    public string skillName;
    public string skillType;
    public string description;
    public Sprite skillIcon;
    public Sprite range;

    [Header("逻辑配置")]
    public SkillReleaseType releaseType;
    [Tooltip("当 releaseType 不为 SelfCentered 时，表示释放中心可选取的最大距离")]
    public int releaseRange;

    public SkillEffectAreaType effectAreaType;
    [Tooltip("当 effectAreaType 为 PatternBased 时，配置生效区域的图案数据")]
    public SkillAreaData areaData;

    public SkillTargetType targetType;

    [Header("技能数值")]
    [Tooltip("例如伤害值或治疗值")]
    public int powerValue;

    [Header("视觉配置")]
    [Tooltip("技能生效范围高亮时的格子颜色")]
    public Color effectCellColor = Color.red;

    /// <summary>
    /// 执行技能效果。  
    /// 参数说明：  
    /// caster：施法者  
    /// releaseCell：策划或系统选定的释放中心所在的格子  
    /// direction：对于 Directional 类型技能，可传入一个方向（例如 Vector2Int.up 表示上方）  
    /// </summary>
    public abstract void Execute(BaseCharacter caster, List<SkillTargetInfo> affectedTargets);
}