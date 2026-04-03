using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Superstitio.Main.Base;
using Superstitio.Main.DynamicVars;
using Superstitio.Main.DynamicVars.Extensions;
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
    Target = TargetType.AnyPlayer
})
{
    private const int DamageSelf = 3;

    private const int CorruptusThreshold = 3;

    private const int Damage = 4;

    private const int DamageUpgrade = 2;

    /// <inheritdoc />
    protected override IEnumerable<DynamicVarWithUpgrade> InitVarsWithUpgrade =>
    [
        new DamageSelfVar(DamageSelf, ValueProp.Move),
        new DamageVar(Damage, ValueProp.Move).WithUpgrade(DamageUpgrade),
        new DynamicVar(nameof(CorruptusThreshold), CorruptusThreshold)
    ];


    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        await Damage(this.Owner.Creature, this.DynamicVars.DamageSelf.BaseValue);

        await Damage(cardPlay.Target, this.DynamicVars.Damage.BaseValue);

        if ((this.Owner.Creature.GetPower<CorruptusPower>()?.Amount ?? 0) <
            this.DynamicVars.GetVarOrThrow<DynamicVar>(nameof(CorruptusThreshold)).IntValue) return;

        await Damage(cardPlay.Target, this.DynamicVars.Damage.BaseValue);

        return;

        async Task Damage(Creature target, decimal amount)
        {
            await DamageCmd.Attack(amount).FromCard(this)
                .Targeting(target).WithHitFx("vfx/vfx_attack_slash").Execute(choiceContext);
        }
    }
}