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

public class HangingCardTokenDisplayer
{
    public HangingCardToken Token { get; init; }
    public HangingCardDisplay Display { get; init; }

    // 存储当前的触发上下文，比单纯的 bool 携带更多信息
    public TriggerContext CurrentContext { get; set; } = new(false);

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
        NTargetManager.Instance.TargetingEnded += this.OnTargetingEnded;
    }

    private void Unsubscribe()
    {
        RunManager.Instance.HoveredModelTracker.HoverChanged -= this.OnGlobalHoverChanged;
        NTargetManager.Instance.CreatureHovered -= this.OnCreatureHovered;
        NTargetManager.Instance.CreatureUnhovered -= this.OnCreatureUnhovered;
        NTargetManager.Instance.TargetingEnded -= this.OnTargetingEnded;
    }


    private void OnGlobalHoverChanged(ulong playerId)
    {
        if (playerId != this.Token.OriginalOwner.NetId) return;

        var tracker = RunManager.Instance.HoveredModelTracker;
        var model = tracker.GetHoveredModel(playerId);

        if (model is CardModel card)
        {
            // 调用 Token 里的高级过滤逻辑
            this.CurrentContext = this.Token.GetTriggerContext(card);

            if (this.CurrentContext.IsMatch)
            {
                this.Display.StartGlow(this.CurrentContext.GlowType);
                return;
            }
        }

        // 不匹配或失去 Hover
        this.CurrentContext = new TriggerContext(false);
        this.Display.EndGlow(); // TODO 正确情况应该是 this.Display 维护一个 CurrentContext，以决定自己的发光情况
    }

    private void OnCreatureHovered(NCreature creature)
    {
        // 只有在匹配上下文且玩家正在选目标时
        if (!this.CurrentContext.IsMatch || !NTargetManager.Instance.IsInSelection)
            return;

        this.Display.Command_Follow(creature); // TODO 是否要Follow应该是Token自己决定的，而不是交给 HangingCardTokenDisplayer
    }

    private void OnCreatureUnhovered(NCreature creature)
    {
        this.Display.Command_Return();
    }

    private void OnTargetingEnded()
    {
        this.Display.Command_Return();
    }
}