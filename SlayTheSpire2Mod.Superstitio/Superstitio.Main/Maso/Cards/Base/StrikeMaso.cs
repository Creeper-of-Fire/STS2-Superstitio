using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Superstitio.Main.Maso.Cards.Base;

/// <summary>
/// 攻击
/// </summary>

public sealed class StrikeMaso() : MasoBaseCard(new()
{
    BaseCost = 1,
    Type = CardType.Attack,
    Rarity = CardRarity.Basic,
    Target = TargetType.AnyEnemy,
})
{
    /// <inheritdoc/>
    protected override HashSet<CardTag> CanonicalTags =>
    [
        CardTag.Strike
    ];

    /// <inheritdoc/>
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(6M, ValueProp.Move)
    ];

    /// <inheritdoc/>
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await DamageCmd.Attack(this.DynamicVars.Damage.BaseValue).FromCard(this)
            .Targeting(cardPlay.Target).WithHitFx("vfx/vfx_attack_slash").Execute(choiceContext);
    }

    /// <inheritdoc/>
    protected override void OnUpgrade() => this.DynamicVars.Damage.UpgradeValueBy(3M);
}