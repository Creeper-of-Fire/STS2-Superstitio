using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Superstitio.Api.BaseLib.HangingCard;
using Superstitio.Api.Card;
using Superstitio.Api.Extensions;
using Superstitio.Api.HangingCard;
using Superstitio.Api.HangingCard.UI;
using Superstitio.Main.Maso.Base;

namespace Superstitio.Main.Maso.Cards.CotiKoki;

/**
 * Title = "性交-侍奉：手交"
 *
 * Description = """
 * 对敌人造成{Damage:diff()}点伤害，连续攻击{Repeat:diff()}次。
 * {CardHangingDescription}
 * """
 *
 * HangingEffect = "获得{StrengthPower:diff()}层力量"
 *
 * Flavor = "用手就能做好的事情却要用其他的部位。该说是你们需求太高了，还是我的两只手应付不过来了？"
 *
 * Sfw.Title = "升龙拳"
 *
 * Sfw.Flavor = "前、下、前下，拳击！"
 */
public class KokiHand() : MasoBaseCard(new CardInitMessage
{
    BaseCost = 1,
    Type = CardType.Attack,
    Rarity = CardRarity.Uncommon,
    Target = TargetType.AnyEnemy,
}), IWithHangingConfigCard
{
    /// <inheritdoc />
    public override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
    [
        new DamageVar(3, ValueProp.Move).WithUpgrade(1),
        new TriggerCountVar(2).WithUpgrade(1),
        new RepeatVar(2),
        new PowerVar<StrengthPower>(1).AddToolTips() // 获得
    ];

    /// <inheritdoc />
    public HangingCardConfig HangingCardConfig => new(
        Card: this,
        HangingType: HangingType.Follow,
        CardTypeFilter: CardType.Attack,
        CardVisualEffect: new CardVisualEffect
        {
            HangGlowType = HangGlowType.Good,
            TargetType = TargetType.Self
        }
    );

    /// <inheritdoc />
    public bool HangingSelfAfterPlay => true;

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.AutoAttack(this, cardPlay, hitCount: this.DynamicVars.Repeat.IntValue).Execute(choiceContext);

        var token = this.CreateHangingToken(async (_, _) =>
        {
            await PowerCmd.ApplyByCard<StrengthPower>(this, this.Owner.Creature);
        });

        await HangingCardManager.HangCard(token, this);
    }
}