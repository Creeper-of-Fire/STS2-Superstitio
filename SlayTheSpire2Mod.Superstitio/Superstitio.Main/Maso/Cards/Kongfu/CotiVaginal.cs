using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Superstitio.Main.Base;
using Superstitio.Main.DynamicVars;
using Superstitio.Main.Features.HangingCard;
using Superstitio.Main.Maso.Base;

namespace Superstitio.Main.Maso.Cards.Kongfu;


/// <summary>
/// 造成伤害，触发 <see cref="HangingType.Follow"/> 的挂载卡
/// </summary>
public class CotiVaginal() : MasoBaseCard(new CardInitMessage
{
    BaseCost = 1,
    Type = CardType.Attack,
    Rarity = CardRarity.Uncommon,
    Target = TargetType.AnyEnemy,
})
{
    /// <inheritdoc/>
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
    [
        new DamageVar(8, ValueProp.Move).WithUpgrade(4)
    ];

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var configs = HangingCardManager.GetHangingCardTokens<AutoHangingCardTokenWithConfig>(this.Owner)
            .Where(it => it.HangingCardConfig.HangingType == HangingType.Follow);

        foreach (var config in configs)
        {
            await config.AfterCardPlayed(choiceContext, cardPlay);
        }
    }
}