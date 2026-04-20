using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Superstitio.Api.Card;
using Superstitio.Api.DynamicVars;
using Superstitio.Api.Extensions;
using Superstitio.Main.DynamicVars;
using Superstitio.Main.Maso.Base;

namespace Superstitio.Main.Maso.Cards.CotiKoki;

/**
 * Title = "性交-侍奉：腿交"
 *
 * Description = "对敌人造成{Damage:diff()}点伤害，连续攻击{Repeat:diff()}次。每造成未被格挡的伤害，抽{DrawCards:diff()}张牌。"
 *
 * Flavor = "轮奸的时候，如果没有位置就只有这样凑合了，不过好像也有人真的很喜欢？会有吗？"
 *
 * Sfw.Title = "咬耳朵"
 *
 * Sfw.Flavor = "拳王专属。"
 */
public class KokiLegPit() : MasoBaseCard(new CardInitMessage
{
    BaseCost = 1,
    Type = CardType.Attack,
    Rarity = CardRarity.Common,
    Target = TargetType.AnyEnemy,
})
{
    /// <inheritdoc />
    public override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
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