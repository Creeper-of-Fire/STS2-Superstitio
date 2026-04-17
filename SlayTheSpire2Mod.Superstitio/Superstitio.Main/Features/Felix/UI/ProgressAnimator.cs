using Godot;

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释

namespace Superstitio.Main.Features.Felix.UI;

/// <summary>
/// 进度动画控制器 - 输入目标 Progress (0-1)，输出动画后的 Progress
/// </summary>
public partial class ProgressAnimator : Node
{
    public enum AnimationStyle
    {
        None,
        Smooth,
        Elastic,
        Overshoot,
        WrapAround
    }

    /// <summary>
    /// 目标进度值 (0-1)
    /// </summary>
    public decimal TargetProgress { get; private set; }

    /// <summary>
    /// 当前动画进度值 (可以超过 0-1 范围用于过充效果)
    /// </summary>
    public float AnimatedProgress { get; private set; }

    public event Action<float>? ProgressChanged;

    private Tween? ActiveTween { get; set; }
    public AnimationStyle CurrentStyle { get; set; } = AnimationStyle.Smooth;
    public float AnimationSpeed { get; set; } = 1.0f;

    /// <summary>
    /// 绕圈完成事件（用于触发闪烁等效果）
    /// </summary>
    public event Action? Wrapped;

    public void SetTarget(decimal progress, AnimationStyle? style = null)
    {
        if (style.HasValue)
        {
            this.CurrentStyle = style.Value;
        }

        this.TargetProgress = progress;
        this.AnimateTo(progress);
    }

    private void SetAnimatedProgress(float value)
    {
        this.AnimatedProgress = value;
        this.ProgressChanged?.Invoke(value);
    }

    private void SetAnimatedProgress(decimal value) => this.SetAnimatedProgress((float)value);


    private void AnimateTo(decimal target)
    {
        this.ActiveTween?.Kill();
        this.ActiveTween = this.CreateTween();

        float duration = 0.3f / this.AnimationSpeed;

        switch (this.CurrentStyle)
        {
            case AnimationStyle.None:
                this.SetAnimatedProgress(target);
                break;

            case AnimationStyle.Smooth:
                this.ActiveTween.TweenMethod(
                    Callable.From<float>(this.SetAnimatedProgress),
                    this.AnimatedProgress, (float)target, duration
                ).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
                break;

            case AnimationStyle.Elastic:
                this.ActiveTween.TweenMethod(
                    Callable.From<float>(this.SetAnimatedProgress),
                    this.AnimatedProgress, (float)target, duration * 1.5f
                ).SetTrans(Tween.TransitionType.Elastic).SetEase(Tween.EaseType.Out);
                break;

            case AnimationStyle.Overshoot:
                this.ActiveTween.TweenMethod(
                    Callable.From<float>(this.SetAnimatedProgress),
                    this.AnimatedProgress, (float)target * 1.1f, duration * 0.5f
                ).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
                this.ActiveTween.TweenMethod(
                    Callable.From<float>(this.SetAnimatedProgress),
                    (float)target * 1.1f, (float)target, duration * 0.7f
                ).SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
                break;

            case AnimationStyle.WrapAround:

                bool isExactlyInteger = target % 1m == 0m;
                float fractionalTarget = (float)(target % 1m);

                // 先冲到满（允许超过 1.0 显示完整一圈）
                this.ActiveTween.TweenMethod(
                    Callable.From<float>(this.SetAnimatedProgress),
                    this.AnimatedProgress, 1.2f, duration * 0.5f
                ).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.In);

                // 瞬间归零
                this.ActiveTween.TweenCallback(Callable.From(() =>
                {
                    this.SetAnimatedProgress(0f);
                    this.Wrapped?.Invoke();
                }));

                // 增长到小数部分
                if (!isExactlyInteger && fractionalTarget > 0f)
                {
                    this.ActiveTween.TweenMethod(
                        Callable.From<float>(this.SetAnimatedProgress),
                        0f, fractionalTarget, duration
                    ).SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
                }
                break;
        }
    }
}