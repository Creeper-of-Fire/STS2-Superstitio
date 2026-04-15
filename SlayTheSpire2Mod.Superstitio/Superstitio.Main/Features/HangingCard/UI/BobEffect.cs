using Godot;

namespace Superstitio.Main.Features.HangingCard.UI;

/// <summary>
/// 负责处理简谐运动（上下漂浮）的逻辑类
/// </summary>
public class BobEffect
{
    private float Amplitude { get; init; } = 8f;
    private float Speed { get; init; } = 2.0f;
    
    // 内部累加器
    private double Timer { get; set; } = 0.0;

    /// <summary>
    /// 更新计时器并返回当前的偏移量
    /// </summary>
    public float Update(double delta)
    {
        this.Timer += delta * this.Speed;
        return Mathf.Sin((float)this.Timer) * this.Amplitude;
    }
    
    /// <summary>
    /// 重置计时器
    /// </summary>
    public void Reset() => this.Timer = 0.0;
}