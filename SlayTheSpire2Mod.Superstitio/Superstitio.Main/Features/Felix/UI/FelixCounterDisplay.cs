using Godot;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Combat;

// ReSharper disable NullableWarningSuppressionIsUsed

namespace Superstitio.Main.Features.Felix.UI;

/// <summary>
/// 快感计数器显示器 - 显示在能量球周围的进度
/// </summary>
public partial class FelixCounterDisplay : Control
{
    // ========== 静态实例管理 ==========
    private static FelixCounterDisplay? Instance { get; set; }

    // ========== 对外部的引用 ==========
    private FelixPower Power { get; set; } = null!;

    // ========== 使用的控件 ==========
    private RingProgressBar RingBar { get; set; } = null!;

    // ========== 属性 ==========

    /// <summary>
    /// 当前的层数
    /// </summary>
    private int CurrentAmount => this.Power.Amount;

    /// <summary>
    /// 高潮阈值
    /// </summary>
    private int ClimaxThreshold => this.Power.ClimaxThreshold;

    // ========== 常量配置 ==========
    private const float ProgressLerpSpeed = 6f;

    private const float SizeRate = 1.2f;
    private const float OffsetRatio = -0.263f;
    private const float ThicknessRate = 0.18f; // 环厚度占半径的比例

    private const float TargetStartAngle = -100f;
    private const float TargetFillDegrees = 200f;

    // /// <summary>
    // /// 计算圆环端点刚好触碰球体时所需的垂直偏移比例
    // /// </summary>
    // /// <param name="sizeRate">圆环相对于球的缩放比 (k)</param>
    // /// <param name="gapDegrees">缺口的总角度 (theta)</param>
    // /// <returns>OffsetRatio</returns>
    // public static float CalculateOffsetRatio(float sizeRate, float gapDegrees)
    // {
    //     float alpha = Mathf.DegToRad(gapDegrees / 2f);
    //     float k = sizeRate;
    //
    //     // 检查端点是否过宽
    //     float horizontalDistance = k * Mathf.Sin(alpha);
    //     if (horizontalDistance > 1.0f)
    //     {
    //         GD.PrintErr($"[FelixUI] 无法对齐：SizeRate {k} 配合缺口 {gapDegrees} 导致端点超出了球体边缘！");
    //         return -0.2f; // 返回一个默认容错值
    //     }
    //
    //     // 公式计算 h/r
    //     float h_over_r = k * Mathf.Cos(alpha) - Mathf.Sqrt(1f - horizontalDistance * horizontalDistance);
    //
    //     // 转为相对于直径的比例 (h / 2r)，向上为负
    //     return -h_over_r / 2f;
    // }

    /// <summary>
    /// 确保实例存在
    /// </summary>
    public static FelixCounterDisplay Ensure(NEnergyCounter energyCounter, FelixPower felixPower)
    {
        if (IsInstanceValid(Instance))
        {
            return Instance;
        }

        var parent = energyCounter.GetParent<Control>();

        float offsetRatio = -0.25f;
        var offset = new Vector2(0, energyCounter.Size.Y * offsetRatio);

        Instance = new FelixCounterDisplay
        {
            Name = nameof(FelixCounterDisplay),
            MouseFilter = MouseFilterEnum.Ignore,
            Size = energyCounter.Size,
            Position = energyCounter.Position + offset
        };

        Instance.Power = felixPower;

        Instance.InitializeRingProgressBar();

        // 添加为同级节点
        parent.AddChildSafely(Instance);
        // 确保显示在能量球的前面
        parent.MoveChild(Instance, energyCounter.GetIndex() + 1);

        return Instance;
    }

    /// <summary>
    /// 初始化环形进度条
    /// </summary>
    private void InitializeRingProgressBar()
    {
        // RingBar 比父容器大一点
        var ringSize = this.Size * SizeRate;
        var offset = (this.Size - ringSize) / 2;
        float ringRadius = Mathf.Max(ringSize.X, ringSize.Y) / 2f;
        float ringThickness = ringRadius * ThicknessRate;

        this.RingBar = new RingProgressBar
        {
            Name = nameof(RingProgressBar),
            Size = ringSize,
            Position = offset,
            RingGeometry = new RingGeometry
            {
                OuterRadius = ringRadius,
                Thickness = ringThickness,
                StartAngle = TargetStartAngle,
                FillDegrees = TargetFillDegrees,
                FillMode = TextureProgressBar.FillModeEnum.ClockwiseAndCounterClockwise,
            },
            LerpSpeed = ProgressLerpSpeed,
            MouseFilter = MouseFilterEnum.Ignore,
            ShowText = true,
            ShowTooltipOnHover = true // 可以后续根据需要开启
        };

        this.AddChildSafely(this.RingBar);
    }

    /// <inheritdoc />
    public override void _Ready()
    {
        this.RingBar.SetMaxValue(this.ClimaxThreshold);
        this.RingBar.SetTargetValue(this.CurrentAmount);
        // 延迟一帧打印，确保布局完成
        this.CallDeferred(nameof(this.PrintDeepLayoutInfo));
    }

    private void PrintDeepLayoutInfo()
    {
        GD.Print("[FelixLog] === Deep Layout Debug Start ===");

        // 1. 检查父级
        var parent = this.GetParent<Control>();
        if (parent != null)
        {
            GD.Print($"[FelixLog] Parent: Size={parent.Size}, GlobalPos={parent.GlobalPosition}, Pivot={parent.PivotOffset}");
        }

        // 2. 检查自身 (FelixCounterDisplay)
        GD.Print(
            $"[FelixLog] Self (Display): Size={this.Size}, Pos={this.Position}, GlobalPos={this.GlobalPosition}, Pivot={this.PivotOffset}");

        // 3. 检查中层 (RingProgressBar)
        GD.Print(
            $"[FelixLog] Child (RingBar): Size={this.RingBar.Size}, Pos={this.RingBar.Position}, GlobalPos={this.RingBar.GlobalPosition}");

        // 4. 检查底层 (RingRenderer)
        var core = this.RingBar.ActiveRenderer;
        GD.Print($"[FelixLog] Renderer (Pure): Size={core.Size}, Pos={core.Position}, GlobalPos={core.GlobalPosition}");
        GD.Print($"[FelixLog] Renderer Radius Prop: {core.RingGeometry.OuterRadius}, Thickness Prop: {core.RingGeometry.Thickness}");

        // 5. 检查核心 TextureProgressBar
        if (core.ProgressBar != null)
        {
            var pb = core.ProgressBar;
            GD.Print($"[FelixLog] TextureProgressBar: Size={pb.Size}, Pos={pb.Position}, GlobalPos={pb.GlobalPosition}");
            GD.Print($"[FelixLog] TextureUnder Size: {pb.TextureUnder?.GetSize() ?? Vector2.Zero}");
            GD.Print($"[FelixLog] NinePatchStretch: {pb.NinePatchStretch}");
        }

        GD.Print("[FelixLog] === Deep Layout Debug End ===");
    }

    // ========== 运行目标 ==========

    private int LastRawAmount { get; set; } = 0;

    /// <summary>
    /// 刷新目标值
    /// </summary>
    private void RefreshTarget()
    {
        this.RingBar.SetTargetValue(this.Power.Amount);
        this.RingBar.SetMaxValue(this.Power.ClimaxThreshold);
    }

    // ========== 外部可进行的指令 ==========

    /// <summary>
    /// 刷新显示
    /// </summary>
    public void RefreshAmount()
    {
        this.RefreshTarget();
    }

    /// <summary>
    /// 触发达到里程碑的闪烁效果
    /// </summary>
    public void FlashMilestone()
    {
        this.RingBar.Flash();
    }
}