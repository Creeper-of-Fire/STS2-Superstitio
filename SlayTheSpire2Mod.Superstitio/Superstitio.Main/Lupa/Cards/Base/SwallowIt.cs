using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Superstitio.Main.Base;
using Superstitio.Main.DynamicVars;
using Superstitio.Main.Features.Corruptus;
using Superstitio.Main.Features.PowerCardPower;
using Superstitio.Main.Lupa.Base;

namespace Superstitio.Main.Lupa.Cards.Base;

/**
 * Title = "吞精"
 *
 * Description = """
 * 获得{Block:diff()}点[gold]格挡[/gold]。
 * 下次你失去生命时，将其转换为[sine][red]腐朽[/red][/sine]。
 * """
 *
 * Flavor = "看，一滴都不剩哦。"
 *
 * Power.Description= """
 * 下次你失去生命时，将其转换为[sine][red]腐朽[/red][/sine]。
 * """
 *
 * Power.SmartDescription= """
 * 下{Amount}次你失去生命时，将其转换为[sine][red]腐朽[/red][/sine]。
 * """
 * 
 * Sfw.Title = "挥袖"
 *
 * Sfw.Flavor = "此事平淡无奇。"
 */
public class SwallowIt() : LupaBaseCard(new CardInitMessage
{
    BaseCost = 1,
    Rarity = CardRarity.Basic,
    Target = TargetType.Self,
    Type = CardType.Skill
})
{
    /// <inheritdoc />
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Retain,
    ];

    /// <inheritdoc />
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
    [
        new BlockVar(5, ValueProp.Move).WithUpgrade(3),
        new PowerVar<SwallowItPower>(1),
    ];

    /// <inheritdoc cref="SwallowIt"/>
    public class SwallowItPower() : SimpleCardPower<SwallowIt>(new PowerInitMessage
    {
        Type = PowerType.Buff,
        InitStackType = PowerInitMessage.StackStyle.Normal,
    }), ICorruptusBuffer
    {
        /// <inheritdoc />
        public CorruptusBufferComponent CorruptusBufferComponent => field ??= new CorruptusBufferComponent(this);

        /// <inheritdoc />
        public override decimal ModifyHpLostAfterOstyLate(
            Creature target,
            decimal amount,
            ValueProp props,
            Creature? dealer,
            CardModel? cardSource
        ) => this.CorruptusBufferComponent.ModifyHpLostAfterOstyLate(target, amount, props, dealer, cardSource)
            .AfterModify(async () => await PowerCmd.Decrement(this));

        /// <inheritdoc />
        public override async Task AfterModifyingHpLostAfterOsty() =>
            await this.CorruptusBufferComponent.AfterModifyingHpLostAfterOsty();

        /// <inheritdoc />
        public Creature OwnerCreature => this.Owner;
    }
}