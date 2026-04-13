using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Superstitio.Main.Base;
using Superstitio.Main.Extensions;
using Superstitio.Main.Features.Felix;
using Superstitio.Main.Lupa.Base;

namespace Superstitio.Main.Lupa.Cards.Rage;

/**
 * Title = "正字记号"
 *
 * Description = """
 * 造成{CalculatedDamage:diff()}点伤害。
 * 本场战斗中每高潮一次，额外造成{ExtraDamage:diff()}点伤害。
 * """
 *
 * Flavor = "这是荣耀的记号！"
 *
 * Sfw.Title = "盛怒"
 *
 * Sfw.Flavor = "我真的很生气。"
 */
public class CountSign() : LupaBaseCard(new CardInitMessage
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
        new ExtraDamageVar(3).WithUpgrade(1),
        new CalculationBaseVar(12).WithUpgrade(4),
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier((card, _) => FelixManager.GetClimaxRecord(card.Owner.Creature).Count()),
    ];

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.AutoAttack(this, cardPlay).Execute(choiceContext);
    }
}