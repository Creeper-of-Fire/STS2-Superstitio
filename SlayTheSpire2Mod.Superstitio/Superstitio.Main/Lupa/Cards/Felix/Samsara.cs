using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using Superstitio.Main.Base;
using Superstitio.Api.Card;
using Superstitio.Api.Extensions;
using Superstitio.Api.Power;
using Superstitio.Main.Features.PowerCardPower;
using Superstitio.Main.Lupa.Base;

namespace Superstitio.Main.Lupa.Cards.Felix;

/**
 * Title = "轮转"
 *
 * Description = """
 * 在[gold]抽牌堆[/gold]中添加{Cards:diff()}张[gold]伤口[/gold]。
 * 你在回合结束时不再自动丢弃所有[gold]手牌[/gold]。
 * 每当你打出卡牌，抽{SamsaraPower:diff()}张牌。
 * """
 *
 * Flavor = "一切都只是轮回的一部分，何不放肆一点？"
 *
 * Power.Description = """
 * 你在回合结束时不再自动丢弃所有[gold]手牌[/gold]。
 * 每当你打出卡牌，抽牌。
 * """
 *
 * Power.SmartDescription = """
 * 你在回合结束时不再自动丢弃所有[gold]手牌[/gold]。
 * 每当你打出卡牌，抽[blue]{Amount}[/blue]张牌。
 * """
 *
 * Sfw.Title = "轮转"
 *
 * Sfw.Flavor = "生死轮转，无穷无尽。"
 */
public class Samsara() : LupaBaseCard(new CardInitMessage
{
    BaseCost = 3,
    Type = CardType.Power,
    Rarity = CardRarity.Rare,
    Target = TargetType.Self
})
{
    private const int Wounds = 8;
    private const int WoundsUpgrade = -2;
    private const int DrawWhenUse = 1;

    /// <inheritdoc />
    public override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
    [
        new CardsVar(Wounds).WithUpgrade(WoundsUpgrade).AddToolTips(HoverTipFactory.FromCard<Wound>()),
        new PowerVar<SamsaraPower>(DrawWhenUse)
    ];

    /// <inheritdoc />
    public sealed class SamsaraPower() : SimpleCardPower<Samsara>(new PowerInitMessage
    {
        Type = PowerType.Buff,
        InitStackType = PowerInitMessage.StackStyle.Normal
    })
    {
        /// <inheritdoc />
        public override bool ShouldFlush(Player player)
        {
            if (player != this.Owner.Player)
                return true;

            return false;
        }

        /// <inheritdoc />
        public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
        {
            if (cardPlay.CardPlayer != this.Owner.Player) return;

            await CardPileCmd.Draw(context, this.Amount, cardPlay.CardPlayer);
        }
    }

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 在抽牌堆中添加伤口
        await CardPileCmd.AddToCombatAndPreview<Wound>(
            target: cardPlay.GetTargetOrThrow(),
            pileType: PileType.Draw,
            count: this.DynamicVars.Cards.IntValue,
            addedByPlayer: false
        );

        await PowerCmd.ApplyByCard<SamsaraPower>(this, this.Owner.Creature);
    }
}