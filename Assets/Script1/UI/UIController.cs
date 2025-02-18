using UnityEngine;
using UnityEngine.UI;

//这个脚本是用来帮助button触发GameManager里的点击事件的
//还有一些其它的控制UI显示的功能
public class UIController : MonoBehaviour
{
    public static UIController Instance;

    [Header("Canvas UI里面的暂停界面和左下角的characterImg拖进来")]
    public GameObject pauseUI;
    public Image characterImg;

    [Header("这个场景的两个骰子拖进来")]
    public DiceRoll dice1;
    public DiceRoll dice2;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }

    public void Restart()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.Restart();
        }
    }
    public void NextLevel()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadNextLevel();
        }
    }
    public void ComeBack()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ComeBack();
        }
    }

    public void Pause()
    {
        pauseUI.SetActive(true);
    }
    public void Continue()
    {
        pauseUI.SetActive(false);
    }

    public void SwitchCharacterImg(Sprite img)
    {
        characterImg.sprite = img;
    }

    public void CharacterEndTurn()
    {
        TurnBasedManager.Instance.turnOrder[TurnBasedManager.Instance.currentTurnIndex].EndTurn();
    }
}
