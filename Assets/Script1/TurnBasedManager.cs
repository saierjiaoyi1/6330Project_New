using UnityEngine;
using System.Collections.Generic;

public class TurnBasedManager : MonoBehaviour
{
    public static TurnBasedManager Instance;

    /// <summary>
    /// 记录回合顺序的角色列表
    /// </summary>
    public List<BaseCharacter> turnOrder = new List<BaseCharacter>();
    private int currentTurnIndex = 0;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        // 查找所有 BaseCharacter 子类实例（玩家和敌人）
        BaseCharacter[] characters = FindObjectsOfType<BaseCharacter>();
        turnOrder = new List<BaseCharacter>(characters);
        // 可根据需要进行排序，例如先玩家后敌人、或根据速度等属性排序
        currentTurnIndex = 0;
        //StartTurn();
    }

    /// <summary>
    /// 开始当前回合角色的回合
    /// </summary>
    public void StartTurn()
    {
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
        currentTurnIndex = (currentTurnIndex + 1) % turnOrder.Count;
        Debug.Log(turnOrder.Count + " " + currentTurnIndex);
        StartTurn();
    }
}