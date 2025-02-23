using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using System;
using Sirenix.Serialization;
using System.Collections;

[CreateAssetMenu(menuName = "Skills/Skill")]
public class SkillSO2 : ScriptableObject
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

    [Header("视觉配置（现在没用）")]
    [Tooltip("技能生效范围高亮时的格子颜色")]
    public Color effectCellColor = Color.red;

    [Header("技能播动画类型")]
    public int skillAnimType;

    [Header("Task 配置")]
    [ShowInInspector, SerializeReference]
    public List<Task> taskList;

    /// <summary>
    /// 执行技能效果。
    /// 由于技能的逻辑由 taskList 中的 Task 组合实现，此方法仅用于数据校验或调试提示，
    /// 实际的 Task 执行由 SkillController 中的协程遍历 taskList 完成。
    /// </summary>
    public IEnumerator Execute(int diceValue, BaseCharacter caster, List<SkillTargetInfo> affectedTargets)
    {
        Debug.Log("SkillSO2");
        // 创建技能执行上下文，可以在其中扩展更多数据（如骰子结果等）
        SkillContext context = new SkillContext();
        context.caster = caster;
        context.targetInfos = affectedTargets;
        // 如果需要，也可以添加 context.diceValue = diceValue; 等

        if (taskList != null)
        {
            foreach (Task task in taskList)
            {
                // 依次执行每个 Task 的协程
                yield return task.Execute(context);
            }
        }
        else
        {
            Debug.LogWarning("SkillSO.Execute：taskList 为空！");
        }
    }
}
