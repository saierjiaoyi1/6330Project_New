using UnityEngine;

/// <summary>
/// 格子状态枚举，后续可根据需要扩展其他状态
/// </summary>
public enum GridCellState
{
    Default,    // 默认状态
    Movable,    // 可移动格子（比如：角色可移动到的范围）
    Selected,   // 被选中的格子（鼠标移动到的格子）
    SkillRange, // 技能生效范围（高亮显示技能实际影响的格子）
    SkillArea   // 技能释放区域（高亮显示可选择释放中心的格子）
}

/// <summary>
/// 用于在 Inspector 中配置“状态 → Sprite”对应关系
/// </summary>
[System.Serializable]
public class GridCellSpriteMapping
{
    public GridCellState cellState;
    public Sprite sprite;
}

/// <summary>
/// 角色状态机的状态枚举
/// </summary>
public enum CharacterState
{
    Idle,       // 空闲状态
    Waiting,    // 等待输入（例如玩家等待指令或敌人决策）
    Moving,     // 正在移动
    Acting,     // 正在执行动作（攻击、释放技能等）
    SelectingSkill  // 正在选定技能释放目标
}

/// <summary>
/// 角色朝向枚举，共四个方向
/// </summary>
public enum Orientation
{
    Up,
    Right,
    Down,
    Left
}

//技能相关
public enum SkillReleaseType//技能释放位置
{
    SelfCentered,      // 只能以自身为中心释放
    FreeSelection,     // 可在一定范围内任意选取释放中心
    TargetUnitSelection// 必须选取一个单位所在的格子作为释放中心
}

public enum SkillEffectAreaType//技能释放方式
{
    Directional,   // 以自身为原点，按方向性拖动确定区域
    PatternBased   // 采用预设的格子图案来确定生效范围
}

public enum SkillTargetType//技能生效目标
{
    Enemies,  // 仅对敌人生效
    Allies,   // 仅对友军生效
    All       // 对所有单位生效
}

public enum Team//角色阵营
{
    Player,
    Enemy
}

public enum DamageType
{
    Normal,     // 普通伤害
    Fire,       // 火焰伤害
    Ice,        // 冰霜伤害
    Cut,        // Cut伤害
    Blunt       // Blunt伤害
    // 以后可以根据需要添加其它伤害类型
}