using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Superstitio.Main.Base;
using Superstitio.Main.DynamicVars;
using Superstitio.Main.DynamicVars.Extensions;
using Superstitio.Main.Extensions;
using Superstitio.Main.Features.HangingCard;
using Superstitio.Main.Maso.Base;

namespace Superstitio.Main.Maso.Cards.Kongfu;

/// <summary>
/// Cost 1 造成 3-4 点 伤害 两次。伴随 打出攻击牌 2-1 次：获得 1 力量 。
/// </summary>
public class KoKiHand() : MasoBaseCard(new CardInitMessage
{
    BaseCost = 1,
    Type = CardType.Attack,
    Rarity = CardRarity.Uncommon,
    Target = TargetType.AnyEnemy,
}), IWithHangingConfigCard
{
    /// <inheritdoc />
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
    [
        new DamageVar(3, ValueProp.Move).WithUpgrade(1),
        new TriggerCountVar(2).WithUpgrade(1),
        new PowerVar<StrengthPower>(1).AddToolTips() // 获得
    ];

    /// <inheritdoc />
    public HangingCardConfig HangingCardConfig => new(
        Card: this,
        HangingType: HangingType.Follow,
        CardTypeFilter: CardType.Attack
    );

    /// <inheritdoc />
    public bool HangingSelfAfterPlay => true;

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.AutoAttack(this, cardPlay).Execute(choiceContext);

        var token = this.CreateHangingToken(async (_, _) =>
        {
            await PowerCmd.ApplyByCard<StrengthPower>(this, this.Owner.Creature, this.DynamicVars.Strength.BaseValue);
        });

        await HangingCardManager.HangCard(token, this);
    }
}