using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.ValueProps;
using Superstitio.Main.Base;
using Superstitio.Main.Extensions;
using Superstitio.Main.Features.Milk;
using Superstitio.Main.Lupa.Base;

namespace Superstitio.Main.Lupa.Cards.Rage;

/**
 * Title = "以乳蒙眼"
 *
 * Description = """
 * 对敌人造成{Damage:diff()}点伤害。
 * [gold]击晕[/gold]该敌人。
 * 给予{MilkPower:diff()}层[pink]乳汁[/pink]。
 * """
 *
 * Flavor = "这招如何？"
 *
 * Sfw.Title = "以血蒙眼"
 *
 * Sfw.Flavor = "这招如何？"
 */
public class BlindfoldWithMilk() : LupaBaseCard(new CardInitMessage
{
    BaseCost = 2,
    Type = CardType.Attack,
    Rarity = CardRarity.Rare,
    Target = TargetType.AnyEnemy
})
{
    private const int Damage = 20;
    private const int DamageUpgrade = 8;
    private const int Milk = 6;
    private const int MilkUpgrade = 2;

    /// <inheritdoc />
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
    [
        new DamageVar(Damage, ValueProp.Move).WithUpgrade(DamageUpgrade)
            .AddToolTips(StunIntent.GetStaticHoverTip()),
        new PowerVar<MilkPower>(Milk).WithUpgrade(MilkUpgrade)
            .AddToolTips<MilkPower>(),
    ];

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var target = cardPlay.GetTargetOrThrow();

        await DamageCmd.AutoAttack(this, cardPlay).Execute(choiceContext);

        await CreatureCmd.Stun(target);

        await PowerCmd.ApplyByCard<MilkPower>(this, target);
    }
}