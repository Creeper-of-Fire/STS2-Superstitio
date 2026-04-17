using Godot;

// ReSharper disable NullableWarningSuppressionIsUsed

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
namespace Superstitio.Main.Features.Felix.UI;

public partial class RingProgressBar : Control
{
    // ========== 核心组件 ==========
    public PureRingProgressBar Core { get; private set; } = null!;
    public Label? Label { get; private set; }
    public Control? Tooltip { get; private set; }

    // ========== 业务属性 ==========
    public float MaxValue
    {
        get;
        set
        {
            field = value;
            this.UpdateCoreProgress();
        }
    } = 100f;

    public float TargetValue
    {
        get;
        set
        {
            field = Mathf.Clamp(value, 0, this.MaxValue);
            this.UpdateCoreProgress();
        }
    }

    // ========== 功能开关 ==========
    public bool ShowText { get; set; } = true;
    public string MyTooltipText { get; set; } = "{0}/{1}\n{2:F0}%";
    public bool ShowTooltipOnHover { get; set; } = true;

    // ========== 事件转发 ==========
    public event Action? OnHoverEnter;
    public event Action? OnHoverExit;
    public event Action? OnClicked;

    // ========== 属性 ==========
    public RingGeometry RingGeometry { get; set; }
    public float LerpSpeed { get; set; }

    // ========== 生命周期 ==========

    public override void _Ready()
    {
        this.Core = new PureRingProgressBar
        {
            Name = nameof(this.Core),
            Size = this.Size,
            Position = Vector2.Zero,
            RingGeometry = this.RingGeometry,
            ProgressColor = new Color(1f, 0.5f, 0.1f),
            LerpSpeed = this.LerpSpeed,
        };
        // 转发事件
        this.Core.OnHoverEnter += () => this.OnHoverEnter?.Invoke();
        this.Core.OnHoverExit += () => this.OnHoverExit?.Invoke();
        this.Core.OnClicked += () => this.OnClicked?.Invoke();
        this.AddChild(this.Core);

        // 设置文本
        if (this.ShowText)
        {
            this.Label = new Label
            {
                Name = nameof(this.Label),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Size = this.Size,
                Position = Vector2.Zero
            };
            this.Label.AddThemeFontSizeOverride("font_size", 14);
            this.AddChild(this.Label);
        }

        this.UpdateCoreProgress();
    }

    private void UpdateCoreProgress()
    {
        if (!this.IsNodeReady()) return;
        if (!this.IsNodeReady()) return;

        float newTarget = this.MaxValue > 0 ? this.TargetValue / this.MaxValue : 0f;
        float currentDisplay = this.Core.DisplayProgress;

        // 如果新的目标值小于当前显示值，说明发生了“大招释放”或“阈值重置”
        // TODO 放到 FelixCounterDisplay 里面去做好吗？现在这样太不合理了。
        // if (newTarget < currentDisplay && currentDisplay > 0.1f) 
        // {
        //     this.PlayWrapAroundAnimation(newTarget);
        // }
        // else
        // {
        // 普通增长逻辑，依然使用 Core 的 Lerp 或简单 Tween
        this.Core.SetTarget(newTarget);
        // }
    }

    private Tween? ActiveTween { get; set; }

    private void PlayWrapAroundAnimation(float finalTarget)
    {
        this.ActiveTween?.Kill();
        this.ActiveTween = this.CreateTween();

        // 1. 先填满：从当前位置快速冲到 1.0
        this.ActiveTween.TweenProperty(this.Core, "DisplayProgress", 1.0f, 0.2f)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.Out);

        // 2. 瞬间重置：在 1.0 满格后，瞬间切回 0
        this.ActiveTween.TweenCallback(Callable.From(() =>
        {
            this.Core.SetDisplayProgress(0f);
            this.Flash(); // 填满时顺便闪烁一下，效果更帅
        }));

        // 3. 再回缩/增长：从 0 增长到新的剩余进度
        this.ActiveTween.TweenProperty(this.Core, "DisplayProgress", finalTarget, 0.4f)
            .SetTrans(Tween.TransitionType.Elastic) // 弹性效果，看起来更有动感
            .SetEase(Tween.EaseType.Out);

        // 同步 TargetProgress 防止 _Process 干扰
        this.ActiveTween.Finished += () => this.Core.TargetProgress = finalTarget;
    }

    public override void _Process(double delta)
    {
        // 确保每帧重绘
        this.QueueRedraw();
        if (this.Label != null && this.ShowText)
        {
            this.Label.Text = $"{Mathf.RoundToInt(this.TargetValue)}/{Mathf.RoundToInt(this.MaxValue)}";
        }
    }

    // ========== 公共 API ==========
    public void SetValue(float value)
    {
        this.TargetValue = value;
    }

    public void SetValue(int value) => this.SetValue((float)value);

    public void Increment(float delta)
    {
        this.SetValue(this.TargetValue + delta);
    }

    public void Flash()
    {
        var tween = this.CreateTween();
        tween.TweenProperty(this.Core, "modulate", new Color(2, 2, 1, 1), 0.1f);
        tween.TweenProperty(this.Core, "modulate", Colors.White, 0.2f);
    }

    public void Pulse()
    {
        var tween = this.CreateTween().SetLoops();
        tween.TweenProperty(this, "scale", Vector2.One * 1.05f, 0.3f);
        tween.TweenProperty(this, "scale", Vector2.One, 0.3f);
    }
}