using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class LevelSelectionUI : MonoBehaviour
{
    [Header("动态生成按钮的容器（带 Layout 组件）")]
    public Transform buttonContainer;
    [Header("关卡按钮预制体")]
    public GameObject levelButtonPrefab;
    [Header("下方的 Play 按钮")]
    public Button playButton;

    // 内部保存所有生成的按钮引用
    private List<LevelButton> levelButtons = new List<LevelButton>();
    // 当前选中的关卡索引
    private int selectedLevelIndex = -1;
    // 当前选中的按钮引用，用于更新高亮状态
    private LevelButton selectedButton;

    private void Start()
    {
        // 从 GameManager 中获取关卡列表
        List<LevelInfo> levels = GameManager.Instance.levels;
        for (int i = 0; i < levels.Count; i++)
        {
            // 实例化按钮预制体，并设置为 buttonContainer 的子对象
            GameObject btnObj = Instantiate(levelButtonPrefab, buttonContainer);
            LevelButton lb = btnObj.GetComponent<LevelButton>();
            // 初始化按钮，传入关卡信息、索引和当前 UI 脚本引用
            lb.Setup(levels[i], i, this);
            levelButtons.Add(lb);
        }

        playButton.onClick.AddListener(OnPlayButtonClicked);
    }

    /// <summary>
    /// 由每个关卡按钮调用，表示某个关卡被选中
    /// </summary>
    public void SelectLevel(int index, LevelButton button)
    {
        // 取消之前按钮的高亮
        if (selectedButton != null)
        {
            selectedButton.SetHighlighted(false);
        }
        selectedLevelIndex = index;
        selectedButton = button;
        selectedButton.SetHighlighted(true);

        // 更新全局选中的关卡（方便“下一关”等逻辑）
        GameManager.Instance.SelectedLevelIndex = index;
    }

    private void OnPlayButtonClicked()
    {
        if (selectedLevelIndex >= 0)
        {
            GameManager.Instance.PlaySelectedLevel();
        }
        else
        {
            Debug.LogWarning("请选择一个关卡！");
        }
    }
}
