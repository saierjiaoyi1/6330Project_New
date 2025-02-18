using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;

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

    [Header("俩骰子")]
    public DiceRoll dice1;
    public DiceRoll dice2;

    private int? dice1Result = null;
    private int? dice2Result = null;

    private Action<int, int> onRollCompleteCallback; // 回调函数

    private void Awake()
    {
        
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(this.gameObject);
    }

    private void Start()
    {
        if (dice1 != null) dice1.OnRollComplete += HandleDiceRoll;
        if (dice2 != null) dice2.OnRollComplete += HandleDiceRoll;
    }

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

    //重新开始这一关
    public void Restart()
    {
        LoadScene(levels[SelectedLevelIndex].sceneName);
    }

    //回到主菜单
    public void ComeBack()
    {
        LoadScene("Main Menu");
    }

    //roll骰子
    public async Task<(int, int)> RollDice()
    {
        dice1Result = null;
        dice2Result = null;

        dice1?.StartRoll();
        dice2?.StartRoll();

        // **等待两个骰子都完成**
        await WaitForDiceResults();

        return (dice1Result.Value, dice2Result.Value);
    }
    private void HandleDiceRoll(DiceRoll dice, int result)
    {
        if (dice == dice1) dice1Result = result;
        if (dice == dice2) dice2Result = result;
    }

    private async Task WaitForDiceResults()
    {
        while (!dice1Result.HasValue || !dice2Result.HasValue)
        {
            await Task.Delay(100); // 每 100ms 检查一次
        }
    }
}