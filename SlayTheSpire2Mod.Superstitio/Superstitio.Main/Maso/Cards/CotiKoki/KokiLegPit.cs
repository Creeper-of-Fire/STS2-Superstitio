using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Superstitio.Main.Base;
using Superstitio.Main.DynamicVars;
using Superstitio.Main.DynamicVars.Extensions;
using Superstitio.Main.Extensions;
using Superstitio.Main.Maso.Base;

namespace Superstitio.Main.Maso.Cards.CotiKoki;

/// <summary>
/// Cost 1，造成 4-2 点 伤害 两次。每造成未被格挡的伤害，抽 1 张牌。
/// </summary>
public class KokiLegPit() : MasoBaseCard(new CardInitMessage
{
    BaseCost = 1,
    Type = CardType.Attack,
    Rarity = CardRarity.Common,
    Target = TargetType.AnyEnemy,
})
{
    /// <inheritdoc />
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
    [
        new DamageVar(4, ValueProp.Move).WithUpgrade(2),
        new RepeatVar(2),
        new DrawCardsVar(1)
    ];

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var attackCommand = await DamageCmd.AutoAttack(this, cardPlay, hitCount: this.DynamicVars.Repeat.IntValue).Execute(choiceContext);
        foreach (var unused in attackCommand.Results.ToList().Where(it => it.UnblockedDamage > 0))
        {
            await CardPileCmd.Draw(choiceContext, this.DynamicVars.DrawCards.IntValue, this.Owner);
        }
    }
}