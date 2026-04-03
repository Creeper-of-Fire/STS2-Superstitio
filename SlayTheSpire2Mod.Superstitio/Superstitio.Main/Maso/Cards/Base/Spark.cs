using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Superstitio.Main.Base;
using Superstitio.Main.DynamicVars;
using Superstitio.Main.DynamicVars.Extensions;
using Superstitio.Main.Extensions;
using Superstitio.Main.Features.Corruptus;
using Superstitio.Main.Maso.Base;

namespace Superstitio.Main.Maso.Cards.Base;

/// <summary>
/// 对自身造成伤害，对敌人造成伤害。若腐朽大于，再造成一次伤害。
/// </summary>
public class Spark() : MasoBaseCard(new CardInitMessage()
{
    BaseCost = 1,
    Type = CardType.Attack,
    Rarity = CardRarity.Basic,
    Target = TargetType.AnyEnemy
})
{
    /// <inheritdoc />
    protected override bool ShouldGlowGoldInternal => this.ShouldTrigger;

    /// <summary>
    /// 是否触发
    /// </summary>
    protected bool ShouldTrigger => (this.Owner.Creature.GetPower<CorruptusPower>()?.Amount ?? 0) >=
                                    this.DynamicVars.GetVarOrThrow<DynamicVar>(nameof(CorruptusThreshold)).IntValue;

    private const int DamageSelf = 3;

    private const int CorruptusThreshold = 3;

    private const int Damage = 6;

    private const int DamageUpgrade = 2;

    /// <inheritdoc />
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
    [
        new DamageSelfVar(DamageSelf, ValueProp.Move),
        new DamageVar(Damage, ValueProp.Move).WithUpgrade(DamageUpgrade),
        new DynamicVar(nameof(CorruptusThreshold), CorruptusThreshold).AddToolTips<CorruptusPower>()
    ];

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.AutoAttack(this, this.Owner.Creature, varForDamage: this.DynamicVars.DamageSelf).Execute(choiceContext);

        await DamageCmd.AutoAttack(this, cardPlay).Execute(choiceContext);

        if (this.ShouldTrigger)
            await DamageCmd.AutoAttack(this, cardPlay).Execute(choiceContext);
    }
}