using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class TurnBasedManager : MonoBehaviour
{
    public static TurnBasedManager Instance;

    /// <summary>
    /// 记录回合顺序的角色列表
    /// </summary>
    public List<BaseCharacter> turnOrder = new List<BaseCharacter>();
    private int currentTurnIndex = 0;

    // 回合计数器：每当所有角色都行动一次后加 1
    public int roundCounter = 1;

    // 绑定胜利和失败界面的UI GameObject（在 Inspector 中赋值）
    public GameObject victoryUI;
    public GameObject defeatUI;

    // 新增：待移除的角色列表
    private List<BaseCharacter> pendingRemovals = new List<BaseCharacter>();

    // 游戏结束标志
    private bool gameOver = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private IEnumerator Start()
    {
        // 查找所有 BaseCharacter 子类实例（玩家和敌人）
        //BaseCharacter[] characters = FindObjectsOfType<BaseCharacter>();
        //turnOrder = new List<BaseCharacter>(characters);
        // 可根据需要进行排序，例如先玩家后敌人、或根据速度等属性排序
        yield return new WaitForEndOfFrame();
        currentTurnIndex = 0;
        StartTurn();
    }

    /// <summary>
    /// 开始当前回合角色的回合
    /// </summary>
    public void StartTurn()
    {
        if (gameOver) return;
        if (turnOrder.Count == 0) return;
        BaseCharacter currentCharacter = turnOrder[currentTurnIndex];
        Debug.Log("当前回合角色：" + currentCharacter.name);
        currentCharacter.OnTurnStart();
    }

    /// <summary>
    /// 当前角色结束回合后调用，自动切换到下一角色
    /// </summary>
    public void NextTurn()
    {
        if (gameOver) return;
        currentTurnIndex = (currentTurnIndex + 1) % turnOrder.Count;

        // 如果currentTurnIndex回到0，说明一整轮已结束，回合计数器+1
        if (currentTurnIndex == 0)
        {
            roundCounter++;
            Debug.Log("当前回合数：" + roundCounter);
        }

        Debug.Log(turnOrder.Count + " " + currentTurnIndex);
        StartTurn();
    }

    /// <summary>
    /// 角色死亡时调用，将该角色从回合列表中移除，并检查胜负条件
    /// </summary>
    /// <param name="character">死亡的角色</param>
    public void RemoveCharacter(BaseCharacter character)
    {
        Debug.Log(turnOrder.Contains(character));
        if (turnOrder.Contains(character))
        {
            int removedIndex = turnOrder.IndexOf(character);
            turnOrder.Remove(character);
            Debug.Log($"{name}死掉了，移除了" + character);

            // 如果被移除的角色在当前回合之前，需要调整currentTurnIndex
            if (removedIndex < currentTurnIndex)
            {
                currentTurnIndex--;
            }
            // 如果当前索引超出范围，则重置为0
            if (currentTurnIndex >= turnOrder.Count)
            {
                currentTurnIndex = 0;
            }
        }

        CheckWinLoseConditions();
    }



    /// <summary>
    /// 检查所有敌人或玩家是否全部死亡
    /// </summary>
    private void CheckWinLoseConditions()
    {
        bool anyPlayerAlive = false;
        bool anyEnemyAlive = false;

        // 遍历当前存活的角色列表进行判断
        foreach (BaseCharacter character in turnOrder)
        {
            if (character is PlayerCharacter)
            {
                anyPlayerAlive = true;
            }
            else if (character is EnemyCharacter)
            {
                anyEnemyAlive = true;
            }
        }

        // 如果没有敌人存活，则胜利
        if (!anyEnemyAlive)
        {
            gameOver = true;
            Debug.Log("所有敌人已死亡，胜利！");
            if (victoryUI != null)
                victoryUI.SetActive(true);
        }
        // 如果没有玩家存活，则失败
        else if (!anyPlayerAlive)
        {
            gameOver = true;
            Debug.Log("所有玩家已死亡，失败！");
            if (defeatUI != null)
                defeatUI.SetActive(true);
        }
    }
}