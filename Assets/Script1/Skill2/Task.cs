using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 所有 Task 的基类，定义了统一的 Execute 接口。
/// </summary>
[System.Serializable]
public abstract class Task
{
    /// <summary>
    /// 执行当前 Task 的逻辑，接收一个 SkillContext 作为上下文数据。
    /// </summary>
    public abstract IEnumerator Execute(SkillContext context);
}

/// <summary>
/// 技能执行时的上下文，包含了释放者、目标信息等数据。
/// </summary>
public class SkillContext
{
    public BaseCharacter caster;                // 释放技能的角色
    public List<SkillTargetInfo> targetInfos;     // 技能生效范围内所有格子的信息
    // 根据需要扩展其它上下文数据
}
