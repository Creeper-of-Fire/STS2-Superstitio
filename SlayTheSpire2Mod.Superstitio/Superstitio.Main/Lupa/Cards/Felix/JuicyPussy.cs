using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Superstitio.Main.Base;
using Superstitio.Main.Extensions;
using Superstitio.Main.Features.Corruptus;
using Superstitio.Main.Features.Felix;
using Superstitio.Main.Features.PowerCardPower;
using Superstitio.Main.Lupa.Base;

namespace Superstitio.Main.Lupa.Cards.Felix;

/**
 * Title = "多汁小穴"
 *
 * Description = "达到[pink]顶峰[/pink]时，移除{JuicyPussyPower:diff()}点[sine][red]腐朽[/red][/sine]。"
 *
 * Flavor = "这就是为什么我不穿内裤。"
 *
 * Power.Description = "达到[pink]顶峰[/pink]时，移除[sine][red]腐朽[/red][/sine]。"
 *
 * Power.SmartDescription = "达到[pink]顶峰[/pink]时，移除[blue]{Amount}[/blue]点[sine][red]腐朽[/red][/sine]。"
 *
 * Sfw.Title = "怒气"
 *
 * Sfw.Flavor = "一怒之下，怒了一下。"
 */
public class JuicyPussy() : LupaBaseCard(new CardInitMessage
{
    BaseCost = 1,
    Type = CardType.Power,
    Rarity = CardRarity.Rare,
    Target = TargetType.Self
})
{
    private const int CorruptusRemoveWhenClimax = 3;
    private const int CorruptusRemoveWhenClimaxUpgrade = 1;

    /// <inheritdoc />
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
    [
        new PowerVar<JuicyPussyPower>(CorruptusRemoveWhenClimax).WithUpgrade(CorruptusRemoveWhenClimaxUpgrade)
            .AddToolTips<JuicyPussyPower>(),
    ];

    /// <inheritdoc cref="JuicyPussy"/>
    public class JuicyPussyPower() : SimpleCardPower<JuicyPussy>(new PowerInitMessage
    {
        Type = PowerType.Debuff,
        InitStackType = PowerInitMessage.StackStyle.Normal
    }), IAfterClimaxReached
    {
        /// <inheritdoc />
        public async Task AfterClimaxReached(Creature powerOwner, FelixPower felixPower, Creature? applier, CardModel? cardSource)
        {
            await CorruptusManager.DecreaseCorruptus(powerOwner, this.Amount, applier, cardSource);
        }
    }

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.ApplyByCard<JuicyPussyPower>(this, this.Owner.Creature);
    }
}