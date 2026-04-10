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
 * Title = "辫子神功"
 *
 * Description = """
 * 对敌人造成{Damage:diff()}点伤害。
 * {CardHangingDescription}
 * """
 *
 * HangingEffect = "将带有消耗的本牌复制加入手牌，其在被打出之前耗能变为0"
 *
 * Flavor = "鞭剪了，神留着，祖宗的东西再好，该割的时候就得割！再怎么变，也难不死咱。什么新玩意，都能玩到家，一变还得是绝活！"
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