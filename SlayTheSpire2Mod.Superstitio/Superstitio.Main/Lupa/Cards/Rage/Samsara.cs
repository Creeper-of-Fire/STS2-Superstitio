using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using Superstitio.Main.Base;
using Superstitio.Main.Extensions;
using Superstitio.Main.Features.PowerCardPower;
using Superstitio.Main.Lupa.Base;

namespace Superstitio.Main.Lupa.Cards.Rage;

/// <summary>
/// 轮转 - 在[gold]抽牌堆[/gold]中添加{Cards:diff()}张[gold]伤口[/gold]。\n回合结束时，保留你所有的手牌。\n每当你打出卡牌，抽[blue]{SamsaraPower:diff()}[/blue]张牌。
/// </summary>
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
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
    [
        new CardsVar(Wounds).WithUpgrade(WoundsUpgrade).AddToolTips(HoverTipFactory.FromCard<Wound>()),
        new PowerVar<SamsaraPower>(DrawWhenUse)
    ];

    /// <inheritdoc />
    public sealed class SamsaraPower : PowerCardPower<Samsara>
    {
        /// <inheritdoc />
        public override PowerStackType StackType => PowerStackType.Counter;

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

            await CardPileCmd.Draw(context,this.Amount, cardPlay.CardPlayer);
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
        
        await PowerCmd.ApplyByCard<SamsaraPower>(this, this.Owner);
    }
}