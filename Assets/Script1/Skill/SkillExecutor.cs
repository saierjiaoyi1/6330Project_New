using UnityEngine;
using System.Collections.Generic;

public static class SkillExecutor
{
    /// <summary>
    /// 根据释放中心 releaseCell 以及技能区域配置 areaData 计算出目标信息列表  
    /// 若 direction 有值，则对区域数据进行旋转；否则不旋转
    /// </summary>
    public static List<SkillTargetInfo> GetAffectedTargets(GridCell releaseCell, SkillAreaData areaData, Vector2Int? direction)
    {
        List<SkillTargetInfo> result = new List<SkillTargetInfo>();
        if (releaseCell == null || areaData == null) return result;
        GridMapManager gridMap = releaseCell.parentMap;
        if (gridMap == null) return result;

        List<SkillAreaCellData> cellDataList = areaData.cellDataList;
        if (direction.HasValue)
        {
            cellDataList = RotateSkillAreaCellDataList(cellDataList, direction.Value);
        }
        foreach (SkillAreaCellData cellData in cellDataList)
        {
            int targetX = releaseCell.x + cellData.offset.x;
            int targetY = releaseCell.y + cellData.offset.y;
            GridCell cell = gridMap.GetCell(targetX, targetY);
            if (cell != null)
            {
                result.Add(new SkillTargetInfo(cell, cellData.featureCode, cellData.cellColor));
            }
        }
        return result;
    }

    /// <summary>
    /// 将 SkillAreaCellData 列表按照给定旋转方向旋转  
    /// 默认约定数据正向为 (0,1)
    /// rotationDir 为四个正方向之一
    /// </summary>
    public static List<SkillAreaCellData> RotateSkillAreaCellDataList(List<SkillAreaCellData> cellDataList, Vector2Int rotationDir)
    {
        List<SkillAreaCellData> rotated = new List<SkillAreaCellData>();
        foreach (SkillAreaCellData cellData in cellDataList)
        {
            SkillAreaCellData newCellData = new SkillAreaCellData();
            newCellData.featureCode = cellData.featureCode;
            newCellData.cellColor = cellData.cellColor;
            Vector2Int offset = cellData.offset;
            if (rotationDir == new Vector2Int(0, 1))
                newCellData.offset = offset;
            else if (rotationDir == new Vector2Int(1, 0))
                newCellData.offset = new Vector2Int(offset.y, -offset.x);
            else if (rotationDir == new Vector2Int(0, -1))
                newCellData.offset = new Vector2Int(-offset.x, -offset.y);
            else if (rotationDir == new Vector2Int(-1, 0))
                newCellData.offset = new Vector2Int(-offset.y, offset.x);
            else
                newCellData.offset = offset;
            rotated.Add(newCellData);
        }
        return rotated;
    }
}