using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Combat;

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

    public HangingCardState CurrentState
    {
        get;
        private set
        {
            field = value;
            this.UpdateHitboxState();
        }
    } = null!;

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
        display.CurrentState = display.State_InQueue;
        return display;
    }

    public Control Hitbox { get; private set; } = null!;

    private void UpdateHitboxState()
    {
        if (!this.IsNodeReady())
            return;
        this.Hitbox.MouseFilter = this.IsIdleState
            ? Control.MouseFilterEnum.Pass
            : Control.MouseFilterEnum.Ignore;
    }

    private bool IsIdleState =>
        this.CurrentState == this.State_InQueue;

    public override void _Ready()
    {
        this.CardNode = NCard.Create(this.Token.HangingCard)!;
        this.AddChild(this.CardNode);

        // TODO 加入一个计数器，这个不急，要做的还有很多
        // _counterLabel = new Label {
        //     Text = token.RemainCount.ToString(),
        //     Position = new Vector2(50, -10)
        // };
        // AddChild(_counterLabel);

        // 简单 Hitbox 用于响应 UI 层面的 Hover
        this.Hitbox = new Control { Size = new Vector2(160, 220), Position = new Vector2(-80, -110) };
        this.Hitbox.MouseFilter = Control.MouseFilterEnum.Pass;
        this.Hitbox.Connect(Control.SignalName.MouseEntered, Callable.From(() => this.IsMouseOver = true));
        this.Hitbox.Connect(Control.SignalName.MouseExited, Callable.From(() => this.IsMouseOver = false));
        this.AddChild(this.Hitbox);
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
            if (model != null)
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
    private const float FollowScale = 0.25f;

    /// <summary>
    /// 获取目标位置（中心点）
    /// </summary>
    /// <param name="target">目标生物</param>
    /// <param name="lerpFactor">插值系数：0 = 队列位置，1 = 怪物边缘</param>
    private Vector2 GetTargetCenter(NCreature target, float lerpFactor)
    {
        // 获取怪物的碰撞箱尺寸
        Vector2 hitboxSize = target.Hitbox.Size;

        // 获取卡牌尺寸
        Vector2 cardSize = this.CardNode.GetCurrentSize();

        // 计算从队列位置指向怪物中心的方向
        Vector2 queuePos = this.QueuePosition;
        Vector2 monsterCenter = target.Hitbox.GlobalPosition + hitboxSize / 2f;
        Vector2 directionFromMonster = (queuePos - monsterCenter).Normalized();

        // 计算怪物碰撞箱在该方向上的投影半径
        // 对于矩形碰撞箱，根据方向计算精确的边界距离
        float monsterRadius = GetRectangleRadiusInDirection(hitboxSize, directionFromMonster);
        float cardRadius = GetRectangleRadiusInDirection(cardSize, directionFromMonster);

        // 停在碰撞箱边缘
        Vector2 edgePos = monsterCenter + directionFromMonster * (monsterRadius + cardRadius);

        Vector2 targetPos = queuePos.Lerp(edgePos, lerpFactor);

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
            return (halfWidth * halfHeight) / Mathf.Sqrt(
                halfWidth * halfWidth * dy * dy +
                halfHeight * halfHeight * dx * dx
            ) * Mathf.Sqrt(dx * dx + dy * dy);
        }

        return 0f;
    }

    // 位置移动速度
    private const float MoveSpeedFollow = 8f;
    private const float MoveSpeedHit = 15f;
    private const float MoveSpeedReturn = 10f;

    // 缩放速度
    private const float ScaleLerpSpeed = 3f;

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
        this.DisplayScale = Mathf.Lerp(this.DisplayScale, targetScale, (float)delta * ScaleLerpSpeed);
        this.CardNode.Scale = Vector2.One * this.DisplayScale;

        return this.State_InQueue;
    }

    /// <summary>
    /// 状态：追踪目标（怪物/玩家）。
    /// </summary>
    protected HangingCardState State_Following(double delta)
    {
        if (!IsInstanceValid(this.FollowTarget)) return this.State_Returning;

        Vector2 targetPos = this.GetTargetCenter(this.FollowTarget, FollowLerpFactor);

        this.GlobalPosition = this.GlobalPosition.Lerp(targetPos, (float)delta * MoveSpeedFollow);
        this.DisplayScale = Mathf.Lerp(this.DisplayScale, FollowScale, (float)delta * ScaleLerpSpeed);
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

        Vector2 targetPos = GetTargetCenter(this.FollowTarget, HitLerpFactor);

        this.GlobalPosition = this.GlobalPosition.Lerp(targetPos, (float)delta * MoveSpeedHit);

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
        this.GlobalPosition = this.GlobalPosition.Lerp(this.QueuePosition, (float)delta * MoveSpeedReturn);
        this.DisplayScale = Mathf.Lerp(this.DisplayScale, IdleScale, (float)delta * ScaleLerpSpeed);
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

    public void StartGlow(HangGlowType type)
    {
        this.VisualOffset = new Vector2(0, -100);
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