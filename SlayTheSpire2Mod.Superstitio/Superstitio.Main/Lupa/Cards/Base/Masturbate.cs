using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using Superstitio.Main.Base;
using Superstitio.Main.DynamicVars;
using Superstitio.Main.DynamicVars.Extensions;
using Superstitio.Main.Features.Felix;
using Superstitio.Main.Features.HangingCard;
using Superstitio.Main.Lupa.Base;

namespace Superstitio.Main.Lupa.Cards.Base;

/// <summary>
/// 自慰 - 0费技能，挂起自身，获得4(6)点快感。打出任意2张牌后，抽1张牌。
/// </summary>
public class Masturbate() : LupaBaseCard(new CardInitMessage
{
    BaseCost = 0,
    Type = CardType.Skill,
    Rarity = CardRarity.Uncommon,
    Target = TargetType.Self,
}), IWithHangingConfigCard
{
    /// <summary>
    /// 基础快感值
    /// </summary>
    private const int FelixGain = 4;

    /// <summary>
    /// 升级增加的快感值
    /// </summary>
    private const int FelixGainUpgrade = 2;

    /// <summary>
    /// 触发抽牌所需打出的牌数
    /// </summary>
    private const int TriggerCount = 2;

    /// <summary>
    /// 触发抽牌的牌数
    /// </summary>
    private const int DrawCard = 1;

    /// <inheritdoc />
    protected override IEnumerable<DynamicVarWithUpgrade> InitVarsWithUpgrade =>
    [
        new FelixVar(FelixGain).WithUpgrade(FelixGainUpgrade),
        new TriggerCountVar(TriggerCount),
        new DrawCardsVar(DrawCard)
    ];

    /// <inheritdoc />
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        ..base.ExtraHoverTips,
        HoverTipFactory.FromPower<FelixPower>()
    ];

    /// <inheritdoc />
    public HangingCardConfig HangingCardConfig => new(
        Card: this,
        HangingType: HangingType.Delay,
        TriggerCount: this.DynamicVars.TriggerCount,
        CardTypeFilter: CardType.None
    );

    /// <inheritdoc />
    public bool HangingSelfAfterPlay => true;

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 获取快感值
        decimal felixAmount = this.DynamicVars.Felix.BaseValue;

        // 使用 FelixManager 增加快感
        await FelixManager.ModifyFelix(this.Owner.Creature, felixAmount, this.Owner.Creature, cardPlay.Card);

        // 创建挂起令牌
        var token = this.CreateHangingToken(async (context, _) =>
        {
            await CardPileCmd.Draw(context, this.DynamicVars.DrawCards.BaseValue, this.Owner, fromHandDraw: true);
        });

        // 挂起自身
        await HangingCardManager.HangCard(token, this);
    }
}