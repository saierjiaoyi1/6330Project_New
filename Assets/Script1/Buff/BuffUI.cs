using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuffUI : MonoBehaviour
{
    public GameObject buffIconPrefab;

    private void Awake()
    {
        buffIconPrefab = Resources.Load<GameObject>("Prefabs/BuffIcon");
    }

    /// <summary>
    /// 根据当前激活的 buff 列表刷新 UI 显示
    /// </summary>
    public void RefreshUI(List<BuffInstance> activeBuffs)
    {
        // 清空当前所有子物体
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        // 遍历 activeBuffs，生成新的 Buff 图标
        foreach (var buff in activeBuffs)
        {
            if (buffIconPrefab == null || buff.buffData == null || buff.buffData.buffIcon == null)
            {
                Debug.LogWarning("BuffIconPrefab 为空，或者 BuffData 无效，跳过该 Buff");
                continue;
            }

            // 实例化 buffIconPrefab，并设置为 BuffUI 的子物体
            GameObject buffIcon = Instantiate(buffIconPrefab, transform);
            Image iconImage = buffIcon.GetComponent<Image>();

            if (iconImage != null)
            {
                iconImage.sprite = buff.buffData.buffIcon; // 设置 Buff 图标
            }
            else
            {
                Debug.LogWarning("buffIconPrefab 缺少 Image 组件，无法设置 Buff 图标");
            }
        }

        Debug.Log("刷新 BuffUI，当前 Buff 数量：" + activeBuffs.Count);
    }
}
