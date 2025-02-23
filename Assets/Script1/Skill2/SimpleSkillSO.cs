using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(menuName = "Simple Skills/Simple Skill")]
public class SimpleSkillSO : ScriptableObject
{
    [Header("基本配置")]
    public string skillName = "Test Skill";

    [Header("Task 配置")]
    // 使用 SerializeReference 来支持多态序列化，同时 InlineProperty 使每个子项直接展开显示
    [SerializeReference, InlineProperty, ListDrawerSettings(Expanded = true, DraggableItems = false)]
    public List<Task> tasks = new List<Task>();
}