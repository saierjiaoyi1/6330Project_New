using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LevelButton : MonoBehaviour
{
    [Header("显示关卡名称的文本")]
    public Text levelNameText;
    [Header("按钮组件")]
    public Button button;

    private int levelIndex;
    private LevelSelectionUI levelSelectionUI;

    /// <summary>
    /// 初始化按钮显示和事件绑定
    /// </summary>
    public void Setup(LevelInfo info, int index, LevelSelectionUI ui)
    {
        levelIndex = index;
        levelSelectionUI = ui;
        levelNameText.text = info.levelName;

        // 给按钮添加点击事件
        button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        // 通知关卡选择 UI 本按钮被点击
        levelSelectionUI.SelectLevel(levelIndex, this);
    }

    /// <summary>
    /// 设置按钮高亮状态
    /// </summary>
    public void SetHighlighted(bool highlighted)
    {

        if(highlighted)
        {
            button.Select();
        }
        else
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }
}
