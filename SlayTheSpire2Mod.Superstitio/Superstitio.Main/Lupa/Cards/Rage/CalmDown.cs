using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using Superstitio.Main.Base;
using Superstitio.Main.Features.HangingCard;
using Superstitio.Main.Lupa.Base;

namespace Superstitio.Main.Lupa.Cards.Rage;

/**
 * Title = "冷静"
 *
 * Description = """
 * 抽当前[orange]合力序列[/orange]总数的牌。
 * 释放所有[orange]合力序列[/orange]中的卡牌。
 * """
 *
 * Flavor = """
 * 冥想……冷静，冷静，冥想……果然还是好想自慰。
 * o(TヘTo)
 * """
 *
 * Sfw.Title = "冷静"
 *
 * Sfw.Flavor = "冷静下来，重新思考。"
 */
public class CalmDown() : LupaBaseCard(new CardInitMessage
{
    BaseCost = 1,
    Type = CardType.Skill,
    Rarity = CardRarity.Common,
    Target = TargetType.Self
})
{
    /// <inheritdoc />
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade => [];

    /// <inheritdoc />
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        ..base.ExtraHoverTips,
        HangingStaticLocs.SequenceHoverTip,
    ];

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var cardTokens = HangingCardManager.GetHangingCardTokens<AutoHangingCardTokenWithConfig>(this.Owner).ToList();
        int drawCount = cardTokens.Count;
        foreach (var cardToken in cardTokens)
        {
            await cardToken.UnHangCard();
        }

        await CardPileCmd.Draw(choiceContext, drawCount, this.Owner);
    }
}