using System.Diagnostics.CodeAnalysis;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using Superstitio.Api.HangingCard;
using Superstitio.Api.HangingCard.UI;

namespace Superstitio.Api.BaseLib.HangingCard;

/// <inheritdoc />
[method: SetsRequiredMembers]
public abstract record AutoHangingCardTokenWithConfig(
    HangingCardConfig HangingCardConfig,
    PileType ReturnPileType
) : AutoHangingCardToken(
    RemainCount: HangingCardConfig.TriggerCount.PreviewTriggers,
    InitialCount: HangingCardConfig.TriggerCount.BaseTriggers,
    HangingCard: HangingCardConfig.Card,
    OriginalOwner: HangingCardConfig.Card.Owner,
    ReturnPileType: ReturnPileType
)
{
    /// <inheritdoc />
    public override LocString Description =>
        HangingDescriptionBuilder.BuildHangingDescription(this.HangingCardConfig with
        {
            TriggerCount = new TriggerCountVar(this.RemainCount)
        }).HangingDescription;

    /// <inheritdoc />
    public override IEnumerable<IHoverTip> ExtraHoverTips =>
        HangingDescriptionBuilder.GetHoverTips(this.HangingCardConfig with
        {
            TriggerCount = new TriggerCountVar(this.RemainCount)
        }, showHangingTotalDescription: false);

    /// <summary>
    /// 
    /// </summary>
    public CardType CardTypeFilter { get; init; } = HangingCardConfig.CardTypeFilter;

    /// <summary>
    /// 
    /// </summary>
    public Func<PlayerChoiceContext, CardPlay, Task> HangingAction { get; init; } = (_, _) => Task.CompletedTask;

    /// <summary>
    /// 
    /// </summary>
    public HangingCardConfig HangingCardConfig { get; init; } = HangingCardConfig;

    /// <inheritdoc />
    protected override bool ShouldRespond(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (cardPlay.Card is INoTriggerHangingOnPlay)
            return false;
        return this.ShouldRespondCard(cardPlay.Card);
    }

    /// <summary>
    /// 是否响应指定的卡牌（用于预览和响应）
    /// </summary>
    protected bool ShouldRespondCard(CardModel card)
    {
        if (this.CardTypeFilter == CardType.None)
            return true;

        return card.Type == this.CardTypeFilter;
    }

    /// <inheritdoc />
    public override async Task OnTrigger(PlayerChoiceContext context, CardPlay cardPlay)
    {
        switch (this.HangingCardConfig.HangingType)
        {
            case HangingType.Follow:
                await this.PlayHitVfx(context, cardPlay);
                await this.HangingAction(context, cardPlay);
                return;
            // 如果是 Delay 类型
            case HangingType.Delay when this.RemainCount > 1:
                if (this.Displayer != null)
                    await this.Displayer.Display.Command_Progress();
                return;
            default:
                return;
        }
    }

    /// <inheritdoc />
    public override async Task OnEnd(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (this.HangingCardConfig.HangingType != HangingType.Delay)
            return;
        await this.PlayHitVfx(context, cardPlay);
        await this.HangingAction(context, cardPlay);
    }

    private async Task PlayHitVfx(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var hangingTriggerContext = new HangingTriggerContext
        {
            HoveredCard = cardPlay.Card,
            HoveredCreature = NCombatRoom.Instance?.GetCreatureNode(cardPlay.Target),
            IsTargetingActive = true,
        };

        var hangingTriggerResult = this.CreateTriggerResult(hangingTriggerContext);

        if (this.Displayer is null)
            return;

        await this.Displayer.Display.Command_HitAndWait(hangingTriggerResult); // TODO 也许可以加一个无Hit使用效果的功能
    }

    /// <summary>
    /// 当响应指定的卡牌时，创建一个触发上下文
    /// </summary>
    protected abstract HangingTriggerResult CreateTriggerResult(HangingTriggerContext context);

    /// <inheritdoc />
    public sealed override HangingTriggerResult? GetTriggerResult(HangingTriggerContext context)
    {
        // 如果不响应这张牌，直接不发光
        if (!this.ShouldRespondCard(context.HoveredCard))
            return null;

        switch (this.HangingCardConfig.HangingType)
        {
            // 根据挂起类型和计数决定预览形态
            case HangingType.Delay when this.RemainCount > 1:
                // 返回 ProgressCount 辉光，且 TargetCreature 必须为 null
                // 这样卡牌就不会飞向怪物，而是留在原地，但会发出“推进中”的颜色
                return new HangingTriggerResult(HangGlowType.ProgressCount, null);
            default:
                return this.CreateTriggerResult(context);
        }
    }
}