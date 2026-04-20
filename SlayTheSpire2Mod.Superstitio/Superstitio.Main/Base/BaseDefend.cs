using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Superstitio.Api.Card;
using Superstitio.Main.Resource;

namespace Superstitio.Main.Base;

/// <summary>
/// 基础牌 - 防御
/// </summary>
[CustomImgName("Invite")]
public abstract class BaseDefend() : SuperstitioBaseCard(new CardInitMessage
{
    BaseCost = 1,
    Type = CardType.Skill,
    Rarity = CardRarity.Basic,
    Target = TargetType.Self,
})
{
    /// <inheritdoc/>
    protected override HashSet<CardTag> CanonicalTags =>
    [
        CardTag.Defend
    ];

    /// <inheritdoc/>
    public override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
    [
        new BlockVar(5, ValueProp.Move).WithUpgrade(3)
    ];

    /// <inheritdoc/>
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(this.Owner.Creature, this.DynamicVars.Block, cardPlay);
    }
}