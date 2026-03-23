using System.Diagnostics.CodeAnalysis;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using Superstitio.Main.DynamicVars;

namespace Superstitio.Main.Features.HangingCard;

/// <inheritdoc />
[method: SetsRequiredMembers]
public record AutoHangingCardTokenWithConfig(
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
        HangingDescriptionBuilder.BuildHangingDescription(this.HangingCardConfig).HangingDescription;

    /// <inheritdoc />
    public override IEnumerable<IHoverTip> ExtraHoverTips =>
        HangingDescriptionBuilder.GetHoverTips(
            this.HangingCardConfig with { TriggerCount = new HangingTriggerVar(this.HangingCardConfig.TriggerCount.PreviewTriggers) },
            showHangingTotalDescription: false);

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
        return cardPlay.Card.Type == this.CardTypeFilter;
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
}