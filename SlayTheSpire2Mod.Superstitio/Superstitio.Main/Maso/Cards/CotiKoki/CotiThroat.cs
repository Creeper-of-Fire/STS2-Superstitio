using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Superstitio.Api.BaseLib.HangingCard;
using Superstitio.Api.Card;
using Superstitio.Api.Extensions;
using Superstitio.Api.HangingCard;
using Superstitio.Main.Maso.Base;

namespace Superstitio.Main.Maso.Cards.CotiKoki;

/**
 * Title = "性交-插入：深喉"
 *
 * Description = "对敌人造成{Damage:diff()}点伤害。如果有挂起的牌，获得{Energy:energyIcons()}。"
 *
 * Flavor = "经过练习就不会反胃了，真好。"
 *
 * Sfw.Title = "虎吼拳"
 *
 * Sfw.Flavor = "如虎啸山林，其势不仅伤敌，更能振奋士气。"
 */
public class CotiThroat() : MasoBaseCard(new CardInitMessage
{
    BaseCost = 2,
    Type = CardType.Attack,
    Rarity = CardRarity.Common,
    Target = TargetType.AnyEnemy,
})
{
    /// <inheritdoc />
    protected override bool ShouldGlowGoldInternal => this.HasHangingCards;

    private bool HasHangingCards => HangingCardManager.GetHangingCardTokens<HangingCardToken>(this.Owner).Any();

    /// <inheritdoc/>
    public override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
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