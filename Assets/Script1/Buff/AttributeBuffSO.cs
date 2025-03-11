using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;

public enum AttributeType
{
    Attack,
    FireResistance,
    IceResistance,
    CutResistance,
    BluntResistance,
    MoveRange
    // 可根据需要扩展其它属性类型
}

public enum ModificationType
{
    Flat,
    Percentage
}

/// <summary>
/// 单个属性修改的数据结构
/// </summary>
[System.Serializable]
public class AttributeModification
{
    [LabelText("属性类型")]
    public AttributeType attributeType;

    [LabelText("修改类型")]
    public ModificationType modificationType;

    [LabelText("修改数值")]
    public float value;
}

[CreateAssetMenu(menuName = "Buffs/Attribute Buff")]
public class AttributeBuffSO : BuffSO
{
    [Header("属性修改配置")]
    [TableList]
    public List<AttributeModification> modifications = new List<AttributeModification>();

    private void OnEnable()
    {
        buffType = BuffType.AttributeModification;
    }

    public void OnApplyBuff(BaseCharacter baseCharacter)
    {
        foreach(AttributeModification var in modifications)
        {
            if(var.attributeType == AttributeType.Attack)
            {
                if(var.modificationType == ModificationType.Flat)
                {
                    baseCharacter.attack += var.value;
                }
                else if(var.modificationType == ModificationType.Percentage)
                {
                    baseCharacter.attack += baseCharacter.baseAttack * var.value;
                }
            }
            else if (var.attributeType == AttributeType.FireResistance)
            {
                if (var.modificationType == ModificationType.Flat)
                {
                    baseCharacter.fireResistance += var.value;
                }
                else if (var.modificationType == ModificationType.Percentage)
                {
                    baseCharacter.fireResistance += baseCharacter.baseFireResistance * var.value;
                }
            }
            else if (var.attributeType == AttributeType.IceResistance)
            {
                if (var.modificationType == ModificationType.Flat)
                {
                    baseCharacter.iceResistance += var.value;
                }
                else if (var.modificationType == ModificationType.Percentage)
                {
                    baseCharacter.iceResistance += baseCharacter.baseIceResistance * var.value;
                }
            }
            else if (var.attributeType == AttributeType.CutResistance)
            {
                if (var.modificationType == ModificationType.Flat)
                {
                    baseCharacter.CutResistance += var.value;
                }
                else if (var.modificationType == ModificationType.Percentage)
                {
                    baseCharacter.CutResistance += baseCharacter.baseCutResistance * var.value;
                }
            }
            else if (var.attributeType == AttributeType.BluntResistance)
            {
                if (var.modificationType == ModificationType.Flat)
                {
                    baseCharacter.BluntResistance += var.value;
                }
                else if (var.modificationType == ModificationType.Percentage)
                {
                    baseCharacter.BluntResistance += baseCharacter.baseBluntResistance * var.value;
                }
            }
        }
    }
    public void OnRemoveBuff(BaseCharacter baseCharacter)
    {
        foreach (AttributeModification var in modifications)
        {
            if (var.attributeType == AttributeType.Attack)
            {
                if (var.modificationType == ModificationType.Flat)
                {
                    baseCharacter.attack -= var.value;
                }
                else if (var.modificationType == ModificationType.Percentage)
                {
                    baseCharacter.attack -= baseCharacter.baseAttack * var.value;
                }
            }
            else if (var.attributeType == AttributeType.FireResistance)
            {
                if (var.modificationType == ModificationType.Flat)
                {
                    baseCharacter.fireResistance -= var.value;
                }
                else if (var.modificationType == ModificationType.Percentage)
                {
                    baseCharacter.fireResistance -= baseCharacter.baseFireResistance * var.value;
                }
            }
            else if (var.attributeType == AttributeType.IceResistance)
            {
                if (var.modificationType == ModificationType.Flat)
                {
                    baseCharacter.iceResistance -= var.value;
                }
                else if (var.modificationType == ModificationType.Percentage)
                {
                    baseCharacter.iceResistance -= baseCharacter.baseIceResistance * var.value;
                }
            }
            else if (var.attributeType == AttributeType.CutResistance)
            {
                if (var.modificationType == ModificationType.Flat)
                {
                    baseCharacter.CutResistance -= var.value;
                }
                else if (var.modificationType == ModificationType.Percentage)
                {
                    baseCharacter.CutResistance -= baseCharacter.baseCutResistance * var.value;
                }
            }
            else if (var.attributeType == AttributeType.BluntResistance)
            {
                if (var.modificationType == ModificationType.Flat)
                {
                    baseCharacter.BluntResistance -= var.value;
                }
                else if (var.modificationType == ModificationType.Percentage)
                {
                    baseCharacter.BluntResistance -= baseCharacter.baseBluntResistance * var.value;
                }
            }
        }
    }
}
