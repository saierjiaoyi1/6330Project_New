using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;

/// <summary>
/// 播放动画 Task：通过传入的 trigger 名称触发状态机动画。
/// </summary>

[System.Serializable]
public class TaskPlayAnim : Task
{
    public string triggerName; // 状态机中动画的触发器名称

    public override IEnumerator Execute(SkillContext context)
    {
        Debug.Log("TaskPlayAnimation: 播放触发器 " + triggerName);
        if (context.caster != null && context.caster.GetComponentInChildren<Animator>() != null)
        {
            context.caster.GetComponentInChildren<Animator>().SetTrigger(triggerName);
        }
        yield break;
    }
}