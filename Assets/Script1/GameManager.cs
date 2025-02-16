using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    /// <summary>
    /// 是否允许鼠标输入（例如移动、选格子等）
    /// </summary>
    [Tooltip("当前是否允许鼠标点击")]
    public bool mouseInputEnabled = true;

    // 存储所有关卡信息
    [Header("关卡配置")]
    public List<LevelInfo> levels = new List<LevelInfo>();

    // 当前选中的关卡索引（在关卡选择页或关卡通过后更新）
    [HideInInspector]
    public int SelectedLevelIndex = -1;

    private void Awake()
    {
        Instance = this;
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        DontDestroyOnLoad(this.gameObject);
    }

    /*
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void InitGameManager()
    {
        if (Instance == null)
        {
            GameObject go = new GameObject("GameManager");
            go.AddComponent<GameManager>();
        }
    }
    */

    /// <summary>
    /// 加载指定 scene
    /// </summary>
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// 根据当前选中的关卡加载对应的 scene
    /// </summary>
    public void PlaySelectedLevel()
    {
        if (SelectedLevelIndex >= 0 && SelectedLevelIndex < levels.Count)
        {
            LoadScene(levels[SelectedLevelIndex].sceneName);
        }
        else
        {
            Debug.LogWarning("没有选中的关卡或关卡索引无效！");
        }
    }

    /// <summary>
    /// 进入下一关（假设关卡顺序在 levels 中排列）
    /// </summary>
    public void LoadNextLevel()
    {
        if (SelectedLevelIndex >= 0 && SelectedLevelIndex < levels.Count - 1)
        {
            SelectedLevelIndex++;
            LoadScene(levels[SelectedLevelIndex].sceneName);
        }
        else
        {
            Debug.Log("已经是最后一关或未设置关卡！");
        }
    }
}