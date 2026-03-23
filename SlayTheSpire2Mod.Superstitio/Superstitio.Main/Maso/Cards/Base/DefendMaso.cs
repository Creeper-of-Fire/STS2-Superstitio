using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Superstitio.Main.Base;

namespace Superstitio.Main.Maso.Cards.Base;

/// <summary>
/// 防御
/// </summary>

public class DefendMaso() : MasoBaseCard(new CardInitMessage
{
    BaseCost = 1,
    Type = CardType.Skill,
    Rarity = CardRarity.Basic,
    Target = TargetType.Self,
})
{
    /// <summary>
    /// 指示此卡牌是否获得格挡。
    /// </summary>
    public override bool GainsBlock => true;

    /// <inheritdoc/>
    protected override HashSet<CardTag> CanonicalTags =>
    [
        CardTag.Defend
    ];

    /// <inheritdoc/>
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(5M, ValueProp.Move)
    ];

    /// <inheritdoc/>
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(this.Owner.Creature, this.DynamicVars.Block, cardPlay);
    }

    /// <inheritdoc/>
    protected override void OnUpgrade() => this.DynamicVars.Block.UpgradeValueBy(3M);
}