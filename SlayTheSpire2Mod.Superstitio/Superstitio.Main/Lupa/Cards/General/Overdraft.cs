using BaseLib.Cards.Variables;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Superstitio.Main.Base;
using Superstitio.Main.DynamicVars;
using Superstitio.Main.Extensions;
using Superstitio.Main.Features.PowerCardPower;
using Superstitio.Main.Lupa.Base;

namespace Superstitio.Main.Lupa.Cards.General;

/**
 * Title = "透支"
 *
 * Description = """
 * 抽{DrawCards:diff()}张牌。
 * 下回合开始时，[gold]消耗[/gold]你[gold]手牌[/gold]中的{OverdraftPower:diff()}张牌。
 * """
 *
 * Power.Description = "下回合开始时，消耗你手牌中的牌。"
 *
 * Power.DescriptionSmart = "下回合开始时，消耗你手牌中的[b]{Amount}[/b]张牌。"
 *
 * Flavor = ""
 */
public class Overdraft() : LupaBaseCard(new CardInitMessage
{
    BaseCost = 1,
    Type = CardType.Skill,
    Rarity = CardRarity.Uncommon,
    Target = TargetType.Self
})
{
    /// <inheritdoc />
    public class OverdraftPower() : SimpleCardPower<Overdraft>(new PowerInitMessage
    {
        InitStackType = PowerInitMessage.StackStyle.Normal,
        Type = PowerType.Debuff
    })
    {
        /// <inheritdoc />
        public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
        {
            if (player != this.Owner.Player)
                return;
            var prefs = new CardSelectorPrefs(CardSelectorPrefs.ExhaustSelectionPrompt, this.Amount);
            var list = (await CardSelectCmd.FromHand(choiceContext, player, prefs, null, this)).ToList();
            foreach (var item in list)
            {
                await CardCmd.Exhaust(choiceContext, item);
            }
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
    [
        new DrawCardsVar(3).WithUpgrade(1),
        new PowerVar<OverdraftPower>(1)
    ];

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CardPileCmd.AutoDraw(choiceContext, this);
        await PowerCmd.ApplyByCard<OverdraftPower>(this, this.Owner.Creature);
    }
}