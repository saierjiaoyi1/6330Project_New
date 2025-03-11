using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(menuName = "Buffs/Turn Start Damage Buff")]
public class TurnStartDamageBuffSO : BuffSO
{
    [Header("伤害配置")]
    [LabelText("伤害类型")]
    public DamageType damageType;

    [LabelText("伤害数值")]
    public float damageValue;

    private void OnEnable()
    {
        buffType = BuffType.TurnStartDamage;
    }
}
