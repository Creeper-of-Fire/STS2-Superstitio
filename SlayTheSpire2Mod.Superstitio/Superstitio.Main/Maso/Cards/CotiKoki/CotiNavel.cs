using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Superstitio.Main.Base;
using Superstitio.Main.Extensions;
using Superstitio.Main.Features.Felix;
using Superstitio.Main.Maso.Base;

namespace Superstitio.Main.Maso.Cards.CotiKoki;

/**
 * Title = "熊形拳"
 *
 * Description = "对敌人造成{CalculatedDamage:diff()}点伤害，对自身造成{CalculatedDamage:diff()}点伤害。获得{VulnerablePower:diff()}层易伤。"
 *
 * Flavor = "笨重但致命的一击，如狂熊拍击般不顾一切。"
 */
public class CotiNavel() : MasoBaseCard(new CardInitMessage
{
    BaseCost = 2,
    Type = CardType.Attack,
    Rarity = CardRarity.Rare,
    Target = TargetType.AnyEnemy,
})
{
    /// <inheritdoc />
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain];

    /// <inheritdoc />
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
    [
        new PowerVar<VulnerablePower>(1).AddToolTips(),
        new ExtraDamageVar(3).WithUpgrade(1),
        new CalculationBaseVar(15).WithUpgrade(5),
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier((card, _) => FelixManager.GetClimaxRecord(card.Owner.Creature).Count()),
    ];

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 对目标造成伤害
        await DamageCmd.AutoAttack(this, cardPlay).Execute(choiceContext);
        // 对自身造成伤害
        await DamageCmd.AutoAttack(this, this.Owner.Creature).Execute(choiceContext);

        // 自身获得易伤
        await PowerCmd.ApplyByCard<VulnerablePower>(this, this.Owner.Creature);
    }
}