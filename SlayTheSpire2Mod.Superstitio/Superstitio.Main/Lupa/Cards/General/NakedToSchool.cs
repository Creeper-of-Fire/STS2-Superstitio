using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Superstitio.Api.Card;
using Superstitio.Main.Lupa.Base;

namespace Superstitio.Main.Lupa.Cards.General;

/**
 * Title = "全裸上学日"
 *
 * Description = """
 * 从弃牌堆中选择最多{CardsVar:diff()}张牌，将它们升级并放入抽牌堆。
 * """
 *
 * Flavor = ""
 */
public class NakedToSchool() : LupaBaseCard(new CardInitMessage
{
    BaseCost = 1,
    Type = CardType.Skill,
    Rarity = CardRarity.Uncommon,
    Target = TargetType.Self
})
{
    /// <inheritdoc />
    public override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
    [
        new CardsVar(3).WithUpgrade(1)
    ];

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var prefs = new CardSelectorPrefs(this.SelectionScreenPrompt, this.DynamicVars.Cards.IntValue);
        var cardsIn = PileType.Discard.GetPile(this.Owner).Cards.ToList();
        var cards = await CardSelectCmd.FromSimpleGrid(choiceContext, cardsIn, this.Owner, prefs);
        foreach (var card in cards)
        {
            CardCmd.Upgrade(card);
            CardCmd.Preview(card);
            await CardPileCmd.Add(card, PileType.Draw);
        }
    }
}