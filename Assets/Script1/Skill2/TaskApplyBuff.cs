using System.Collections;
using UnityEngine;
using Sirenix.OdinInspector;

public enum BuffTarget { Self, GridBased }

[System.Serializable]
public class TaskApplyBuff : Task
{
    [LabelText("Buff 配置")]
    public BuffSO buff; // 在 Inspector 中选择具体的 Buff 配置（可选择上面任一派生类）

    [EnumToggleButtons]
    [LabelText("生效目标")]
    public BuffTarget target;

    [LabelText("目标格子特征码（仅用于 GridBased）")]
    public int targetFeatureCode;

    public override IEnumerator Execute(SkillContext context)
    {
        Debug.Log("TaskApplyBuff: 应用 buff " + buff.buffName + " 至 " + target);
        if (target == BuffTarget.Self)
        {
            context.caster.buffManager.ApplyBuff(buff);
        }
        else if (target == BuffTarget.GridBased)
        {
            // 遍历上下文中的目标格子，匹配特征码后应用 buff
            foreach (var t in context.targetInfos)
            {
                if (t.featureCode == targetFeatureCode)
                {
                    BaseCharacter bc = t.cell.GetComponent<BaseCharacter>();
                    if (bc != null)
                    {
                        bc.buffManager.ApplyBuff(buff);
                    }
                }
            }
        }
        yield break;
    }
}
