using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SkillButtonController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
{
    [HideInInspector] public SkillSO2 skill;              // 对应的技能数据（从 PlayerCharacter.SkillList 中传入）
    [HideInInspector] public GameObject tooltipPrefab;   // tooltip 预制体

    private GameObject tooltipInstance;  // 当前生成的 tooltip 实例

    /// <summary>
    /// 鼠标指针进入按钮时生成 tooltip 并填充数据
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltipPrefab != null && tooltipInstance == null)
        {
            // 将 tooltip 实例化到当前 Canvas 下，确保跟随鼠标
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                tooltipInstance = Instantiate(tooltipPrefab, canvas.transform);
            }
            else
            {
                tooltipInstance = Instantiate(tooltipPrefab);
            }

            // 填充 tooltip 的信息
            TooltipUI tooltipUI = tooltipInstance.GetComponent<TooltipUI>();
            if (tooltipUI != null)
            {
                tooltipUI.SetSkillInfo(skill.skillName, skill.skillType, skill.skillIcon, skill.description, skill.range);
            }

            // 初始设置 tooltip 位置
            UpdateTooltipPosition(eventData);
        }
    }

    /// <summary>
    /// 鼠标在按钮上移动时，更新 tooltip 位置
    /// </summary>
    public void OnPointerMove(PointerEventData eventData)
    {
        UpdateTooltipPosition(eventData);
    }

    /// <summary>
    /// 鼠标离开按钮时销毁 tooltip
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltipInstance != null)
        {
            Destroy(tooltipInstance);
            tooltipInstance = null;
        }
    }

    /// <summary>
    /// 根据鼠标屏幕坐标更新 tooltip 的位置，设置一个偏移，使 tooltip 以右下角为锚点显示
    /// </summary>
    private void UpdateTooltipPosition(PointerEventData eventData)
    {
        if (tooltipInstance != null)
        {
            RectTransform tooltipRect = tooltipInstance.GetComponent<RectTransform>();
            // 计算偏移：这里以 tooltip 的宽度为偏移（向左偏移）
            Vector3 eventPos = new Vector3(eventData.position.x, eventData.position.y, 0);
            Vector3 offset = new Vector3(0, 40, 0);
            tooltipInstance.transform.position = eventPos + offset;
        }
    }
    private void OnDisable()
    {
        Destroy(tooltipInstance);
    }
}
