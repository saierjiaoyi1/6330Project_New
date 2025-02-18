using UnityEngine;
using UnityEngine.UI;

public class TooltipUI : MonoBehaviour
{
    [Header("组件引用")]
    public Text skillNameText;
    public Text skillTypeText;
    public Image skillIconImage;
    public Text descriptionText;
    public Image rangeImage;

    /// <summary>
    /// 设置 tooltip 显示的信息
    /// </summary>
    /// <param name="skillName">技能名称</param>
    /// <param name="skillType">技能类型</param>
    /// <param name="skillIcon">技能图标</param>
    /// <param name="description">技能描述</param>
    /// <param name="rangeSprite">技能范围图片</param>
    public void SetSkillInfo(string skillName, string skillType, Sprite skillIcon, string description, Sprite rangeSprite)
    {
        if (skillNameText != null)
            skillNameText.text = skillName;
        if (skillTypeText != null)
            skillTypeText.text = skillType;
        if (skillIconImage != null)
            skillIconImage.sprite = skillIcon;
        if (descriptionText != null)
            descriptionText.text = description;
        if (rangeImage != null)
            rangeImage.sprite = rangeSprite;
    }

}
