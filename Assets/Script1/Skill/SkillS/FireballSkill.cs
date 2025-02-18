using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "FireballSkill", menuName = "Skill/Fireball", order = 2)]
public class FireballSkillSO : SkillSO
{
    public override void Execute(int diceValue, BaseCharacter caster, List<SkillTargetInfo> affectedTargets)
    {
        // 立即启动协程处理火球技能的动画和效果
        caster.StartCoroutine(FireballSkillCoroutine(diceValue, caster, affectedTargets));
        
    }

    private IEnumerator FireballSkillCoroutine(int diceValue, BaseCharacter caster, List<SkillTargetInfo> targets)
    {
        caster.currentState = CharacterState.Acting;
        Debug.Log($"{skillName} 开始播放动画...");
        // 播放动画（此处用等待模拟动画播放时间）
        caster.PlayAttackAnim(4);
        yield return new WaitForSeconds(1f);
        // 在动画达到特定节点时，结算技能效果

        List<BaseCharacter> validTargets = new List<BaseCharacter>();
        foreach (SkillTargetInfo targetInfo in targets)
        {
            if (targetInfo.cell != null && targetInfo.cell.occupant != null)
            {
                BaseCharacter target = targetInfo.cell.occupant;
                bool isValid = false;
                switch (targetType)
                {
                    case SkillTargetType.Enemies:
                        isValid = (target.team != caster.team);
                        break;
                    case SkillTargetType.Allies:
                        isValid = (target.team == caster.team);
                        break;
                    case SkillTargetType.All:
                        isValid = true;
                        break;
                }

                // 如果是有效目标且还未添加进列表，则添加
                if (isValid && !validTargets.Contains(target))
                {
                    validTargets.Add(target);
                }
            }
        }
        float finalValue;
        if (diceValue == 12) finalValue = powerValue * 2.0f;
        else if(diceValue <= 11 && diceValue >= 9) finalValue = powerValue * 1.5f;
        else if (diceValue <= 8 && diceValue >= 3) finalValue = powerValue * 1.0f;
        else finalValue = powerValue * 0.8f;

        // 依次对缓存的目标施加伤害
        foreach (BaseCharacter target in validTargets)
        {
            Debug.Log($"{skillName} 对 {target.name} 造成 {powerValue} 点伤害");
            target.ReceiveDamage((int)finalValue, DamageType.Fire);
        }
        // 等待动画结束
        yield return new WaitForSeconds(1f);
        Debug.Log($"{skillName} 动画播放结束。");
        caster.currentState = CharacterState.Waiting;
        caster.EndTurn();
    }
}