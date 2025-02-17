using UnityEngine;
using UnityEngine.UI;

public class CharacterSelection : MonoBehaviour
{
    [Header("显示角色信息的UI预制体，必须放在Canvas下（Screen Space - Overlay）")]
    public GameObject uiPrefab;

    // 运行时动态生成的UI实例
    private GameObject uiInstance;

    // 标记鼠标是否在模型上
    private bool isMouseOver = false;

    // 角色信息组件（挂在父物体上）
    private BaseCharacter baseCharacter;

    public Canvas canvas;

    private void Start()
    {
        // 尝试从父物体获取角色信息
        if (transform.parent != null)
        {
            baseCharacter = transform.parent.GetComponent<BaseCharacter>();
        }
        if (GetComponent<Outline>() != null)
        {
            GetComponent<Outline>().enabled = false;
        }

    }

    // 当鼠标进入本物体碰撞器范围
    private void OnMouseEnter()
    {
        isMouseOver = true;

        Outline outLine = GetComponent<Outline>();
        // 启用描边效果
        if (outLine != null)
        {
            outLine.enabled = true;
            outLine.OutlineColor = Color.white;
        }

        // 实例化UI预制体（如果还没有的话）
        if (uiPrefab != null && uiInstance == null)
        {
            uiInstance = Instantiate(uiPrefab, canvas.transform, false);
        }

        // 更新UI中的数据
        UpdateUI();
    }

    // 当鼠标离开本物体碰撞器范围
    private void OnMouseExit()
    {
        isMouseOver = false;

        if (GetComponent<Outline>() != null)
        {
            GetComponent<Outline>().enabled = false;
        }

        // 销毁UI实例
        if (uiInstance != null)
        {
            Destroy(uiInstance);
            uiInstance = null;
        }
    }

    private void Update()
    {
        // 当鼠标悬停时，使UI跟随鼠标
        if (isMouseOver && uiInstance != null)
        {
            RectTransform rt = uiInstance.GetComponent<RectTransform>();
            if (rt != null)
            {
                Vector3 offset = new Vector3(10f, -10f, 0f);
                rt.position = Input.mousePosition + offset;
            }
        }
    }

    // 更新UI界面显示角色信息
    private void UpdateUI()
    {
        if (baseCharacter != null && uiInstance != null)
        {
            // 获取预制体下面的文本框
            Text nameText = uiInstance.transform.Find("Name").GetComponent<Text>();
            Text hpText = uiInstance.transform.Find("HP").GetComponent<Text>();
            Text FireText = uiInstance.transform.Find("Fire").GetComponent<Text>();
            Text FrostText = uiInstance.transform.Find("Frost").GetComponent<Text>();
            Text CutText = uiInstance.transform.Find("Cut").GetComponent<Text>();
            Text BluntText = uiInstance.transform.Find("Blunt").GetComponent<Text>();

            if (nameText != null)
            {
                nameText.text = baseCharacter.Name;
            }
            if (hpText != null)
            {
                hpText.text = "HP: " + baseCharacter.currentHealth;
            }
            if (FireText != null)
            {
                FireText.text = "Fire Res: " + baseCharacter.fireResistance;
            }
            if (FrostText != null)
            {
                FrostText.text = "Frost Res: " + baseCharacter.iceResistance;
            }
            if (CutText != null)
            {
                CutText.text = "Cut Res: " + baseCharacter.CutResistance;
            }
            if (BluntText != null)
            {
                BluntText.text = "Blunt Res: " + baseCharacter.BluntResistance;
            }
        }
    }
}