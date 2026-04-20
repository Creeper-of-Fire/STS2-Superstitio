using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Superstitio.Api.Card;
using Superstitio.Main.Features.Corruptus;
using Superstitio.Main.Lupa.Base;

namespace Superstitio.Main.Lupa.Cards.General;

/**
 * Title = "禅意"
 *
 * Description = """
 * 移除{Block:diff()}点[sine][red]腐朽[/red][/sine]。
 * 为{Cards:diff()}张[gold]手牌[/gold]添加[gold]消耗[/gold]。
 * 若选择的卡牌不能被打出，则将其[gold]消耗[/gold]。
 * """
 *
 * SelectionScreenPrompt = "选择一张牌添加[gold]消耗[/gold]。"
 *
 * Flavor = "保持不动……深呼吸……要维持这个姿势被干。"
 *
 * Sfw.Title = "禅意"
 *
 * Sfw.Flavor = "禅意境界，看破红尘。"
 */
public class ZenState() : LupaBaseCard(new CardInitMessage
{
    BaseCost = 1,
    Type = CardType.Skill,
    Rarity = CardRarity.Common,
    Target = TargetType.Self
})
{
    private const int CorruptusRemove = 5;
    private const int CorruptusRemoveUpgrade = 3;
    private const int SelectCount = 1;

    /// <inheritdoc />
    public override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
    [
        new BlockVar(CorruptusRemove, ValueProp.Move).WithUpgrade(CorruptusRemoveUpgrade)
            .AddToolTips<CorruptusPower>(),
        new CardsVar(SelectCount)
            .AddToolTips(HoverTipFactory.FromKeyword(CardKeyword.Exhaust)),
    ];

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 移除B点腐朽
        await CorruptusManager.DecreaseCorruptus(this.Owner.Creature, this.DynamicVars.Block.BaseValue, this.Owner.Creature, this);

        // 从手牌中选择M张不具有消耗的牌赋予消耗
        var cardModels = await CardSelectCmd.FromHand(
            prefs: new CardSelectorPrefs(this.SelectionScreenPrompt, this.DynamicVars.Cards.IntValue),
            context: choiceContext,
            player: this.Owner,
            filter: c => !c.Keywords.Contains(CardKeyword.Exhaust),
            source: this);

        foreach (var card in cardModels)
        {
            CardCmd.ApplyKeyword(card, CardKeyword.Exhaust);
            if (card.Keywords.Contains(CardKeyword.Unplayable))
                await CardCmd.Exhaust(choiceContext, card);
        }
    }
}