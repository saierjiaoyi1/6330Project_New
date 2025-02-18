using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "HeavyAttackSkill", menuName = "Skill/HeavyAttack", order = 2)]
public class HeavyAttackSkillSO : SkillSO
{
    public override void Execute(int diceValue, BaseCharacter caster, List<SkillTargetInfo> affectedTargets)
    {
        // 立即启动协程处理火球技能的动画和效果
        caster.StartCoroutine(FireballSkillCoroutine(diceValue, caster, affectedTargets));

    }

    private IEnumerator FireballSkillCoroutine(int diceValue, BaseCharacter caster, List<SkillTargetInfo> targets)
    {
        caster.currentState = CharacterState.Acting;
        Debug.Log($"{skillName} 开始播放动画..." + targets.Count);
        // 播放动画（此处用等待模拟动画播放时间）
        caster.PlayAttackAnim(2);
        yield return new WaitForSeconds(1f);
        
        // 在动画达到特定节点时，结算技能效果（造成伤害）
        float finalValue;
        if (diceValue == 12) finalValue = powerValue * 2.0f;
        else if (diceValue <= 11 && diceValue >= 9) finalValue = powerValue * 1.5f;
        else if (diceValue <= 8 && diceValue >= 3) finalValue = powerValue * 1.0f;
        else finalValue = powerValue * 0.8f;
        foreach (SkillTargetInfo targetInfo in targets)
        {
            if (targetInfo.cell != null && targetInfo.cell.occupant != null)
            {
                bool validTarget = false;
                switch (targetType)
                {
                    case SkillTargetType.Enemies:
                        validTarget = targetInfo.cell.occupant.team != caster.team;
                        break;
                    case SkillTargetType.Allies:
                        validTarget = targetInfo.cell.occupant.team == caster.team;
                        break;
                    case SkillTargetType.All:
                        validTarget = true;
                        break;
                }
                if (validTarget)
                {
                    // 此处可以根据 targetInfo.featureCode 施加不同效果
                    if (targetInfo.featureCode == 0)
                    {
                        targetInfo.cell.occupant.ReceiveDamage((int)finalValue, DamageType.Blunt);
                    }
                    
                }
            }
        }
        // 等待动画结束
        yield return new WaitForSeconds(1f);
        Debug.Log($"{skillName} 动画播放结束。");
        caster.currentState = CharacterState.Waiting;
        caster.EndTurn();
    }
}