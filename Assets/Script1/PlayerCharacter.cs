using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class PlayerCharacter : BaseCharacter
{
    [Header("显示在屏幕左下角的立绘图像")]
    public Sprite img;
    public override void OnTurnStart()
    {
        base.OnTurnStart();
        // 此处可以调用 UI 系统激活玩家输入逻辑
        GameManager.Instance.mouseInputEnabled = true;
        SkillUIManager.Instance.ShowSkillsForPlayer(this);
        UIController.Instance.SwitchCharacterImg(img);
    }

    private void Update()
    {
        // 只有在自己的回合且状态为 Waiting 时，响应鼠标点击
        if (currentState == CharacterState.Waiting && GameManager.Instance.mouseInputEnabled)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (IsBlockedByUI()) return;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit, 100f))
                {
                    // 要求格子物体上挂有 Collider（例如 BoxCollider2D 或 Collider）
                    GridCell clickedCell = hit.collider.GetComponent<GridCell>();
                    if (clickedCell != null)
                    {
                        OnGridCellClicked(clickedCell);
                    }
                }
            }
        }
        if(animator != null)
        {
            if(currentState == CharacterState.Moving) animator.SetBool("isRunning", true);
            else animator.SetBool("isRunning", false);
        }
    }

    bool IsBlockedByUI()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (RaycastResult result in results)
        {
            // 检查是否包含阻挡组件
            if (result.gameObject.GetComponent<BlockCharacterClick>() != null)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 当玩家点击某个格子时调用（例如通过 UI 或鼠标事件触发）
    /// </summary>
    public void OnGridCellClicked(GridCell cell)
    {
        if (cell.cellState == GridCellState.Movable)
        {
            // 利用 A* 算法计算路径
            List<GridCell> path = Pathfinding.FindPath(currentCell, cell);
            // 此处简单判断路径长度是否在移动范围内
            MoveAlongPath(path);
            /*
            if (path != null && path.Count - 1 <= movementRange)
            {
                MoveAlongPath(path);
            }
            else
            {
                Debug.Log("选中的格子超出移动范围或不可达！");
            }
            */
        }
    }

    /// <summary>
    /// 重写移动结束后的行为，玩家移动完后不自动结束回合
    /// </summary>
    protected override void OnMovementComplete()
    {
        // 保持 Waiting 状态，允许玩家继续操作
        currentState = CharacterState.Waiting;
        GameManager.Instance.mouseInputEnabled = true;
    }

    /// <summary>
    /// 玩家点击跳过回合按钮时调用
    /// </summary>
    public void SkipTurn()
    {
        Debug.Log("玩家选择跳过回合。");
        EndTurn();
    }
}