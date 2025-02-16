using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StartMenuUI : MonoBehaviour
{
    [Header("UI 按钮")]
    public Button levelSelectButton;
    public Button quitButton;

    [Header("关卡选择页面的 Scene 名称")]
    public string levelSelectionSceneName = "LevelSelection";

    private void Start()
    {
        levelSelectButton.onClick.AddListener(OnLevelSelectClicked);
        quitButton.onClick.AddListener(OnQuitClicked);
    }

    private void OnLevelSelectClicked()
    {
        // 切换到关卡选择页面
        SceneManager.LoadScene(levelSelectionSceneName);
    }

    private void OnQuitClicked()
    {
        // 退出游戏
        Application.Quit();
    }
}
