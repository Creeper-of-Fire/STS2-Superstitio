using System.Diagnostics.CodeAnalysis;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using Superstitio.Main.Base;
using Superstitio.Main.DynamicVars;
using Superstitio.Main.Features.HangingCard.UI;

namespace Superstitio.Main.Features.HangingCard;


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
        return this.ShouldRespondCard(cardPlay.Card);
    }

    /// <summary>
    /// 是否响应指定的卡牌
    /// </summary>
    protected bool ShouldRespondCard(CardModel card)
    {
        if (this.CardTypeFilter == CardType.None)
            return true;

        return card.Type == this.CardTypeFilter;
    }

    /// <inheritdoc />
    protected override async Task OnEnd(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (this.HangingCardConfig.HangingType != HangingType.Delay)
            return;
        await this.HangingAction(context, cardPlay);
    }

    /// <inheritdoc />
    protected override async Task OnTrigger(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (this.HangingCardConfig.HangingType != HangingType.Follow)
            return;
        await this.HangingAction(context, cardPlay);
    }

    /// <summary>
    /// 当响应指定的卡牌时，创建一个触发上下文
    /// </summary>
    protected abstract TriggerContext CreateTriggerContext(CardModel hoveredCard, NCreature? hoveredCreature);

    /// <inheritdoc />
    public sealed override TriggerContext GetTriggerContext(CardModel hoveredCard, NCreature? hoveredCreature)
    {
        bool shouldRespond = this.ShouldRespondCard(hoveredCard);

        if (!shouldRespond)
            return new TriggerContext(HangGlowType.None, null);

        return this.CreateTriggerContext(hoveredCard, hoveredCreature);
    }
}