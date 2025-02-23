using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Buff.cs：抽象的buff逻辑基类
public abstract class Buff
{
    public BuffData data; // 数据引用
    public float remainingDuration;

    public Buff(BuffData data)
    {
        this.data = data;
        this.remainingDuration = data.baseDuration;
    }

    // 当buff被添加到单位时调用
    public virtual void OnApply(BaseCharacter unit) { }

    // 每回合开始时调用
    public virtual void OnTurnStart(BaseCharacter unit) { }

    // 每回合结束时调用
    public virtual void OnTurnEnd(BaseCharacter unit) { }

    // 当buff被移除时调用
    public virtual void OnRemove(BaseCharacter unit) { }
}

[CreateAssetMenu(menuName = "Buff/BuffData")]
public class BuffData : ScriptableObject
{
    public string buffName;
    public string description;
    public Sprite icon;
    public float baseDuration;
    // 其他需要的数据
}

// BuffManager.cs：角色身上的buff管理组件
public class BuffManager : MonoBehaviour
{
    private List<Buff> activeBuffs = new List<Buff>();

    // 添加buff
    public void AddBuff(Buff newBuff, BaseCharacter unit)
    {
        // 可在这里处理叠加规则或替换逻辑
        activeBuffs.Add(newBuff);
        newBuff.OnApply(unit);
        // 通知UI更新buff图标和描述
    }

    // 每回合开始更新buff
    public void OnTurnStart(BaseCharacter unit)
    {
        for (int i = activeBuffs.Count - 1; i >= 0; i--)
        {
            activeBuffs[i].OnTurnStart(unit);
            activeBuffs[i].remainingDuration -= 1; // 根据回合数递减
            if (activeBuffs[i].remainingDuration <= 0)
            {
                activeBuffs[i].OnRemove(unit);
                activeBuffs.RemoveAt(i);
                // 通知UI移除buff显示
            }
        }
    }
}
