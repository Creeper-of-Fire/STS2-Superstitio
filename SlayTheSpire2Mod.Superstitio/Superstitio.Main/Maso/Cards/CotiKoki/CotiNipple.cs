using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Superstitio.Main.Base;
using Superstitio.Main.DynamicVars.Extensions;
using Superstitio.Main.Extensions;
using Superstitio.Main.Features.Milk;
using Superstitio.Main.Maso.Base;

namespace Superstitio.Main.Maso.Cards.CotiKoki;

/**
 * Title = "猴形拳"
 *
 * Description = "对随机敌人造成{Damage:diff()}点伤害，给予{MilkPower:diff()}层[pink]仁慈[/pink]，连续攻击{Repeat:diff()}次。"
 *
 * Flavor = "灵活跳跃的拳法，在混乱的招式中寻找胜机。"
 */
public class CotiNipple() : MasoBaseCard(new CardInitMessage
{
    BaseCost = 1,
    Type = CardType.Attack,
    Rarity = CardRarity.Common,
    Target = TargetType.RandomEnemy
})
{
    private const int Milk = 2;

    /// <inheritdoc />
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
    [
        new DamageVar(5, ValueProp.Move).WithUpgrade(2),
        new PowerVar<MilkPower>(Milk),
        new RepeatVar(2)
    ];

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(this.CombatState);
        for (int i = 0; i < this.DynamicVars.Repeat.IntValue; i++)
        {
            var target = this.Owner.RunState.Rng.CombatTargets.NextItem(this.CombatState.HittableEnemies);
            if (target == null)
                continue;

            await DamageCmd.AutoAttack(this, target, forceAttackTarget: true).Execute(choiceContext);
            await PowerCmd.ApplyByCard<MilkPower>(this, target);
        }
    }
}