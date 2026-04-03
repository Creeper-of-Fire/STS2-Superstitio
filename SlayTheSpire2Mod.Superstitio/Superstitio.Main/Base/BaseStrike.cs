using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Superstitio.Main.Base;

/// <summary>
/// 基础牌 - 打击
/// </summary>
public abstract class BaseStrike() : SuperstitioBaseCard(new CardInitMessage
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
    protected override IEnumerable<DynamicVarWithUpgrade> InitVarsWithUpgrade =>
    [
        new DamageVar(6, ValueProp.Move).WithUpgrade(3)
    ];

    /// <inheritdoc/>
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await DamageCmd.Attack(this.DynamicVars.Damage.BaseValue).FromCard(this)
            .Targeting(cardPlay.Target).WithHitFx("vfx/vfx_attack_slash").Execute(choiceContext);
    }
}