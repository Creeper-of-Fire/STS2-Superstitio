using System.Diagnostics.CodeAnalysis;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using Superstitio.Main.Base;
using Superstitio.Main.DynamicVars;

namespace Superstitio.Main.Features.HangingCard;

/// <summary>
/// 为实现了 <see cref="IWithHangingConfigCard"/> 接口的卡牌提供创建挂起令牌的扩展方法。
/// </summary>
public static class IWithHangingConfigCardExtensions
{
    /// <summary>
    /// 基于卡牌的挂起配置创建一个 <see cref="AutoHangingCardTokenWithConfig"/> 实例。
    /// </summary>
    /// <typeparam name="TCard">卡牌类型，必须继承自 <see cref="SuperstitioBaseCard"/> 并实现 <see cref="IWithHangingConfigCard"/> 接口。</typeparam>
    /// <param name="card">当前卡牌实例。</param>
    /// <param name="HangingAction">当挂起条件触发时执行的异步动作。</param>
    /// <returns>一个配置好的 <see cref="AutoHangingCardTokenWithConfig"/> 实例。</returns>
    public static AutoHangingCardTokenWithConfig CreateHangingToken<TCard>(
        this TCard card,
        Func<PlayerChoiceContext, CardPlay, Task> HangingAction
    )
        where TCard : SuperstitioBaseCard, IWithHangingConfigCard
    {
        return new AutoHangingCardTokenWithConfig(card.HangingCardConfig, card.BaseResultPileType)
        {
            // 如果卡牌在打出后挂起自身，则手动禁用“挂起后，由 Token 手动从战斗中移除"功能，转而让游戏自动在打出后送入 ResultPileType
            ShouldManualRemoveFromBattle = !card.HangingSelfAfterPlay,
            HangingAction = HangingAction
        };
    }
}

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
        if (this.CardTypeFilter == CardType.None)
            return true;

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