using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;

/// <summary>
/// Wait Task：等待一段时间后再执行后续 Task。
/// </summary>
[System.Serializable]
public class TaskWait : Task
{
    [LabelText("等待时长 (秒)")]
    public float duration;  // 等待时长（单位：秒）

    public override IEnumerator Execute(SkillContext context)
    {
        Debug.Log("TaskWait: 等待 " + duration + " 秒");
        yield return new WaitForSeconds(duration);
    }
}