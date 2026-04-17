using Godot;

namespace Superstitio.Main.Features.Felix.UI;

/// <summary>
/// 圆环几何参数配置
/// </summary>
public readonly record struct RingGeometry()
{
    /// <summary>
    /// 圆环的外半径（从圆心到外边缘的距离）
    /// </summary>
    public float OuterRadius { get; init; } = 56;

    /// <summary>
    /// 圆环的厚度（外半径与内半径的差值）
    /// </summary>
    public float Thickness { get; init; } = 8;

    /// <summary>
    /// 圆环填充的起始角度（以度为单位，0°为顶部，顺时针增加）
    /// </summary>
    public float StartAngle { get; init; } = -135;

    /// <summary>
    /// 圆环填充的角度范围（以度为单位，360°表示完整圆环）
    /// </summary>
    public float FillDegrees { get; init; } = 270;

    /// <summary>
    /// 获取结束角度
    /// </summary>
    public float EndAngle => this.StartAngle + this.FillDegrees;

    /// <summary>
    /// 获取内半径
    /// </summary>
    public float InnerRadius => this.OuterRadius - this.Thickness;

    /// <summary>
    /// 获取缺口角度
    /// </summary>
    public float GapDegrees => 360f - this.FillDegrees;

    /// <summary>
    /// 填充模式
    /// </summary>
    public TextureProgressBar.FillModeEnum FillMode { get; init; } = TextureProgressBar.FillModeEnum.Clockwise;
}