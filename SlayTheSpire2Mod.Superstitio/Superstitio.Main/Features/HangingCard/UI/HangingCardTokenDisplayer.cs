using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Runs;

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
    bool IsMatch,
    HangGlowType GlowType = HangGlowType.None,
    string? CustomHint = null
);

public partial class HangingCardTokenDisplayer
{
    public HangingCardToken Token { get; init; }
    public HangingCardDisplay Display { get; init; }

    // 存储当前的触发上下文，比单纯的 bool 携带更多信息
    public TriggerContext CurrentContext { get; set; } = new(false);

    public HangingCardTokenDisplayer(HangingCardToken token)
    {
        Display = NHangingCardQueue.EnsureCreated().AddCard(token);
        Token = token;
        SubscribeSignals();
    }

    public void UnHangCard()
    {
        this.Unsubscribe();
        NHangingCardQueue.Instance?.RemoveCard(Token);
    }

    private void SubscribeSignals()
    {
        RunManager.Instance.HoveredModelTracker.HoverChanged += OnGlobalHoverChanged;
        NTargetManager.Instance.CreatureHovered += OnCreatureHovered;
        NTargetManager.Instance.TargetingEnded += OnTargetingEnded;
    }

    private void Unsubscribe()
    {
        RunManager.Instance.HoveredModelTracker.HoverChanged -= OnGlobalHoverChanged;
        NTargetManager.Instance.CreatureHovered -= OnCreatureHovered;
        NTargetManager.Instance.TargetingEnded -= OnTargetingEnded;
    }

    private void OnGlobalHoverChanged(ulong playerId)
    {
        var tracker = RunManager.Instance.HoveredModelTracker;
        var model = tracker.GetHoveredModel(playerId);

        if (model is CardModel card)
        {
            // 调用 Token 里的高级过滤逻辑
            CurrentContext = Token.GetTriggerContext(card);

            if (CurrentContext.IsMatch)
            {
                Display.Command_Anticipate(CurrentContext.GlowType);
                return;
            }
        }

        // 不匹配或失去 Hover
        CurrentContext = new(false);
        if (Display.CurrentState == Display.State_InQueue)
            Display.Command_Idle();
    }

    private void OnCreatureHovered(NCreature creature)
    {
        // 只有在匹配上下文且玩家正在选目标时
        if (CurrentContext.IsMatch && NTargetManager.Instance.IsInSelection)
        {
            var offset = creature.Entity.IsPlayer ? new Vector2(-120, -60) : new Vector2(120, -60);
            Display.Command_Follow(creature, offset);
        }
    }

    private void OnTargetingEnded()
    {
        Display.Command_Return();
    }
}