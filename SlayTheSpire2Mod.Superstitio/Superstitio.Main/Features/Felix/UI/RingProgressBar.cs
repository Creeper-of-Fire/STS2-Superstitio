using Godot;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.addons.mega_text;

// ReSharper disable NullableWarningSuppressionIsUsed

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
namespace Superstitio.Main.Features.Felix.UI;

public partial class RingProgressBar : Control
{
    // ========== 核心组件 ==========
    // 两个层级：底色层（全满）和活动层（进度）
    public RingRenderer BaseRenderer { get; private set; } = null!;
    public RingRenderer ActiveRenderer { get; private set; } = null!;

    // 颜色定义 (快感常用的：浅粉 -> 深粉 -> 紫色 -> 金色 ...)
    private static readonly Color[] LayerColors =
    {
        new Color(1f, 1f, 1f, 0.0f), // Index 0: 完全透明 (初始化或0层以下)
        new Color(1.0f, 0.6f, 0.8f), // Index 1: 浅粉 (0-10层时的进度颜色)
        new Color(0.9f, 0.3f, 0.6f), // Index 2: 深粉 (10-20层时的进度颜色)
        new Color(0.6f, 0.2f, 0.9f), // Index 3: 紫色
        new Color(1.0f, 0.8f, 0.2f), // Index 4: 金色
        new Color(0.9f, 0.2f, 0.2f), // Index 5: 红色
    };

    /// <summary>
    /// 获取指定层级的颜色（支持循环或封顶）
    /// </summary>
    private Color GetLayerColor(int index)
    {
        if (index <= 0) return LayerColors[0];
        // 如果超过了定义的颜色范围，可以循环使用后面的颜色（跳过透明色）
        if (index >= LayerColors.Length)
        {
            int loopIndex = 1 + (index - 1) % (LayerColors.Length - 1);
            return LayerColors[loopIndex];
        }

        return LayerColors[index];
    }

    public MegaLabel? Label { get; private set; }
    public Control? Tooltip { get; private set; }

    // ========== 业务数值 ==========
    /// <summary>
    /// 最大值（业务层使用）
    /// </summary>
    public decimal MaxValue { get; private set; }

    /// <summary>
    /// 目标值（业务层使用）
    /// </summary>
    public decimal TargetValue { get; private set; }

    /// <summary>
    /// 当前显示的值（用于文本显示）
    /// </summary>
    public float VisualAmount { get; private set; }

    // ========== 事件转发 ==========
    public event Action? OnHoverEnter;
    public event Action? OnHoverExit;
    public event Action? OnClicked;


    // ========== 功能开关 ==========
    public bool ShowText { get; set; } = true;
    public string MyTooltipText { get; set; } = "{0}/{1}\n{2:F0}%";
    public bool ShowTooltipOnHover { get; set; } = true;

    // ========== 配置 ==========
    public RingGeometry RingGeometry { get; set; }
    public float LerpSpeed { get; set; }

    // ========== 生命周期 ==========

    public override void _Ready()
    {
        // 初始化底色渲染器 (永远 100%)
        this.BaseRenderer = new RingRenderer
        {
            Name = nameof(this.BaseRenderer),
            Size = this.Size,
            RingGeometry = this.RingGeometry,
            RenderTextureUnder = true,
            MouseFilter = MouseFilterEnum.Ignore,
        };
        this.AddChildSafely(this.BaseRenderer);
        this.BaseRenderer.SetDisplay(1.0f);

        // 初始化活动渲染器 (显示当前进度)
        this.ActiveRenderer = new RingRenderer
        {
            Name = nameof(this.ActiveRenderer),
            Size = this.Size,
            RingGeometry = this.RingGeometry,
            RenderTextureUnder = false,
            MouseFilter = MouseFilterEnum.Pass,
        };
        // 转发事件
        this.ActiveRenderer.OnHoverEnter += () => this.OnHoverEnter?.Invoke();
        this.ActiveRenderer.OnHoverExit += () => this.OnHoverExit?.Invoke();
        this.ActiveRenderer.OnClicked += () => this.OnClicked?.Invoke();
        this.AddChildSafely(this.ActiveRenderer);

        // 设置文本
        if (this.ShowText)
        {
            this.InitializeMegaLabel();
            this.SetupHoverEffects();
        }
    }

    private void InitializeMegaLabel()
    {
        float height = this.Size.Y;
        this.Label = new MegaLabel
        {
            Name = nameof(this.Label),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            // 匹配提供的参数
            MinFontSize = Mathf.RoundToInt(height * 0.15f),
            MaxFontSize = Mathf.RoundToInt(height * 0.20f),
            AutoSizeEnabled = true,
            // 必须响应悬停
            MouseFilter = MouseFilterEnum.Pass,
            // 设置 Layout 和 Anchors (Preset 15 = Full Rect)
            LayoutMode = 1
        };

        this.AddChildSafely(this.Label);

        // 应用主题覆盖 (Sts2 艺术风格)
        // 字体颜色: Color(1, 0.964706, 0.886275, 1) - 柔和奶油色
        this.Label.AddThemeColorOverride("font_color", new Color(1, 0.964706f, 0.886275f, 1));
        // 阴影颜色: 透明黑
        this.Label.AddThemeColorOverride("font_shadow_color", new Color(0, 0, 0, 0.188f));
        // 描边颜色: 深红棕色
        this.Label.AddThemeColorOverride("font_outline_color", new Color(0.3f, 0.0759f, 0.051f, 1)); // 常量设置
        this.Label.AddThemeConstantOverride("shadow_offset_x", 3);
        this.Label.AddThemeConstantOverride("shadow_offset_y", 2);
        int outlineSize = Mathf.RoundToInt(height * 0.12f);
        this.Label.AddThemeConstantOverride("outline_size", outlineSize);
        this.Label.AddThemeConstantOverride("shadow_outline_size", outlineSize);

        // 刷新字体 (MegaLabel 逻辑)
        this.Label.RefreshFont();
    }


    private bool IsActiveRingHovered { get; set; }
    private bool IsBaseRingHovered { get; set; }
    private bool IsLabelHovered { get; set; }
    private Tween? LabelFadeTween { get; set; }

    private void SetupHoverEffects()
    {
        if (this.Label == null) return;

        // 活动层（顶层进度）悬停
        this.ActiveRenderer.OnHoverEnter += () =>
        {
            this.IsActiveRingHovered = true;
            this.UpdateFadeState();
        };
        this.ActiveRenderer.OnHoverExit += () =>
        {
            this.IsActiveRingHovered = false;
            this.UpdateFadeState();
        };

        // 底色层（全满底层）悬停
        this.BaseRenderer.OnHoverEnter += () =>
        {
            this.IsBaseRingHovered = true;
            this.UpdateFadeState();
        };
        this.BaseRenderer.OnHoverExit += () =>
        {
            this.IsBaseRingHovered = false;
            this.UpdateFadeState();
        };

        // 文字区域悬停
        this.Label.MouseEntered += () =>
        {
            this.IsLabelHovered = true;
            this.UpdateFadeState();
        };
        this.Label.MouseExited += () =>
        {
            this.IsLabelHovered = false;
            this.UpdateFadeState();
        };
    }

    private void UpdateFadeState()
    {
        // 三者中任意一个被悬停，文字即变透明
        float targetAlpha = (this.IsActiveRingHovered || this.IsBaseRingHovered || this.IsLabelHovered) ? 0.2f : 1.0f;
        this.FadeLabel(targetAlpha);
    }

    /// <summary>
    /// 平滑调整 Label 的透明度
    /// </summary>
    private void FadeLabel(float targetAlpha)
    {
        if (this.Label == null || !IsInstanceValid(this.Label)) return;

        // 杀死之前的动画，防止冲突
        this.LabelFadeTween?.Kill();
        this.LabelFadeTween = this.CreateTween();

        // 使用 Cubic 曲线让渐变看起来更自然
        this.LabelFadeTween.TweenProperty(this.Label, "modulate:a", targetAlpha, 0.2f)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.Out);
    }

    public override void _Process(double delta)
    {
        // 1. 视觉值平滑追赶实际值
        this.VisualAmount = Mathf.Lerp(this.VisualAmount, (float)this.TargetValue, (float)delta * this.LerpSpeed);

        // 极小差距直接同步
        if (Mathf.Abs(this.VisualAmount - (float)this.TargetValue) < 0.01f) this.VisualAmount = (float)this.TargetValue;

        // 2. 核心数学计算
        float threshold = (float)this.MaxValue;
        if (threshold <= 0) return;

        // 当前完整填充了多少层
        int layerIndex = Mathf.FloorToInt(this.VisualAmount / threshold);
        // 当前层正在填充的进度 (0.0 ~ 1.0)
        float progress = (this.VisualAmount % threshold) / threshold;
        
        // 如果 VisualAmount 恰好是 threshold 的倍数，Mathf.Floor 会跳到下一层
        // 但此时 Progress 是 0，视觉上底部就是上一层的满颜色。
        
        // 动态开启底色层碰撞箱
        // 如果层数大于 0，说明底色层有颜色（非透明），那么整个圆环都应该有碰撞箱
        var targetMouseFilter = layerIndex > 0 ? MouseFilterEnum.Pass : MouseFilterEnum.Ignore;
        if (this.BaseRenderer.MouseFilter != targetMouseFilter)
        {
            this.BaseRenderer.MouseFilter = targetMouseFilter;
            // 如果切换到 Ignore，强制重置其悬停状态防止状态卡死
            if (targetMouseFilter == MouseFilterEnum.Ignore)
            {
                this.IsBaseRingHovered = false;
                this.UpdateFadeState();
            }
        }

        // 3. 更新颜色和显示
        this.BaseRenderer.ProgressBar!.TintProgress = this.GetLayerColor(layerIndex);
        this.ActiveRenderer.ProgressBar!.TintProgress = this.GetLayerColor(layerIndex + 1);
        this.ActiveRenderer.SetDisplay(progress);

        if (this.Label != null && this.ShowText)
        {
            this.Label.SetTextAutoSize($"{Mathf.RoundToInt(this.VisualAmount)}/{(int)this.MaxValue}");
            this.SyncLabelHitbox();
        }
    }

    /// <summary>
    /// 核心逻辑：根据文字内容动态调整 Label 节点的 Size，使其碰撞箱完美贴合文字
    /// </summary>
    private void SyncLabelHitbox()
    {
        if (this.Label == null) return;

        // 获取当前正在使用的字体和字号（MegaLabel 已经根据其 AutoSize 逻辑算好了）
        Font font = this.Label.GetThemeFont("font");
        int fontSize = this.Label.GetThemeFontSize("font_size");
        int outline = this.Label.GetThemeConstant("outline_size");

        // 计算当前字符串在屏幕上的实际像素尺寸
        Vector2 stringSize = font.GetMultilineStringSize(
            text: this.Label.Text,
            alignment: this.Label.HorizontalAlignment,
            width: -1f, // 计数器不需要自动换行
            fontSize: fontSize
        );

        // 加上描边的厚度（左右、上下各一份）
        Vector2 realSize = stringSize + Vector2.One * (outline * 2);

        // 如果尺寸有变化，则更新
        if (!this.Label.Size.IsEqualApprox(realSize))
        {
            this.Label.Size = realSize;

            // 重新计算 Position 以保持居中和偏移
            float h = this.Size.Y;
            float vOffset = -h * 0.42f;
            this.Label.Position = (this.Size - realSize) / 2f + new Vector2(0, vOffset);

            // 刷新 PivotOffset 保证缩放动画（如果有的话）依然从中心开始
            this.Label.PivotOffset = realSize / 2f;
        }
    }

    // ========== 公共 API ==========

    /// <summary>
    /// 设置最大值
    /// </summary>
    public void SetMaxValue(decimal value, ProgressAnimator.AnimationStyle? style = null)
    {
        this.MaxValue = value;
    }

    /// <summary>
    /// 设置目标值
    /// </summary>
    public void SetTargetValue(decimal value, ProgressAnimator.AnimationStyle? style = null)
    {
        this.TargetValue = value;
    }

    public void Flash()
    {
        var tween = this.CreateTween();
        tween.TweenProperty(this.ActiveRenderer, "modulate", new Color(2, 2, 1, 1), 0.1f);
        tween.TweenProperty(this.ActiveRenderer, "modulate", Colors.White, 0.2f);
    }

    public void Pulse()
    {
        var tween = this.CreateTween().SetLoops();
        tween.TweenProperty(this, "scale", Vector2.One * 1.05f, 0.3f);
        tween.TweenProperty(this, "scale", Vector2.One, 0.3f);
    }
}