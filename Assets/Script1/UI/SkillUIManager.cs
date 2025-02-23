using UnityEngine;
using UnityEngine.UI;

public class SkillUIManager : MonoBehaviour
{
    public static SkillUIManager Instance;

    [Header("UI引用")]
    // 技能按钮生成容器（带有 Layout 组件的父物体）
    public Transform skillButtonContainer;
    // 技能按钮预制体（包含 Button、Text 组件）
    public GameObject skillButtonPrefab;
    // Tooltip 预制体（包含 TooltipUI 脚本）
    public GameObject tooltipPrefab;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    /// <summary>
    /// 根据传入的玩家角色生成技能按钮
    /// </summary>
    /// <param name="player">当前回合的玩家角色（需包含 SkillList 与 PlayerSkillController）</param>
    public void ShowSkillsForPlayer(BaseCharacter player)
    {
        // 先清空之前生成的按钮
        foreach (Transform child in skillButtonContainer)
        {
            Destroy(child.gameObject);
        }

        // 遍历玩家的技能列表，生成对应的按钮
        for (int i = 0; i < player.skillList.Count; i++)
        {
            int skillIndex = i; // 防止闭包问题
            SkillSO2 skill = player.skillList[skillIndex];

            // 实例化技能按钮预制体，并设置其父物体
            GameObject buttonObj = Instantiate(skillButtonPrefab, skillButtonContainer);

            // 设置按钮显示的文本为技能名称
            Text buttonText = buttonObj.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                buttonText.text = skill.skillName;
            }

            // 为按钮添加点击事件：调用 player.PlayerSkillController.SelectSkill(index)
            Button btn = buttonObj.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() =>
                {
                    player.gameObject.GetComponent<PlayerSkillController>().SelectSkill(skillIndex);
                });
            }

            // 添加 SkillButtonController 脚本，用于处理鼠标悬停显示 tooltip 的逻辑
            SkillButtonController sbc = buttonObj.AddComponent<SkillButtonController>();
            sbc.skill = skill;
            sbc.tooltipPrefab = tooltipPrefab;
        }
    }
}
