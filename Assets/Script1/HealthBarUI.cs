using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [Header("跟踪目标：敌人或玩家的 Transform")]
    public Transform target;

    [Header("世界坐标偏移（需要根据角色模型高度调整）")]
    public Vector3 worldOffset = new Vector3(0, 2.5f, 0);

    [Header("血条UI组件（Slider）")]
    public Slider healthSlider;

    private BaseCharacter baseChar;  // 获取目标身上的BaseCharacter组件
    private Camera mainCam;

    void Start()
    {
        if (target == null)
        {
            Debug.LogError("HealthBarUI：未设置目标！");
            enabled = false;
            return;
        }

        baseChar = target.GetComponent<BaseCharacter>();
        if (baseChar == null)
        {
            Debug.LogError("目标 " + target.name + " 上没有 BaseCharacter 组件！");
        }

        mainCam = Camera.main;
    }

    void Update()
    {
        // 将目标位置加上偏移转换为屏幕坐标，保证血条始终悬浮在角色上方
        Vector3 worldPos = target.position + worldOffset;
        Vector3 screenPos = mainCam.WorldToScreenPoint(worldPos);
        transform.position = screenPos;

        // 根据目标当前血量更新血条显示（Slider 的值在 0～1 之间）
        if (healthSlider != null && baseChar != null)
        {
            healthSlider.value = baseChar.currentHealth / baseChar.health;
        }
    }
}
