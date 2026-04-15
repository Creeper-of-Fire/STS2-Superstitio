using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Runs;
using Superstitio.Main.Features.HangingCard.UI.Patch;

// ReSharper disable NullableWarningSuppressionIsUsed
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释

namespace Superstitio.Main.Features.HangingCard.UI;

/// <summary>
/// 辉光类型，直接对应旧版的逻辑
/// </summary>
public enum HangGlowType
{
    /// <summary>
    /// 不发光
    /// </summary>
    None,

    /// <summary>
    /// 正面效果
    /// </summary>
    Good,

    /// <summary>
    /// 负面效果
    /// </summary>
    Bad,

    /// <summary>
    /// 特殊效果
    /// </summary>
    Special
}

/// <summary>
/// 触发上下文：记录当前触发状态的所有细节
/// </summary>
public record struct TriggerContext(
    HangGlowType GlowType, // 该发什么光
    NCreature? TargetToFollow // 我现在应该飞向哪个生物（如果有）
);

public class HangingCardTokenDisplayer
{
    public HangingCardToken Token { get; init; }

    public HangingCardDisplay Display { get; init; }

    // 环境快照
    protected CardModel? CurrentHoveredCard { get; set; }
    protected NCreature? CurrentHoveredCreature { get; set; }

    public HangingCardTokenDisplayer(HangingCardToken token)
    {
        this.Display = NHangingCardQueue.EnsureCreated().AddCard(token);
        this.Token = token;
        this.SubscribeSignals();
    }

    public void UnHangCard()
    {
        this.Unsubscribe();
        NHangingCardQueue.Instance?.RemoveCard(this.Token);
    }

    private void SubscribeSignals()
    {
        RunManager.Instance.HoveredModelTracker.HoverChanged += this.OnGlobalHoverChanged;
        NTargetManager.Instance.CreatureHovered += this.OnCreatureHovered;
        NTargetManager.Instance.CreatureUnhovered += this.OnCreatureUnhovered;
        NTargetManager.Instance.TargetingBegan += this.OnTargetingBegan;
        NTargetManager.Instance.TargetingEnded += this.OnTargetingEnded;

        HangingSelectionBridge.CardSelectionStateChanged += this.OnCardSelectionStateChanged;
    }

    private void Unsubscribe()
    {
        RunManager.Instance.HoveredModelTracker.HoverChanged -= this.OnGlobalHoverChanged;
        NTargetManager.Instance.CreatureHovered -= this.OnCreatureHovered;
        NTargetManager.Instance.CreatureUnhovered -= this.OnCreatureUnhovered;
        NTargetManager.Instance.TargetingBegan -= this.OnTargetingBegan;
        NTargetManager.Instance.TargetingEnded -= this.OnTargetingEnded;

        HangingSelectionBridge.CardSelectionStateChanged -= this.OnCardSelectionStateChanged;
    }

    private void OnCardSelectionStateChanged()
    {
        this.RefreshVisualState();
    }


    private void OnGlobalHoverChanged(ulong playerId)
    {
        if (playerId != this.Token.OriginalOwner.NetId) return;

        var model = RunManager.Instance.HoveredModelTracker.GetHoveredModel(playerId);
        this.CurrentHoveredCard = model as CardModel;

        this.RefreshVisualState();
    }

    private void OnCreatureHovered(NCreature creature)
    {
        this.CurrentHoveredCreature = creature;
        this.RefreshVisualState();
    }

    private void OnCreatureUnhovered(NCreature creature)
    {
        this.CurrentHoveredCreature = null;
        this.RefreshVisualState();
    }

    private void OnTargetingBegan()
    {
        this.CurrentHoveredCreature = null;
        this.RefreshVisualState();
    }

    private void OnTargetingEnded()
    {
        this.CurrentHoveredCreature = null;
        this.RefreshVisualState();
    }

    /// <summary>
    /// 核心逻辑：刷新视觉意图。
    /// 它是唯一的出口，将 Token 的逻辑结论应用到 Display 上。
    /// </summary>
    private void RefreshVisualState()
    {
        if (this.CurrentHoveredCard is null)
        {
            this.Display.EndGlow();
            this.Display.Command_Return();
            return;
        }

        // 询问 Token：基于当前的“手牌+目标”组合，我该处于什么状态？
        var context = this.Token.GetTriggerContext(this.CurrentHoveredCard, this.CurrentHoveredCreature);

        // 设置辉光意图
        this.Display.StartGlow(context.GlowType);

        // 统一的选择激活状态判定：
        // 1. NTargetManager 认为在选目标 (单体牌)
        // 2. 我们的钩子认为在选目标 (全体/对己/AOE)
        bool selectionActive = NTargetManager.Instance.IsInSelection || HangingSelectionBridge.IsAnyTargetingActive;

        // 设置位置意图（仅在玩家处于目标选择状态时允许跟随）
        if (context.TargetToFollow is not null && selectionActive)
        {
            this.Display.Command_Follow(context.TargetToFollow);
        }
        else
        {
            this.Display.Command_Return();
        }
    }
}