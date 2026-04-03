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
 * 重复两次： NL 对随机敌人 造成 !D! 点 伤害 ，给予 !M! superstitiomod:仁慈 。
 *
        private const val COST = 1
        private const val DAMAGE = 5
        private const val UPGRADE_DAMAGE = 2
        private const val MAGIC = 2
        private const val DAMAGE_TIME = 2
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
        new PowerVar<MilkPower>(nameof(Milk), Milk),
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
            await PowerCmd.ApplyByCard<MilkPower>(this, target, (int)this.DynamicVars.GetVarOrThrow(nameof(Milk)).BaseValue);
        }
    }
}