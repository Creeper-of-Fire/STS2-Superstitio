using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace Superstitio.Main.Features.HangingCard;

/// <summary>
/// 挂起卡牌
/// </summary>
public class HangingCardPower : CustomPowerModel, IHangingCarrier
{
    /// <inheritdoc />
    public override PowerType Type => PowerType.Buff;

    /// <inheritdoc />
    public override PowerStackType StackType => PowerStackType.Single;

    /// <inheritdoc />
    public override bool IsInstanced => true;

    /// <summary>
    /// 在 hover 提示中显示被吸收的卡牌
    /// </summary>
    /// <inheritdoc />
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        this.HangingCard is null ? [] : [HoverTipFactory.FromCard(this.HangingCard)];

    /// <summary>
    /// 挂起卡牌的挂起者
    /// </summary>
    public HangingCardToken? HangingCardToken { get; set; }

    /// <summary>
    /// 被挂起的卡牌
    /// </summary>
    public CardModel? HangingCard => this.HangingCardToken?.HangingCard;

    /// <summary>
    /// 卡牌的原始拥有者
    /// </summary>
    public Player? OriginalOwner => this.HangingCardToken?.OriginalOwner;
    
    /// <summary>
    /// 挂起卡牌
    /// </summary>
    public async Task HangCard(HangingCardToken hangingCardToken) => await hangingCardToken.HangCard(this);

    /// <inheritdoc />
    public async Task HangingTerminate() => await PowerCmd.Remove(this);

    /// <inheritdoc />
    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (this.HangingCardToken is null)
            return;

        await this.HangingCardToken.AfterCardPlayed(context, cardPlay);

        if (this.HangingCardToken is null || this.HangingCardToken.RemainCount <= 0)
            return;

        await PowerCmd.ModifyAmount(
            this,
            this.HangingCardToken.RemainCount - this.Amount,
            null,
            cardPlay.Card
        );
    }
}