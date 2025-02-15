using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    /// <summary>
    /// 是否允许鼠标输入（例如移动、选格子等）
    /// </summary>
    [Tooltip("当前是否允许鼠标点击")]
    public bool mouseInputEnabled = true;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
}