using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Superstitio.Main.Base;
using Superstitio.Main.DynamicVars;
using Superstitio.Main.DynamicVars.Extensions;
using Superstitio.Main.Extensions;
using Superstitio.Main.Features.Felix;
using Superstitio.Main.Features.HangingCard;
using Superstitio.Main.Maso.Base;

namespace Superstitio.Main.Maso.Cards.CotiKoki;

/// <summary>
/// 打出后挂起自身，后续每次打出任意牌时获得2点快感
/// </summary>
public sealed class KokiBlow() : MasoBaseCard(new CardInitMessage
{
    BaseCost = 0,
    Type = CardType.Attack,
    Rarity = CardRarity.Uncommon,
    Target = TargetType.AnyEnemy,
}), IWithHangingConfigCard
{
    /// <inheritdoc />
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
    [
        new DamageVar(5, ValueProp.Move).WithUpgrade(3),
        new TriggerCountVar(3).WithUpgrade(1),
        new FelixVar(2).AddToolTips() // 获得
    ];

    /// <inheritdoc />
    public HangingCardConfig HangingCardConfig => new(
        Card: this,
        HangingType: HangingType.Follow,
        CardTypeFilter: CardType.None
    );

    /// <inheritdoc />
    public bool HangingSelfAfterPlay => true;

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.AutoAttack(this, cardPlay).Execute(choiceContext);

        var token = this.CreateHangingToken(async (_, _) =>
        {
            await FelixManager.ModifyFelix(this.Owner.Creature, this.DynamicVars.Felix.BaseFelix, this.Owner.Creature, this);
        });

        await HangingCardManager.HangCard(token, this);
    }
}