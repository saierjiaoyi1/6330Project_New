using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffManager : MonoBehaviour
{
    // 当前角色身上生效的 buff 列表（以 BuffInstance 形式记录）
    public List<BuffInstance> activeBuffs = new List<BuffInstance>();

    // 引用显示 buff 图标的 UI 组件（请自行实现或绑定）
    public BuffUI buffUI;

    // 角色脚本
    private BaseCharacter baseCharacter;

    private void Start()
    {
        baseCharacter = GetComponent<BaseCharacter>();
    }

    /// <summary>
    /// 添加并应用一个 buff
    /// </summary>
    public void ApplyBuff(BuffSO buffData)
    {
        BuffInstance newBuff = new BuffInstance(buffData);
        if (buffData.ifOnlyOne)
        {
            for (int i = activeBuffs.Count - 1; i >= 0; i--)
            {
                if (activeBuffs[i].buffData == buffData)
                {
                    RemoveBuff(activeBuffs[i]);
                }
            }
        }
        activeBuffs.Add(newBuff);
        Debug.Log("Applied buff: " + buffData.buffName);

        // 对于属性修改 buff，立即应用属性加成
        if (buffData is AttributeBuffSO)
        {
            ApplyAttributeBuff(buffData as AttributeBuffSO);
        }

        RefreshBuffUI();
    }

    /// <summary>
    /// 移除一个 buff 实例，并撤销其效果
    /// </summary>
    public void RemoveBuff(BuffInstance buffInstance)
    {
        activeBuffs.Remove(buffInstance);
        Debug.Log("Removed buff: " + buffInstance.buffData.buffName);

        if (buffInstance.buffData is AttributeBuffSO)
        {
            RemoveAttributeBuff(buffInstance.buffData as AttributeBuffSO);
        }

        RefreshBuffUI();
    }

    /// <summary>
    /// 在角色回合开始时调用，处理回合开始时的 buff 效果
    /// </summary>
    public void OnTurnStart()
    {
        foreach (var buff in activeBuffs)
        {
            if (buff.buffData is TurnStartDamageBuffSO)
            {
                TurnStartDamageBuffSO tsBuff = buff.buffData as TurnStartDamageBuffSO;
                Debug.Log("Turn start damage from buff: " + tsBuff.buffName);
                // 假设角色有 ReceiveDamage 方法
                BaseCharacter character = GetComponent<BaseCharacter>();
                if (character != null)
                {
                    character.ReceiveDamage(tsBuff.damageValue, tsBuff.damageType);
                }
            }
        }
    }

    /// <summary>
    /// 在角色回合结束时调用，处理回合结束时的 buff 效果，并减少持续回合数
    /// </summary>
    public void OnTurnEnd()
    {
        // 先处理回合结束时的伤害效果，然后减少 buff 持续回合数，最后移除过期的 buff
        List<BuffInstance> buffsToRemove = new List<BuffInstance>();

        foreach (var buff in activeBuffs)
        {
            if (buff.buffData is TurnEndDamageBuffSO)
            {
                TurnEndDamageBuffSO teBuff = buff.buffData as TurnEndDamageBuffSO;
                Debug.Log("Turn end damage from buff: " + teBuff.buffName);
                BaseCharacter character = GetComponent<BaseCharacter>();
                if (character != null)
                {
                    character.ReceiveDamage(teBuff.damageValue, teBuff.damageType);
                }
            }
            // 每个 buff 的剩余回合数减 1
            buff.remainingRounds--;
            if (buff.remainingRounds <= 0)
            {
                buffsToRemove.Add(buff);
            }
        }

        foreach (var buff in buffsToRemove)
        {
            RemoveBuff(buff);
        }
    }

    /// <summary>
    /// 立即应用属性修改 buff的效果（具体逻辑需根据角色属性系统实现）
    /// </summary>
    private void ApplyAttributeBuff(AttributeBuffSO attrBuff)
    {
        Debug.Log("Applying attribute buff: " + attrBuff.buffName);
        // TODO: 根据 attrBuff.modifications 的配置调整角色属性
        attrBuff.OnApplyBuff(baseCharacter);

    }

    /// <summary>
    /// 撤销属性修改 buff 的效果
    /// </summary>
    private void RemoveAttributeBuff(AttributeBuffSO attrBuff)
    {
        Debug.Log("Removing attribute buff: " + attrBuff.buffName);
        // TODO: 撤销之前应用的属性修改
        attrBuff.OnRemoveBuff(baseCharacter);
    }

    /// <summary>
    /// 刷新角色上显示 buff 图标的 UI 组件
    /// </summary>
    public void RefreshBuffUI()
    {
        if (buffUI != null)
        {
            buffUI.RefreshUI(activeBuffs);
        }
    }
}
  