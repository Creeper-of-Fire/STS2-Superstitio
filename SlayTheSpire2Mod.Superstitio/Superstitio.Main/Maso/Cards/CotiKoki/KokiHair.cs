using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Superstitio.Main.Base;
using Superstitio.Main.DynamicVars;
using Superstitio.Main.Extensions;
using Superstitio.Main.Features.HangingCard;
using Superstitio.Main.Maso.Base;

namespace Superstitio.Main.Maso.Cards.CotiKoki;

/**
 * 虚无。Cost 1 造成 5-7 点 *伤害 。 NL superstitiomod:延缓 打出任意牌 3 次： NL 把 带有 NL 消耗 的 复制加入手牌， NL 其在被打出之前，耗能变为0 。 NL superstitiomod:自动释放 。
 * 在新版本中，自动释放被取消，导致这张卡可能过于OP，也许加上一个虚无效果？但是之前就有虚无效果。
 */
public class KokiHair() : MasoBaseCard(new CardInitMessage
{
    BaseCost = 1,
    Type = CardType.Attack,
    Rarity = CardRarity.Uncommon,
    Target = TargetType.AnyEnemy,
}), IWithHangingConfigCard
{
    /// <inheritdoc />
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Ethereal];

    /// <inheritdoc />
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
    [
        new DamageVar(5, ValueProp.Move).WithUpgrade(2),
        new TriggerCountVar(3)
    ];

    /// <inheritdoc />
    public HangingCardConfig HangingCardConfig => new(this, HangingType.Delay, CardType.None);

    /// <inheritdoc />
    public bool HangingSelfAfterPlay => true;

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.AutoAttack(this, cardPlay).Execute(choiceContext);

        var token = this.CreateHangingToken(async (_, _) =>
        {
            var copy = this.CreateClone();
            CardCmd.ApplyKeyword(copy, CardKeyword.Exhaust);
            copy.EnergyCost.SetUntilPlayed(0);
            await CardPileCmd.AddGeneratedCardToCombat(copy, PileType.Hand, addedByPlayer: true);
        });

        await HangingCardManager.HangCard(token, this);
    }
}