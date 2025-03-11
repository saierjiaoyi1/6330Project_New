using UnityEngine;
using System;

[Serializable]
public class BuffInstance
{
    public BuffSO buffData;  // 对应的 buff 配置
    public int remainingRounds; // 当前剩余回合数

    public BuffInstance(BuffSO buffData)
    {
        this.buffData = buffData;
        this.remainingRounds = buffData.duration;
    }
}
