using UnityEngine;
using Sirenix.OdinInspector;
using System;

public enum BuffType
{
    AttributeModification,
    TurnStartDamage,
    TurnEndDamage
}

/// <summary>
/// Buff 配置的基类，包含 buff 名称、描述、图标、持续回合数和类型。
/// </summary>
public abstract class BuffSO : ScriptableObject
{
    [Header("基本配置")]
    [LabelText("Buff 名称")]
    public string buffName;

    [TextArea]
    [LabelText("Buff 描述")]
    public string buffDescription;

    [LabelText("Buff 图标")]
    public Sprite buffIcon;

    [Tooltip("持续回合数（回合结束时减 1，当减至 0 时消失）")]
    public int duration;

    [Header("Buff 类型")]
    [LabelText("Buff 类型")]
    public BuffType buffType;

    [Header("Buff是否只能存在一个")]
    public bool ifOnlyOne;
}
