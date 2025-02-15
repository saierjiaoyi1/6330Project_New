using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DamageText : MonoBehaviour
{
    [Header("UI组件")]
    // 显示伤害数字的 Text 组件（如果你使用 TextMeshPro，请替换成对应组件）
    public Text damageText;

    // 内部变量：浮动速度和显示持续时间（可通过 Init 设置）
    private float floatSpeed;
    private float duration;

    // 本对象的 RectTransform
    private RectTransform rectTransform;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    /// <summary>
    /// 初始化伤害数字显示
    /// </summary>
    /// <param name="damage">伤害数值</param>
    /// <param name="color">数字颜色</param>
    /// <param name="worldPosition">角色的世界坐标，用于转换到屏幕坐标</param>
    /// <param name="customFloatSpeed">浮起速度（单位：像素/秒）</param>
    /// <param name="customDuration">显示持续时间（秒）</param>
    public void Init(int damage, Color color, Vector3 worldPosition, float customFloatSpeed, float customDuration)
    {
        // 设置文本，前面加上减号
        damageText.text = "-" + damage.ToString();
        damageText.color = color;

        floatSpeed = customFloatSpeed;
        duration = customDuration;

        // 将角色的世界坐标转换为屏幕坐标（因为Canvas为Screen Space Overlay）
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPosition);
        rectTransform.position = screenPos;

        // 启动数字浮动和渐隐的协程
        StartCoroutine(FloatAndFade());
    }

    /// <summary>
    /// 协程：控制数字从当前位置向上浮动，同时渐隐，最后销毁预制件
    /// </summary>
    IEnumerator FloatAndFade()
    {
        float elapsedTime = 0f;
        // 记录初始位置
        Vector3 startPos = rectTransform.position;
        // 根据浮起速度和持续时间计算目标位置（向上移动）
        Vector3 endPos = startPos + Vector3.up * floatSpeed * duration;

        // 记录初始颜色
        Color startColor = damageText.color;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;

            // 插值计算位置：从 startPos 到 endPos
            rectTransform.position = Vector3.Lerp(startPos, endPos, t);
            // 插值计算颜色：alpha 从 1 渐变到 0
            damageText.color = new Color(startColor.r, startColor.g, startColor.b, Mathf.Lerp(1, 0, t));

            yield return null;
        }

        // 动画结束后销毁该对象
        Destroy(gameObject);
    }
}