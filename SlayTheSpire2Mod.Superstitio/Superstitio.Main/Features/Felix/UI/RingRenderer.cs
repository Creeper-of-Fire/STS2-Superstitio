using Godot;
using MegaCrit.Sts2.Core.Helpers;

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
namespace Superstitio.Main.Features.Felix.UI;

public partial class RingRenderer : Control
{
    // ========== 导出属性 ==========
    public float InitProgress { get; set; } = 0;

    /// <summary>
    /// 显示进度 (自动 Clamp 到 0-1)
    /// </summary>
    public float DisplayProgress
    {
        get;
        private set => field = Mathf.Clamp(value, 0f, 1f);
    }

    public Color ProgressColor { get; set; } = new(1f, 0.5f, 0.1f);

    // ========== 私有属性 ==========
    public TextureProgressBar? ProgressBar { get; private set; }
    private Texture2D? GeneratedUnderTexture { get; set; }
    private Texture2D? GeneratedProgressTexture { get; set; }
    private bool IsHovered { get; set; }

    public required bool RenderTextureUnder { get; init; }

    // ========== 生命周期 ==========
    public override void _Ready()
    {
        // 生成纹理
        this.GenerateTextures();

        float actualStartAngle = this.RingGeometry.FillMode == TextureProgressBar.FillModeEnum.ClockwiseAndCounterClockwise
            ? this.RingGeometry.StartAngle + this.RingGeometry.FillDegrees / 2f
            : this.RingGeometry.StartAngle;

        // 设置 TextureProgressBar
        this.ProgressBar = new TextureProgressBar
        {
            Name = nameof(TextureProgressBar),
            MinValue = 0.0,
            MaxValue = 1.0,
            Step = 0, // 无步长限制，完全平滑
            Value = 0, // 由于只负责显示，所以初始显示为0完全合理。
            FillMode = (int)this.RingGeometry.FillMode,
            MouseFilter = MouseFilterEnum.Ignore,
            RadialInitialAngle = actualStartAngle,
            RadialFillDegrees = this.RingGeometry.FillDegrees,
            TextureProgress = this.GeneratedProgressTexture,
            TintProgress = this.ProgressColor,
            Size = this.Size,
            Position = Vector2.Zero,
            NinePatchStretch = false, // 明确不拉伸
        };
        if (this.RenderTextureUnder)
            this.ProgressBar.TextureUnder = this.GeneratedUnderTexture;
        this.AddChildSafely(this.ProgressBar);
        // 调用布局更新
        this.UpdateProgressBarLayout();
    }

    private void UpdateProgressBarLayout()
    {
        if (this.ProgressBar == null || this.GeneratedProgressTexture == null) return;

        Vector2 texSize = this.GeneratedProgressTexture.GetSize();

        // 1. 让 ProgressBar 的大小等于纹理的实际像素大小
        this.ProgressBar.Size = texSize;

        // 2. 计算偏移量：(控件大小 - 纹理大小) / 2
        // 这样纹理的中心点就会和 Control 的中心点重合
        this.ProgressBar.Position = (this.Size - texSize) / 2f;
    }


    private void GenerateTextures()
    {
        this.GeneratedUnderTexture = this.GenerateRingTexture(Colors.White, this.RingGeometry);
        this.GeneratedProgressTexture = this.GenerateRingTexture(Colors.White, this.RingGeometry);
    }

    private Texture2D GenerateRingTexture(
        Color color,
        RingGeometry ringGeometry
    )
    {
        return RingTextureGenerator.Generate(
            innerRadius: ringGeometry.InnerRadius,
            outerRadius: ringGeometry.OuterRadius,
            color: color,
            startAngle: ringGeometry.StartAngle,
            fillDegrees: ringGeometry.FillDegrees
        );
    }

    // ========== 对外暴露方法 ==========

    /// <summary>
    /// 设置显示进度
    /// </summary>
    public void SetDisplay(float progress)
    {
        this.DisplayProgress = progress;
        this.ProgressBar?.Value = this.DisplayProgress;
    }

    // ========== 环形热区参数 ==========

    public RingGeometry RingGeometry
    {
        get;
        set
        {
            field = value;
            this.RegenerateTextures(); // 参数一变，纹理重刷
        }
    }

    private void RegenerateTextures()
    {
        if (this.ProgressBar == null)
            return;
        this.GenerateTextures();
        if (this.RenderTextureUnder)
            this.ProgressBar.TextureUnder = this.GeneratedUnderTexture;
        this.ProgressBar.TextureProgress = this.GeneratedProgressTexture;

        // 重新计算偏移
        this.UpdateProgressBarLayout();

        // 计算实际的起始角度
        float actualStartAngle = this.RingGeometry.FillMode == TextureProgressBar.FillModeEnum.ClockwiseAndCounterClockwise
            ? this.RingGeometry.StartAngle + this.RingGeometry.FillDegrees / 2f
            : this.RingGeometry.StartAngle;

        this.ProgressBar.RadialInitialAngle = actualStartAngle;
        this.ProgressBar.RadialFillDegrees = this.RingGeometry.FillDegrees;
        this.ProgressBar.FillMode = (int)this.RingGeometry.FillMode;
    }

    // 获取环形参数
    private float RingInnerRadius => this.RingGeometry.InnerRadius;
    private float RingOuterRadius => this.RingGeometry.OuterRadius;

    /// <summary>
    /// 检测点是否在环形进度区域内
    /// </summary>
    private bool IsPointInRing(Vector2 point)
    {
        // 转换为相对于控件中心的坐标
        Vector2 center = this.Size / 2;
        Vector2 localPoint = point - center;

        // 计算距离
        float distance = localPoint.Length();

        // 条件1：距离在环的范围内
        bool inRadius = distance >= this.RingInnerRadius && distance <= this.RingOuterRadius;
        if (!inRadius) return false;
        if (!inRadius) return false;

        // 条件2：进度遮罩过滤 (根据 FillMode 计算当前点是否已被“填充”)
        return this.CheckProgressMask(localPoint);
    }

    private bool CheckProgressMask(Vector2 localPoint)
    {
        if (this.ProgressBar == null) return false;

        float progress = this.DisplayProgress;
        // 纹理的实际绘制尺寸（基于生成的纹理大小）
        Vector2 texSize = this.GeneratedUnderTexture?.GetSize() ?? this.Size;
        Vector2 halfSize = texSize / 2;

        switch (this.RingGeometry.FillMode)
        {
            // --- 径向模式 ---
            case TextureProgressBar.FillModeEnum.Clockwise:
                return this.IsAngleInRange(localPoint, 0); // 正常顺时针
            case TextureProgressBar.FillModeEnum.CounterClockwise:
                return this.IsAngleInRange(localPoint, 1); // 逆时针
            case TextureProgressBar.FillModeEnum.ClockwiseAndCounterClockwise:
                return this.IsAngleInRange(localPoint, 2); // 双向

            // --- 线性模式 (即便纹理是圆的，遮罩也是线性的) ---
            case TextureProgressBar.FillModeEnum.LeftToRight:
                return (localPoint.X + halfSize.X) / texSize.X <= progress;
            case TextureProgressBar.FillModeEnum.RightToLeft:
                return 1.0f - (localPoint.X + halfSize.X) / texSize.X <= progress;
            case TextureProgressBar.FillModeEnum.TopToBottom:
                return (localPoint.Y + halfSize.Y) / texSize.Y <= progress;
            case TextureProgressBar.FillModeEnum.BottomToTop:
                return 1.0f - (localPoint.Y + halfSize.Y) / texSize.Y <= progress;

            // --- 双向线性 ---
            case TextureProgressBar.FillModeEnum.BilinearLeftAndRight:
                return Mathf.Abs(localPoint.X) / halfSize.X <= progress;
            case TextureProgressBar.FillModeEnum.BilinearTopAndBottom:
                return Mathf.Abs(localPoint.Y) / halfSize.Y <= progress;

            default:
                return true;
        }
    }

    /// <summary>
    /// 专门处理径向角度的判断
    /// </summary>
    private bool IsAngleInRange(Vector2 localPoint, int type)
    {
        float mouseAngle = Mathf.RadToDeg(localPoint.Angle()) + 90f;
        mouseAngle = Mathf.PosMod(mouseAngle, 360f);

        float startAngle = Mathf.PosMod(this.RingGeometry.StartAngle, 360f);
        float totalDegrees = this.RingGeometry.FillDegrees;
        float currentFill = totalDegrees * this.DisplayProgress;

        float min, max;
        if (type == 0) // Clockwise
        {
            min = startAngle;
            max = startAngle + currentFill;
        }
        else if (type == 1) // CounterClockwise
        {
            min = startAngle - currentFill;
            max = startAngle;
        }
        else // ClockwiseAndCounterClockwise
        {
            // 从几何中心往两边扩
            float centerAngle = startAngle + totalDegrees / 2f;
            min = centerAngle - currentFill / 2f;
            max = centerAngle + currentFill / 2f;
        }

        return this.IsAngleBetween(mouseAngle, min, max);
    }

    /// <summary>
    /// 处理循环边界的角度判断
    /// </summary>
    private bool IsAngleBetween(float target, float min, float max)
    {
        // 将所有角度映射到 [0, 360) 空间
        float normalizedTarget = Mathf.PosMod(target, 360f);
        float normalizedMin = Mathf.PosMod(min, 360f);
        float normalizedMax = Mathf.PosMod(max, 360f);

        if (normalizedMin <= normalizedMax)
        {
            // 普通情况：例如从 10° 到 100°
            return normalizedTarget >= normalizedMin && normalizedTarget <= normalizedMax;
        }
        else
        {
            // 跨越 0 点的情况：例如从 350° 到 20°
            return normalizedTarget >= normalizedMin || normalizedTarget <= normalizedMax;
        }
    }

    // ========== 事件 ==========
    public event Action? OnHoverEnter;
    public event Action? OnHoverExit;
    public event Action? OnClicked;

    /// <inheritdoc />
    public override bool _HasPoint(Vector2 point)
    {
        return this.IsPointInRing(point);
    }

    public override void _GuiInput(InputEvent @event)
    {
        switch (@event)
        {
            // case InputEventMouseMotion:
            // {
            //     bool wasHovered = this.IsHovered;
            //     this.IsHovered = this.IsPointInRing(this.GetLocalMousePosition());
            //
            //     if (this.IsHovered && !wasHovered)
            //     {
            //         this.IsHovered = true;
            //         this.OnHoverEnter?.Invoke();
            //         // GD.Print("环：进入");
            //     }
            //     else if (!this.IsHovered && wasHovered)
            //     {
            //         this.IsHovered = false;
            //         this.OnHoverExit?.Invoke();
            //         // GD.Print("环：离开");
            //     }
            //
            //     return;
            // }
            // 点击检测
            case InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left }:
            {
                bool hovering = this.IsPointInRing(this.GetLocalMousePosition());
                if (!hovering)
                    return;

                this.OnClicked?.Invoke();
                this.AcceptEvent();
                // GD.Print("环：点击");
                break;
            }
        }
    }

    /// <inheritdoc />
    public override void _Notification(int what)
    {
        switch (what)
        {
            case (int)NotificationMouseEnter:
                this.IsHovered = true;
                this.OnHoverEnter?.Invoke();
                // GD.Print("环：进入");
                break;
            case (int)NotificationMouseExit:
                this.IsHovered = false;
                this.OnHoverExit?.Invoke();
                // GD.Print("环：离开");
                break;
        }
    }
}