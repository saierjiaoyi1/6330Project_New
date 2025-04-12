using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;

/// <summary>
/// 造成伤害 Task：根据固定伤害与攻击力加成对目标造成伤害。
/// </summary>

[System.Serializable]
public class TaskDamage : Task
{
    [EnumToggleButtons]
    [LabelText("伤害类型")]
    public DamageType damageType;

    public float fixedDamage = 10f;

    public float attackMultiplier = 1f;

    [EnumToggleButtons]

    public Team targetTeam;
    public int targetFeatureCode;

    public override IEnumerator Execute(SkillContext context)
    {
        Debug.Log("TaskDamage: 类型=" + damageType +
                  " 固定伤害=" + fixedDamage +
                  " 加成系数=" + attackMultiplier);
        // 计算总伤害：固定伤害 + (加成系数 * 角色攻击力)
        float totalDamage = fixedDamage + attackMultiplier * context.caster.attack;
	if (context.diceValue == 12) totalDamage = totalDamage * 2.0f;
        else if (context.diceValue <= 11 && context.diceValue >= 9) totalDamage = totalDamage * 1.5f;
        else if (context.diceValue <= 8 && context.diceValue>= 3) totalDamage = totalDamage * 1.0f;
        else totalDamage = totalDamage * 0.8f;

        // 遍历所有技能生效范围内的格子，检查特征码（目标队伍的判断可在此处补充）
        foreach (var target in context.targetInfos)
        {
            Debug.Log(target.featureCode + " " + target.cell.occupant);
            if (target.cell.occupant != null && target.featureCode == targetFeatureCode)
            {
                if (target.cell.occupant.team == targetTeam)
                {
                    Debug.Log("对特征码 " + target.featureCode + " 目标造成 " + totalDamage +
                                              " 点 " + damageType + " 伤害");
                    // 可调用目标角色的受伤接口，例如：
                    target.cell.occupant.GetComponent<BaseCharacter>()?.ReceiveDamage(totalDamage, damageType);
                }
                
            }
        }
        yield break;
    }
}