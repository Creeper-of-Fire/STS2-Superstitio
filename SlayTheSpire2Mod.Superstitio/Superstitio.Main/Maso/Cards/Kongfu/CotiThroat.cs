using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Superstitio.Main.Base;
using Superstitio.Main.Extensions;
using Superstitio.Main.Features.HangingCard;
using Superstitio.Main.Maso.Base;

namespace Superstitio.Main.Maso.Cards.Kongfu;

/**
 * 造成 !D! 点 伤害 。 NL 如果 superstitiomod:合力序列 不为空， NL 获得 !M! 的 [E] 。
        private const val COST = 2
        private const val DAMAGE = 10
        private const val UPGRADE_DAMAGE = 3
        private const val MagicNum = 2
 */
public class CotiThroat() : MasoBaseCard(new CardInitMessage
{
    BaseCost = 2,
    Type = CardType.Attack,
    Rarity = CardRarity.Uncommon,
    Target = TargetType.AnyEnemy,
})
{
    /// <inheritdoc />
    protected override bool ShouldGlowGoldInternal => this.HasHangingCards;

    private bool HasHangingCards => HangingCardManager.GetHangingCardTokens<HangingCardToken>(this.Owner).Any();

    /// <inheritdoc/>
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
    [
        new DamageVar(10, ValueProp.Move).WithUpgrade(3),
        new EnergyVar(2)
    ];

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.AutoAttack(this, cardPlay).Execute(choiceContext);

        if (this.HasHangingCards)
        {
            await PlayerCmd.GainEnergy(this.DynamicVars.Energy.BaseValue, this.Owner);
        }
    }
}