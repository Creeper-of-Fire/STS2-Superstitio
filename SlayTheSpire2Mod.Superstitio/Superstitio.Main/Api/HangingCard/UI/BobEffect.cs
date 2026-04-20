using Godot;

namespace Superstitio.Api.HangingCard.UI;

/// <summary>
/// 负责处理简谐运动（上下漂浮）的逻辑类
/// 只输出归一化的波形值 [-1, 1]
/// </summary>
public class BobEffect
{
    private float Speed { get; init; } = 2.0f;

    /// <summary>
    /// 内部累加器
    /// </summary>
    private double Timer { get; set; } = 0.0;

    /// <summary>
    /// 获取当前归一化的浮动值 [-1, 1]，不推进时间
    /// </summary>
    public float CurrentOffset { get; private set; } = 0.0f;

    /// <summary>
    /// 更新计时器并返回当前的偏移量
    /// </summary>
    public float Update(double delta)
    {
        this.Timer += delta * this.Speed;
        this.CurrentOffset = Mathf.Sin((float)this.Timer);
        return this.CurrentOffset;
    }

    /// <summary>
    /// 重置计时器
    /// </summary>
    public void Reset() => this.Timer = 0.0;
}