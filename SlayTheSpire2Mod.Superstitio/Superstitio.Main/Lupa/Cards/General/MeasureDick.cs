using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Superstitio.Api.Card;
using Superstitio.Main.Lupa.Base;

namespace Superstitio.Main.Lupa.Cards.General;

/**
 * Title = "肚量"
 *
 * Description = "选择最多{Cards:diff()}张牌丢弃，获得其耗能变化量的{Energy:energyIcons()}，抽等量的牌。"
 *
 * Flavor = "看看谁插的最深？"
 *
 * Sfw.Title = "投技"
 *
 * Sfw.Flavor = "投掷卡牌，获得能量。"
 */
public class MeasureDick() : LupaBaseCard(new CardInitMessage
{
    BaseCost = 0,
    Type = CardType.Skill,
    Rarity = CardRarity.Common,
    Target = TargetType.Self
})
{
    private const int SelectCount = 2;
    private const int SelectCountUpgrade = 1;
    private const int Energy = 1;

    /// <inheritdoc />
    public override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
    [
        new CardsVar(SelectCount).WithUpgrade(SelectCountUpgrade),
        new EnergyVar(Energy).AddToolTips(this.EnergyHoverTip),
    ];

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int cardCount = this.DynamicVars.Cards.IntValue;
        var cards = (await CardSelectCmd.FromHandForDiscard(
            choiceContext,
            this.Owner,
            new CardSelectorPrefs(CardSelectorPrefs.DiscardSelectionPrompt, 0, cardCount),
            null,
            this
        )).ToList();
        await CardCmd.Discard(choiceContext, cards);

        int energyChanged = cards.Where(it => !it.EnergyCost.CostsX && !it.Keywords.Contains(CardKeyword.Unplayable))
            .Select(it => Math.Abs(it.EnergyCost.GetAmountToSpend() - it.EnergyCost.GetWithModifiers(CostModifiers.None))).Sum();

        await PlayerCmd.GainEnergy(this.DynamicVars.Energy.BaseValue * energyChanged, this.Owner);

        await CardPileCmd.Draw(choiceContext, cards.Count, this.Owner);
    }
}