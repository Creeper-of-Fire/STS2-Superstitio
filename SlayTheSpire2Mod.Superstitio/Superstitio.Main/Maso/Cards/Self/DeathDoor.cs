using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Superstitio.Main.Base;
using Superstitio.Main.Extensions;
using Superstitio.Main.Features.PowerCardPower;
using Superstitio.Main.Maso.Base;
using Superstitio.Main.Utils;

namespace Superstitio.Main.Maso.Cards.Self;

/**
 * Title = "濒死体验"
 *
 * Description = """
 * 下{DeathDoorPower:diff()}次，当你要被杀死时，免死并回复到1点生命。
 * 如果你只有1点生命，免疫任何非攻击伤害。
 * """
 *
 * Flavor = "好刺激，好有趣，还想再来。"
 *
 * Power.Description = "当你要被杀死时，免死并回复到1点生命。"
 *
 * Power.SmartDescription = "下[blue]{Amount}[/blue]次，当你要被杀死时，免死并回复到1点生命。"
 *
 * AtDeathDoorPower.Description = "如果你只有1点生命，免疫任何非攻击伤害。"
 *
 * Sfw.Title = "重整旗鼓"
 *
 * Sfw.Flavor = "再来……我不服。"
 */
[IsGuro]
public class DeathDoor() : MasoBaseCard(new CardInitMessage
{
    BaseCost = 1,
    Type = CardType.Power,
    Rarity = CardRarity.Rare,
    Target = TargetType.Self
})
{
    private const int DeathDoorCount = 1;
    private const int DeathDoorCountUpgrade = 1;

    /// <inheritdoc />
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
    [
        new PowerVar<DeathDoorPower>(DeathDoorCount).WithUpgrade(DeathDoorCountUpgrade)
    ];

    /// <inheritdoc cref="DeathDoor"/>
    public class DeathDoorPower() : SimpleCardPower<DeathDoor>(new PowerInitMessage
    {
        Type = PowerType.Buff,
        InitStackType = PowerInitMessage.StackStyle.Normal
    })
    {
        /// <inheritdoc cref="DeathDoor"/>
        public class AtDeathDoorPower() : SimpleCardPower<DeathDoor>(new PowerInitMessage
        {
            Type = PowerType.Buff,
            InitStackType = PowerInitMessage.StackStyle.Singular
        })
        {
            /// <inheritdoc />
            public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer,
                CardModel? cardSource)
            {
                if (target != this.Owner)
                    return 1m;
                if (target.CurrentHp > 1)
                    return 1m;

                if (!props.IsPoweredAttack_())
                    return 0m;

                return 1m;
            }
        }

        /// <inheritdoc />
        public override async Task BeforeApplied(
            Creature target,
            decimal amount,
            Creature? applier,
            CardModel? cardSource)
        {
            await PowerCmd.Apply<AtDeathDoorPower>(target, 1, applier, cardSource, true);
        }

        /// <inheritdoc />
        public override bool ShouldDieLate(Creature creature)
        {
            if (creature != this.Owner)
            {
                return true;
            }

            if (this.Amount <= 0)
            {
                return true;
            }

            return false;
        }

        /// <inheritdoc />
        public override async Task AfterPreventingDeath(Creature creature)
        {
            this.Flash();
            await PowerCmd.Decrement(this);
            const decimal amount = 1m;
            await CreatureCmd.Heal(creature, amount);
        }
    }

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.ApplyByCard<DeathDoorPower>(this, this.Owner.Creature);
    }
}