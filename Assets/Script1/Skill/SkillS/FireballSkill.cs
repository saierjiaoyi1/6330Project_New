using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "FireballSkill", menuName = "Skill/Fireball", order = 2)]
public class FireballSkillSO : SkillSO
{
    public override void Execute(BaseCharacter caster, List<SkillTargetInfo> affectedTargets)
    {
        // 立即启动协程处理火球技能的动画和效果
        caster.StartCoroutine(FireballSkillCoroutine(caster, affectedTargets));
        
    }

    private IEnumerator FireballSkillCoroutine(BaseCharacter caster, List<SkillTargetInfo> targets)
    {
        caster.currentState = CharacterState.Acting;
        Debug.Log($"{skillName} 开始播放动画...");
        // 播放动画（此处用等待模拟动画播放时间）
        yield return new WaitForSeconds(1f);
        // 在动画达到特定节点时，结算技能效果
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
                    Debug.Log($"{skillName} 对 {targetInfo.cell.occupant.name} 造成 {powerValue} 点伤害 (featureCode: {targetInfo.featureCode})");
                    //targetInfo.cell.occupant.health -= powerValue;
                    targetInfo.cell.occupant.ReceiveDamage(10, DamageType.Fire);
                    // 此处可以根据 targetInfo.featureCode 施加不同效果
                }
            }
        }
        // 等待动画结束
        yield return new WaitForSeconds(0.5f);
        Debug.Log($"{skillName} 动画播放结束。");
        caster.currentState = CharacterState.Waiting;
        caster.EndTurn();
    }
}