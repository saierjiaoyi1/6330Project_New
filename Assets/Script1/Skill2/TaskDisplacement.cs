using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 位移 Task：角色沿直线移动至指定格子（根据传入特征码列表依次检测可用目标格子）。
/// </summary>
[System.Serializable]
public class TaskDisplacement : Task
{
    public float speed = 5f;  // 移动速度
    public AnimationCurve curve = AnimationCurve.Linear(0, 0, 1, 1);  // 位移平滑曲线
    // 目标格子特征码列表，按优先级排列（依次检测，找到第一个未被占用的格子作为终点）
    public List<int> targetGridFeatureCodes;

    public override IEnumerator Execute(SkillContext context)
    {
        Debug.Log("TaskDisplacement: 开始位移任务");
        GridCell targetCell = null;
        // 检测传入特征码列表，查找第一个有效且未被占用的格子
        foreach (int code in targetGridFeatureCodes)
        {
            foreach (var target in context.targetInfos)
            {
                if (target.featureCode == code && target.cell != null && !target.cell.occupant)
                {
                    targetCell = target.cell;
                    break;
                }
            }
            if (targetCell != null)
                break;
        }

        if (targetCell == null)
        {
            Debug.LogWarning("TaskDisplacement: 未找到有效的目标格子");
            yield break;
        }

        // 位移过程：从释放者当前位置平滑移动到目标格子位置
        Transform casterTransform = context.caster.transform;
        Vector3 startPos = casterTransform.position;
        Vector3 endPos = targetCell.transform.position;
        float distance = Vector3.Distance(startPos, endPos);
        float travelTime = distance / speed;
        float elapsed = 0f;

        while (elapsed < travelTime)
        {
            elapsed += Time.deltaTime;
            // 通过曲线插值控制运动，使位移更自然
            float t = curve.Evaluate(Mathf.Clamp01(elapsed / travelTime));
            casterTransform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }
        // 精确定位到目标格子
        casterTransform.position = endPos;
        context.caster.SnapToNearestGridCell();
        Debug.Log("TaskDisplacement: 位移任务完成");
        yield break;
    }
}
