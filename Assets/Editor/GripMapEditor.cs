using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GridMapManager))]
public class GridMapEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 绘制默认的 Inspector 内容
        DrawDefaultInspector();

        GridMapManager gridMap = (GridMapManager)target;
        if (GUILayout.Button("生成格子地图"))
        {
            gridMap.GenerateGrid();
        }
    }
}