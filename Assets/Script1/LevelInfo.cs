using System;
using UnityEngine;

[Serializable]
public class LevelInfo
{
    [Tooltip("关卡名称")]
    public string levelName;
    [Tooltip("关卡详情说明")]
    public string description;
    [Tooltip("对应的 Scene 名称")]
    public string sceneName;
}
