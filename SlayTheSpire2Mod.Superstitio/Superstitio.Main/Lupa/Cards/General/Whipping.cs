using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Superstitio.Main.Base;
using Superstitio.Main.DynamicVars;
using Superstitio.Main.DynamicVars.Extensions;
using Superstitio.Main.Extensions;
using Superstitio.Main.Features.Felix;
using Superstitio.Main.Lupa.Base;

namespace Superstitio.Main.Lupa.Cards.General;

/**
 * Title = "鞭打"
 *
 * Description = """
 * 对所有敌人造成{Damage:diff()}点伤害。
 * 所造成伤害每有{DamageToFelixRate:diff()}点，获得{Felix:diff()}点[pink]快感[/pink]。
 * """
 *
 * Flavor = ""
 *
 * Sfw.Title = "掌握风暴"
 *
 * Sfw.Flavor = "闪电风暴接近中。"
 */
public class Whipping() : LupaBaseCard(new CardInitMessage
{
    BaseCost = 1,
    Type = CardType.Attack,
    Rarity = CardRarity.Common,
    Target = TargetType.AllEnemies
})
{
    private const int BaseDamage = 6;
    private const int DamageUpgradeAmount = 2;
    private const int DamageToFelixRate = 4;
    private const int DamageToFelixRateUpgrade = -1;
    private const int FelixGain = 1;

    /// <inheritdoc />
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
    [
        new DamageVar(BaseDamage, ValueProp.Move).WithUpgrade(DamageUpgradeAmount),
        new DynamicVar(nameof(DamageToFelixRate), DamageToFelixRate).WithUpgrade(DamageToFelixRateUpgrade),
        new FelixVar(FelixGain)
    ];

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var damageResult = await DamageCmd.AutoAttack(this, cardPlay).Execute(choiceContext);

        int totalDamageDealt = damageResult.Results.Select(result => result.TotalDamage + result.OverkillDamage).Sum();

        int felixGainTimes = totalDamageDealt / this.DynamicVars.GetVarOrThrow(nameof(DamageToFelixRate)).IntValue;

        int felixTotalGain = felixGainTimes * this.DynamicVars.Felix.BaseFelix;

        if (felixTotalGain > 0)
            await FelixManager.ModifyFelix(this.Owner.Creature, felixTotalGain, this.Owner.Creature, this);
    }
}