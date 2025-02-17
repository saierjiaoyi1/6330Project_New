using UnityEngine;
using UnityEngine.UI;

//这个脚本是用来帮助button触发GameManager里的点击事件的
public class UIController : MonoBehaviour
{
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
}
