using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.HoverTips;

// ReSharper disable NullableWarningSuppressionIsUsed
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释

namespace Superstitio.Main.Features.HangingCard.UI;

/// <summary>
/// 状态机委托：执行逻辑并返回下一个状态函数
/// </summary>
public delegate HangingCardState HangingCardState(double delta);

public partial class HangingCardDisplay : Node2D
{
    // --- 属性区域 ---
    public HangingCardToken Token { get; private set; } = null!;

    /// <summary>
    /// 当前正在运行的状态执行器
    /// </summary>
    public HangingCardState CurrentState { get; private set; } = null!;

    /// <summary>
    /// 外部指挥官设定的“目标状态”。
    /// 状态函数会观察此意图，决定是否由于意图改变而进行退出清理。
    /// </summary>
    protected HangingCardState TargetStateIntent { get; set; } = null!;

    public Vector2 VisualOffset { get; set; } // 用于弹起动画的额外偏移

    // 队列中的原始位置
    public Vector2 QueuePosition { get; set; }

    // 当前追踪的场景节点（可能是怪物或玩家）
    public NCreature? FollowTarget { get; set; }

    public NCard CardNode { get; protected set; } = null!;
    public bool IsMouseOver { get; set; } = false;

    public float DisplayScale
    {
        get;
        set => field = Mathf.Max(0, value);
    } = 0.25f;

    // 插值系数：0 = 停在原地，1 = 完全飞到怪物身上
    private const float FollowLerpFactor = 0.85f; // 靠近怪物
    private const float HitLerpFactor = 1.00f; // 撞击时更近

    public BobEffect Bob { get; init; } = new();

    public static HangingCardDisplay Create(HangingCardToken token)
    {
        var display = new HangingCardDisplay();
        display.Token = token;
        return display;
    }

    public Control Hitbox { get; private set; } = null!;

    private void UpdateHitboxState(bool enabled)
    {
        this.Hitbox.MouseFilter = enabled ? Control.MouseFilterEnum.Pass : Control.MouseFilterEnum.Ignore;
    }

    public override void _Ready()
    {
        // 初始意图和状态对齐
        this.CurrentState = this.TargetStateIntent = this.State_InQueue;

        this.CardNode = NCard.Create(this.Token.HangingCard)!;
        this.AddChildSafely(this.CardNode);

        // TODO 加入一个计数器，这个不急，要做的还有很多
        // _counterLabel = new Label {
        //     Text = token.RemainCount.ToString(),
        //     Position = new Vector2(50, -10)
        // };
        // AddChildSafely(_counterLabel);

        // --- 动态计算 Hitbox ---
        // 从 CardNode 获取原始未缩放尺寸
        var originalSize = this.CardNode.GetCurrentSize();
        // Hitbox 应该恒等于 Idle 状态的大小
        // 这样放大后，鼠标只要移出“初始小框”就会触发 Unhover
        var idleSize = originalSize * IdleScale;
        this.Hitbox = new Control
        {
            Size = idleSize,
            Position = -idleSize / 2f // 中心对齐
        };
        this.Hitbox.MouseFilter = Control.MouseFilterEnum.Pass;
        this.Hitbox.Connect(Control.SignalName.MouseEntered, Callable.From(() => this.IsMouseOver = true));
        this.Hitbox.Connect(Control.SignalName.MouseExited, Callable.From(() => this.IsMouseOver = false));
        this.AddChildSafely(this.Hitbox);
    }

    public NCreature? PreviewTarget
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            this.RequestVisualUpdate();
        }
    }


    // 视觉更新请求标志，防止一帧内多次重复更新
    public void RequestVisualUpdate() => this.NeedsVisualUpdate = true;
    protected bool NeedsVisualUpdate { get; set; } = true;

    public static readonly FieldInfo UpgradePreviewTypeField = AccessTools.Field(typeof(CardModel), "_upgradePreviewType");

    public override void _Process(double delta)
    {
        // 确保节点 Ready 后才开始后续逻辑
        if (!this.CardNode.IsNodeReady())
            return;

        // 检查是否需要刷新动态变量 (描述文字、数值、颜色)
        if (this.NeedsVisualUpdate)
        {
            var model = this.CardNode.Model;
            if (model is not null)
            {
                var originalUpgradePreviewType = model.UpgradePreviewType;

                // 临时设置为 Combat，让 CombatState getter 返回正确的值
                model.UpgradePreviewType = CardUpgradePreviewType.Combat;

                // 调用源码里的核心方法
                // 第一个参数设为 None 以避免触发手牌逻辑
                // 第二个参数 Normal 会触发伤害预览计算
                this.CardNode.UpdateVisuals(PileType.None, CardPreviewMode.Normal);

                // 如果有特定的预览目标（正在追踪的怪物），则告诉 NCard
                this.CardNode.SetPreviewTarget(this.PreviewTarget?.Entity); // 传入 Entity 模型
                // 这里内部会再次调用 UpdateVisuals，这是合理的，因为我们这一次对预览目标进行了更新

                // 手动调用动态变量的 UpdateCardPreview
                model.DynamicVars.Values.ToList()
                    .ForEach(it => it.UpdateCardPreview(model, CardPreviewMode.Normal, this.PreviewTarget?.Entity, true));

                UpgradePreviewTypeField.SetValue(model, originalUpgradePreviewType);
            }

            this.NeedsVisualUpdate = false;
        }


        this.CurrentState = this.CurrentState(delta);

        this.UpdateGlowVisuals(delta);
    }

    /// <summary>
    /// 当前的目标辉光类型。改变它只是改变了 Display 的“想法”。
    /// </summary>
    public HangGlowType TargetGlow { get; set; } = HangGlowType.None;

    // --- 内部视觉状态 ---
    // 记录上一帧的辉光，用于触发 AnimShow/Hide 信号
    protected HangGlowType LastGlow { get; set; } = HangGlowType.None;

    /// <summary>
    /// 辉光表现的自发维护逻辑
    /// </summary>
    protected void UpdateGlowVisuals(double delta)
    {
        // 根据意图计算目标颜色
        var targetColor = this.TargetGlow switch
        {
            HangGlowType.Good => Colors.Green,
            HangGlowType.Bad => Colors.Red,
            HangGlowType.Special => Colors.Gold,
            HangGlowType.Preview => Colors.PowderBlue,
            HangGlowType.ProgressCount => Colors.DeepSkyBlue,
            _ => new Color(1, 1, 1, 0) // 透明
        };

        // 平滑过渡颜色（Lerp 让视觉更有质感，而不是生硬的切换）
        this.CardNode.CardHighlight.Modulate = this.CardNode.CardHighlight.Modulate.Lerp(targetColor, (float)delta * 12f);

        // 逻辑触发：仅在类型切换时调用 STS2 的原生动画信号
        if (this.TargetGlow != this.LastGlow)
        {
            if (this.TargetGlow == HangGlowType.None)
                this.CardNode.CardHighlight.AnimHide();
            else
                this.CardNode.CardHighlight.AnimShow();

            this.LastGlow = this.TargetGlow;
        }
    }

    private const float IdleScale = 0.35f;
    private const float HoverScale = 0.85f;
    private const float FollowScale = 0.25f;

    /// <summary>
    /// 获取目标位置（中心点）
    /// </summary>
    /// <param name="target">目标生物</param>
    /// <param name="lerpFactor">插值系数：0 = 队列位置，1 = 怪物边缘</param>
    private Vector2 GetTargetCenter(NCreature target, float lerpFactor)
    {
        // 获取怪物的碰撞箱尺寸
        var hitboxSize = target.Hitbox.Size;

        // 获取卡牌尺寸
        var cardSize = this.CardNode.GetCurrentSize();

        // 计算从队列位置指向怪物中心的方向
        var queuePos = this.QueuePosition;
        var monsterCenter = target.Hitbox.GlobalPosition + hitboxSize / 2f;
        var directionFromMonster = (queuePos - monsterCenter).Normalized();

        // 计算怪物碰撞箱在该方向上的投影半径
        // 对于矩形碰撞箱，根据方向计算精确的边界距离
        float monsterRadius = GetRectangleRadiusInDirection(hitboxSize, directionFromMonster);
        float cardRadius = GetRectangleRadiusInDirection(cardSize, directionFromMonster);

        // 停在碰撞箱边缘
        var edgePos = monsterCenter + directionFromMonster * (monsterRadius + cardRadius);

        var targetPos = queuePos.Lerp(edgePos, lerpFactor);

        return targetPos;
    }

    private static float GetRectangleRadiusInDirection(Vector2 size, Vector2 direction)
    {
        // 将方向转换到局部空间，计算矩形边界
        float halfWidth = size.X * 0.5f;
        float halfHeight = size.Y * 0.5f;

        // 方向向量在 X 和 Y 轴上的投影
        float dx = Mathf.Abs(direction.X);
        float dy = Mathf.Abs(direction.Y);

        // 如果方向接近水平或垂直，直接返回对应半宽/半高
        if (dx > 0.001f || dy > 0.001f)
        {
            return halfWidth * halfHeight / Mathf.Sqrt(
                halfWidth * halfWidth * dy * dy +
                halfHeight * halfHeight * dx * dx
            ) * Mathf.Sqrt(dx * dx + dy * dy);
        }

        return 0f;
    }

    // 位置移动速度
    private const float MoveSpeedInQueue = 15f;
    private const float MoveSpeedFollow = 8f;
    private const float MoveSpeedHit = 15f;
    private const float MoveSpeedReturn = 10f;

    // 缩放速度
    private const float ScaleSpeedInQueue = 15f;
    private const float ScaleSpeedFollow = 5f;
    private const float ScaleSpeedHit = 15f;
    private const float ScaleSpeedReturn = 15f;

    // --- 状态机原子动作 ---

    /// <summary>
    /// 状态：悬挂队列中。
    /// 它管理着自己的内部子状态（Idle/Hover），并负责在退出时清理视觉残留。
    /// </summary>
    public HangingCardState State_InQueue(double delta)
    {
        // 1. 检查退出意图：如果目标不再是自己，执行清理并交出控制权
        if (this.TargetStateIntent != this.State_InQueue)
        {
            this.InQueue_ExitCleanup();
            return this.TargetStateIntent;
        }

        // 2. 内部子状态机：处理 Hover 逻辑
        this.InQueue_UpdateSubStates(delta);

        return this.State_InQueue;
    }

    protected bool AreTipsShowing { get; set; } = false;

    protected NHoverTipSet? ActiveTips { get; set; }

    /// <summary>
    /// 计算当前时刻 InQueue 状态应有的位置和缩放
    /// </summary>
    private (Vector2 position, float scale) CalculateInQueueTarget(double delta)
    {
        const float offsetRatioWhenHoverRightCard = 0.3f;
        const float idleScaleWhenHoverRightCard = IdleScale * 1.25f;
        bool isHoverRightCard = this.TargetGlow != HangGlowType.None;

        var desiredOffset = isHoverRightCard
            ? new Vector2(0, this.CardNode.GetCurrentSize().Y * offsetRatioWhenHoverRightCard)
            : Vector2.Zero;

        var targetPosition = this.QueuePosition + desiredOffset;

        // 缩放表现
        float targetScale;
        if (this.IsMouseOver)
            targetScale = HoverScale;
        else if (isHoverRightCard)
            targetScale = idleScaleWhenHoverRightCard;
        else
            targetScale = IdleScale;


        return (targetPosition, targetScale);
    }

    private const float BobAmplitudeRate = 0.06f;

    /// <summary>
    /// InQueue 内部逻辑：根据鼠标位置更新子状态视觉
    /// </summary>
    private void InQueue_UpdateSubStates(double delta)
    {
        (var targetPosition, float targetScale) = this.CalculateInQueueTarget(delta);
        float bobOffset = this.Bob.Update(delta) * this.CardNode.GetCurrentSize().Y * BobAmplitudeRate;
        this.GlobalPosition = this.GlobalPosition.Lerp(targetPosition + new Vector2(0, bobOffset), (float)delta * MoveSpeedInQueue);
        this.DisplayScale = Mathf.Lerp(this.DisplayScale, targetScale, (float)delta * ScaleSpeedInQueue);
        this.CardNode.Scale = Vector2.One * this.DisplayScale;

        // Hover 子状态表现
        if (!this.IsMouseOver)
        {
            this.ZIndex = 0;
            if (!this.AreTipsShowing)
                return;
            this.AreTipsShowing = false;
            this.ClearHoverTips();
            return;
        }

        this.ZIndex = 1;
        if (!this.AreTipsShowing)
        {
            this.AreTipsShowing = true;
            if (this.CardNode.Model is not null)
            {
                this.ActiveTips = NHoverTipSet.CreateAndShow(this.Hitbox, this.CardNode.Model.HoverTips);
            }
        }

        // 由于提示框内部的 FollowOffset 是静态的，
        // 且 SetAlignment 不考虑 Node2D 引起的全局缩放变化，
        // 我们每帧手动重新校准一次对齐。
        if (this.ActiveTips is null)
            return;

        // 计算视觉边界
        var cardSize = this.CardNode.GetCurrentSize();
        float visualHalfWidth = cardSize.X / 2f;
        float visualHalfHeight = cardSize.Y / 2f;

        // 锚点设置：
        // X: 卡牌中心 + 视觉半宽 + 间隔
        // Y: 卡牌中心 - 视觉半高 (即对齐卡牌视觉顶部)
        var anchor = this.GlobalPosition + new Vector2(visualHalfWidth * 1.05f, -visualHalfHeight);

        // 将 ActiveTips 强行对齐到这个动态锚点
        this.ActiveTips.GlobalPosition = anchor;

        // 仍然调用 SetAlignment 以触发 NHoverTipSet 内部的屏幕边界溢出检查 (CorrectOverflow)
        // 即使它算的位置不对，它最后执行的溢出检查对我们很有用
        this.ActiveTips.SetAlignment(this.Hitbox, HoverTipAlignment.None);
    }

    /// <summary>
    /// 当 State_InQueue 决定结束时，自行负责清理。
    /// </summary>
    private void InQueue_ExitCleanup()
    {
        this.ZIndex = 0;
        this.IsMouseOver = false;
        if (this.AreTipsShowing)
        {
            this.AreTipsShowing = false;
            this.ClearHoverTips();
        }

        // 离开队列后，禁用 Hitbox 避免飞行中意外触发悬停逻辑
        this.Hitbox.MouseFilter = Control.MouseFilterEnum.Ignore;
    }

    // --- HoverTips辅助方法 ---

    private void CreateHoverTips()
    {
        if (this.CardNode.Model is not null)
        {
            var tips = NHoverTipSet.CreateAndShow(this.Hitbox, this.CardNode.Model.HoverTips);
            tips.SetAlignment(this.Hitbox, HoverTipAlignment.Right);
        }
    }

    private void ClearHoverTips()
    {
        NHoverTipSet.Remove(this.Hitbox);
    }

    private float ProgressTimer { get; set; } = 0;

    /// <summary>
    /// 状态：计数推进反馈（原地缩放脉冲）
    /// </summary>
    public HangingCardState State_Progressing(double delta)
    {
        // 简单的缩放脉冲逻辑
        this.ProgressTimer += (float)delta;
        float duration = 0.25f;
        float t = Mathf.Clamp(this.ProgressTimer / duration, 0, 1);

        // 缩放曲线：0 -> 1.3 -> 1
        float curve = Mathf.Sin(t * Mathf.Pi);
        float currentIdleScale = IdleScale; // 或者是当前 InQueue 的目标缩放
        this.DisplayScale = currentIdleScale + curve * 0.15f;
        this.CardNode.Scale = Vector2.One * this.DisplayScale;

        if (t >= 1.0f)
        {
            this.ProgressTimer = 0;
            this.TargetStateIntent = this.State_InQueue;
            return this.State_InQueue;
        }

        return this.State_Progressing;
    }


    /// <summary>
    /// 状态：追踪目标（怪物/玩家）。
    /// </summary>
    protected HangingCardState State_Following(double delta)
    {
        if (this.TargetStateIntent != this.State_Following)
            return this.TargetStateIntent;

        if (!IsInstanceValid(this.FollowTarget))
            return this.State_Returning;

        var targetPos = this.GetTargetCenter(this.FollowTarget, FollowLerpFactor);

        // 计算距离目标的距离
        float distanceX = Mathf.Abs(this.GlobalPosition.X - targetPos.X);
        bool hasReachedTarget = distanceX < 2f; // 阈值可调整

        // 移动到目标位置
        if (!hasReachedTarget)
        {
            // 飞行中：直线移动，无浮动
            this.GlobalPosition = this.GlobalPosition.Lerp(targetPos, (float)delta * MoveSpeedFollow);
        }
        else
        {
            // 到达目标附近：开始浮动
            float bobOffset = this.Bob.Update(delta) * this.CardNode.GetCurrentSize().Y * BobAmplitudeRate;
            var hoverPosition = targetPos + new Vector2(0, bobOffset);
            this.GlobalPosition = this.GlobalPosition.Lerp(hoverPosition, (float)delta * MoveSpeedFollow);
        }

        this.DisplayScale = Mathf.Lerp(this.DisplayScale, FollowScale, (float)delta * ScaleSpeedFollow);
        this.CardNode.Scale = Vector2.One * this.DisplayScale;

        return this.State_Following;
    }

    /// <summary>
    /// 状态：撞击动作。
    /// </summary>
    protected HangingCardState State_Hitting(double delta)
    {
        // 无论 TargetStateIntent 如何，这个动作不会被打断。
        // 快速插值到目标点
        if (!IsInstanceValid(this.FollowTarget))
        {
            this.HitCompletionSource?.TrySetResult();
            this.HitCompletionSource = null;
            this.TargetStateIntent = this.State_Returning;
            return this.State_Returning;
        }

        var targetPos = this.GetTargetCenter(this.FollowTarget, HitLerpFactor);

        this.GlobalPosition = this.GlobalPosition.Lerp(targetPos, (float)delta * MoveSpeedHit);

        // 如果足够近，产生冲击感并返回
        if (this.GlobalPosition.DistanceTo(targetPos) < 20f)
        {
            // 这里可以触发特定的卡牌闪光特效
            this.CardNode.CardHighlight.AnimShow();

            this.HitCompletionSource?.TrySetResult();
            this.HitCompletionSource = null;

            this.TargetStateIntent = this.State_Returning;
            return this.State_Returning;
        }

        return this.State_Hitting;
    }

    /// <summary>
    /// 状态：返回队列。
    /// </summary>
    protected HangingCardState State_Returning(double delta)
    {
        if (this.TargetStateIntent != this.State_Returning)
            return this.TargetStateIntent;

        (var homePosition, float homeScale) = this.CalculateInQueueTarget(delta);

        this.GlobalPosition = this.GlobalPosition.Lerp(homePosition, (float)delta * MoveSpeedReturn);
        this.DisplayScale = Mathf.Lerp(this.DisplayScale, homeScale, (float)delta * ScaleSpeedReturn);
        this.CardNode.Scale = Vector2.One * this.DisplayScale;

        // 检查是否足够接近
        bool positionReached = this.GlobalPosition.DistanceTo(homePosition) < 5f;
        bool scaleReached = Mathf.Abs(this.DisplayScale - homeScale) < 0.01f;

        if (positionReached && scaleReached)
        {
            this.Bob.Reset(); // 回到队列瞬间重置，避免位置突跳
            this.UpdateHitboxState(true); // 重新启用交互
            this.TargetStateIntent = this.State_InQueue; // 回到 InQueue 时，通过对齐意图实现“回家”

            return this.State_InQueue;
        }

        return this.State_Returning;
    }

    /// <summary>
    /// 状态：移除。执行缩小和淡出，完成后销毁节点。
    /// </summary>
    public HangingCardState State_Removing(double delta)
    {
        // 快速缩小
        this.DisplayScale = Mathf.Lerp(this.DisplayScale, 0f, (float)delta * 15f);
        this.CardNode.Scale = Vector2.One * this.DisplayScale;

        // 淡出
        this.Modulate = this.Modulate.Lerp(new Color(1, 1, 1, 0), (float)delta * 15f);

        // 阈值检查：一旦几乎看不见，就真正销毁
        if (this.DisplayScale < 0.01f)
        {
            this.QueueFree();
            // 返回自身以确保最后这一两帧不会报错
            return this.State_Removing;
        }

        return this.State_Removing;
    }

    public void StartGlow(HangGlowType type)
    {
        this.TargetGlow = type;
    }

    public void EndGlow()
    {
        this.TargetGlow = HangGlowType.None;
    }

    // --- 指挥接口 (供 Token 调用) ---

    /// <summary>
    /// 跟随指定对象
    /// </summary>
    public void Command_Follow(NCreature target)
    {
        this.FollowTarget = target;
        this.PreviewTarget = target;
        this.TargetStateIntent = this.State_Following;
    }

    /// <summary>
    /// 在出击后，返回悬挂队列
    /// </summary>
    public void Command_Return()
    {
        this.PreviewTarget = null;
        this.TargetStateIntent = this.State_Returning;
    }

    private TaskCompletionSource? HitCompletionSource { get; set; }

    /// <summary>
    /// 攻击指定对象
    /// </summary>
    public async Task Command_HitAndWait(
        HangingTriggerResult hangingTriggerResult,
        float timeoutSeconds = 2f,
        CancellationToken cancellationToken = default
    )
    {
        // 如果当前正在撞击，先取消并等待一帧
        if (this.CurrentState == this.State_Hitting)
        {
            this.HitCompletionSource?.TrySetCanceled(cancellationToken);
            await Task.Yield(); // 让状态机有机会处理取消
        }

        this.HitCompletionSource = new TaskCompletionSource();

        // 快速回到队列，以便发起攻击
        this.GlobalPosition = this.QueuePosition;
        this.DisplayScale = IdleScale;
        this.CardNode.Scale = Vector2.One * this.DisplayScale;

        this.FollowTarget = hangingTriggerResult.TargetCreature;
        this.PreviewTarget = hangingTriggerResult.TargetCreature;
        this.TargetGlow = hangingTriggerResult.GlowType;
        this.TargetStateIntent = this.State_Following;
        this.CurrentState = this.State_Hitting; // 立刻改变状态
        try
        {
            var timeoutTask = Task.Delay(TimeSpan.FromSeconds(timeoutSeconds), cancellationToken);
            var completedTask = await Task.WhenAny(this.HitCompletionSource.Task, timeoutTask);
            if (completedTask == timeoutTask)
            {
                // 超时：强制完成并返回队列
                this.HitCompletionSource.TrySetResult();
                this.Command_Return();
            }
        }
        catch (OperationCanceledException)
        {
            this.HitCompletionSource.TrySetCanceled(cancellationToken);
            this.Command_Return();
            throw;
        }
        finally
        {
            this.HitCompletionSource = null;
        }
    }

    public async Task Command_Progress()
    {
        this.ProgressTimer = 0;
        this.CurrentState = this.State_Progressing;
        // 等待动画时间
        await Task.Delay(250);
    }

    /// <summary>
    /// 外部调用此方法来移除卡牌显示
    /// </summary>
    public void Command_Remove() => this.TargetStateIntent = this.State_Removing;
}