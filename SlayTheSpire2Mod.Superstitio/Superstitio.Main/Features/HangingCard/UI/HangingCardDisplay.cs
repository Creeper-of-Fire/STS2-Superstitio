using Godot;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Combat;

// ReSharper disable NullableWarningSuppressionIsUsed
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释

namespace Superstitio.Main.Features.HangingCard.UI;

/// <summary>
/// 状态机委托：执行逻辑并返回下一个状态函数
/// </summary>
public delegate HangingCardState HangingCardState(double delta);

public partial class HangingCardDisplay
{
    // --- 手动绑定：方法名常量 ---
    public new class MethodName : Node2D.MethodName
    {
        public new static readonly StringName _Ready = "_Ready";
        public new static readonly StringName _Process = "_Process";
        public new static readonly StringName _ExitTree = "_ExitTree";
    }

    // --- 手动绑定：HasGodotClassMethod ---
    // 引擎在决定是否调用 _Process 之前，会先问这个
    protected override bool HasGodotClassMethod(in godot_string_name method)
    {
        if (method == MethodName._Ready) return true;
        if (method == MethodName._Process) return true;
        if (method == MethodName._ExitTree) return true;
        return base.HasGodotClassMethod(in method);
    }

    // --- 手动绑定：InvokeGodotClassMethod ---
    // 引擎真正分发调用的入口
    protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
    {
        if (method == MethodName._Ready && args.Count == 0)
        {
            this._Ready();
            ret = default;
            return true;
        }

        if (method == MethodName._Process && args.Count == 1)
        {
            // 将 Variant 参数转换为 double delta
            this._Process(VariantUtils.ConvertTo<double>(in args[0]));
            ret = default;
            return true;
        }

        if (method == MethodName._ExitTree && args.Count == 0)
        {
            this._ExitTree();
            ret = default;
            return true;
        }

        return base.InvokeGodotClassMethod(in method, args, out ret);
    }
}

public partial class HangingCardDisplay : Node2D
{
    // --- 属性区域 ---

    public HangingCardToken Token { get; private set; }
    public HangingCardState CurrentState { get; private set; }

    public Vector2 VisualOffset { get; set; } // 用于弹起动画的额外偏移

    // 队列中的原始位置
    public Vector2 QueuePosition { get; set; }

    // 当前追踪的场景节点（可能是怪物或玩家）
    public NCreature? FollowTarget { get; set; }

    // 追踪时的偏移量
    public Vector2 FollowOffset { get; set; }

    public NCard CardNode { get; protected set; } = null!;
    public bool IsMouseOver { get; set; } = false;

    public float DisplayScale
    {
        get;
        set => field = Mathf.Max(0, value);
    } = 0.25f;

    public BobEffect Bob { get; init; } = new();

    public HangingCardDisplay(HangingCardToken token)
    {
        this.Token = token;
        this.CurrentState = this.State_InQueue;
    }

    public override void _Ready()
    {
        this.CardNode = NCard.Create(this.Token.HangingCard)!;
        this.AddChild(this.CardNode);

        // _counterLabel = new Label {
        //     Text = token.RemainCount.ToString(),
        //     Position = new Vector2(50, -10)
        // };
        // AddChild(_counterLabel);

        // 简单 Hitbox 用于响应 UI 层面的 Hover
        var hb = new Control { Size = new Vector2(160, 220), Position = new Vector2(-80, -110) };
        hb.MouseFilter = Control.MouseFilterEnum.Pass;
        hb.Connect(Control.SignalName.MouseEntered, Callable.From(() => this.IsMouseOver = true));
        hb.Connect(Control.SignalName.MouseExited, Callable.From(() => this.IsMouseOver = false));
        this.AddChild(hb);
    }

    public NCreature? PreviewTarget
    {
        get => field;
        set
        {
            if (field != value)
            {
                field = value;
                this.RequestVisualUpdate();
            }
        }
    }


    // 视觉更新请求标志，防止一帧内多次重复更新
    public void RequestVisualUpdate() => this.NeedsVisualUpdate = true;
    protected bool NeedsVisualUpdate { get; set; } = true;

    public override void _Process(double delta)
    {
        // 确保节点 Ready 后才开始后续逻辑
        if (!this.CardNode.IsNodeReady())
            return;

        // 检查是否需要刷新动态变量 (描述文字、数值、颜色)
        if (this.NeedsVisualUpdate)
        {
            // 调用源码里的核心方法
            // 第一个参数设为 None 以避免触发手牌逻辑
            // 第二个参数 Normal 会触发伤害预览计算
            this.CardNode.UpdateVisuals(PileType.None, CardPreviewMode.Normal);

            // 如果有特定的预览目标（正在追踪的怪物），则告诉 NCard
            if (this.PreviewTarget != null)
            {
                this.CardNode.SetPreviewTarget(this.PreviewTarget.Entity); // 传入 Entity 模型
                // 这里内部会再次调用 UpdateVisuals，这是合理的，因为我们这一次对预览目标进行了更新
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
        Color targetColor = this.TargetGlow switch
        {
            HangGlowType.Good => Colors.Green,
            HangGlowType.Bad => Colors.Red,
            HangGlowType.Special => Colors.Gold,
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
    private const float FollowScale = 0.45f;

    // --- 状态机原子动作 ---

    /// <summary>
    /// 状态：在 UI 队列中。仅在此状态下处理 Hover 放大逻辑。
    /// </summary>
    public HangingCardState State_InQueue(double delta)
    {
        // 处理队列位置 + 弹起偏移 + 浮动
        float bobOffset = this.Bob.Update(delta);
        var targetPos = this.QueuePosition + this.VisualOffset + new Vector2(0, bobOffset);
        this.GlobalPosition = this.GlobalPosition.Lerp(targetPos, (float)delta * 10f);
        this.VisualOffset = this.VisualOffset.Lerp(Vector2.Zero, (float)delta * 5f);

        // 处理缩放
        float targetScale = this.IsMouseOver ? HoverScale : IdleScale;
        this.DisplayScale = Mathf.Lerp(this.DisplayScale, targetScale, (float)delta * 10f);
        this.CardNode.Scale = Vector2.One * this.DisplayScale;

        return this.State_InQueue;
    }

    /// <summary>
    /// 状态：追踪目标（怪物/玩家）。
    /// </summary>
    protected HangingCardState State_Following(double delta)
    {
        if (!IsInstanceValid(this.FollowTarget)) return this.State_Returning;

        // 飞向目标生物的中心点 + 指定偏移
        Vector2 globalTarget = this.FollowTarget.GlobalPosition + this.FollowOffset;
        this.GlobalPosition = this.GlobalPosition.Lerp(globalTarget, (float)delta * 12f);

        this.DisplayScale = Mathf.Lerp(this.DisplayScale, FollowScale, (float)delta * 10f);
        this.CardNode.Scale = Vector2.One * this.DisplayScale;

        return this.State_Following;
    }

    /// <summary>
    /// 状态：撞击动作。
    /// </summary>
    protected HangingCardState State_Hitting(double delta)
    {
        // 快速插值到目标点
        if (!IsInstanceValid(this.FollowTarget)) return this.State_Returning;

        this.GlobalPosition = this.GlobalPosition.Lerp(this.FollowTarget.GlobalPosition, (float)delta * 25f);

        // 如果足够近，产生冲击感并返回
        if (this.GlobalPosition.DistanceTo(this.FollowTarget.GlobalPosition) < 20f)
        {
            // 这里可以触发特定的卡牌闪光特效
            this.CardNode.CardHighlight.AnimShow();
            return this.State_Returning;
        }

        return this.State_Hitting;
    }

    /// <summary>
    /// 状态：返回队列。
    /// </summary>
    protected HangingCardState State_Returning(double delta)
    {
        this.GlobalPosition = this.GlobalPosition.Lerp(this.QueuePosition, (float)delta * 15f);
        this.DisplayScale = Mathf.Lerp(this.DisplayScale, IdleScale, (float)delta * 10f);
        this.CardNode.Scale = Vector2.One * this.DisplayScale;

        if (this.GlobalPosition.DistanceTo(this.QueuePosition) < 5f)
        {
            this.Bob.Reset(); // 回到队列瞬间重置，避免位置突跳
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

    // --- 指挥接口 (供 Token 调用) ---

    public void Command_Anticipate(HangGlowType type)
    {
        this.VisualOffset = new Vector2(0, -100);
        this.TargetGlow = type;
    }

    public void Command_Idle()
    {
        this.TargetGlow = HangGlowType.None;
    }

    /// <summary>
    /// 跟随指定对象
    /// </summary>
    public void Command_Follow(NCreature target, Vector2 offset)
    {
        this.FollowTarget = target;
        this.FollowOffset = offset;
        this.PreviewTarget = target;
        this.CurrentState = this.State_Following;
    }

    /// <summary>
    /// 在出击后，返回悬挂队列
    /// </summary>
    public void Command_Return()
    {
        this.PreviewTarget = null;
        this.CurrentState = this.State_Returning;
    }

    /// <summary>
    /// 攻击指定对象
    /// </summary>
    public void Command_Hit(NCreature target)
    {
        this.FollowTarget = target;
        this.CurrentState = this.State_Hitting;
    }

    /// <summary>
    /// 外部调用此方法来移除卡牌显示
    /// </summary>
    public void Command_Remove() => this.CurrentState = this.State_Removing;
}