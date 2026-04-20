using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Superstitio.Api.BaseLib.HangingCard;
using Superstitio.Api.Card;
using Superstitio.Api.Extensions;
using Superstitio.Api.HangingCard;
using Superstitio.Api.HangingCard.UI;
using Superstitio.Main.DynamicVars;
using Superstitio.Main.Maso.Base;

namespace Superstitio.Main.Maso.Cards.CotiKoki;

/**
 * Title = "性交-侍奉：腋交"
 *
 * Description = "{CardHangingDescription}"
 *
 * HangingEffect = "造成{Damage:diff()}点伤害。打出一张本牌的复制品"
 *
 * Flavor = "很想试试看被打火机一点点烧掉腋毛的感觉，可惜本小姐没有这种东西。"
 *
 * Sfw.Title = "肘击"
 *
 * Sfw.Flavor = "曼巴永不退场。"
 */
public class KokiArmpit() : MasoBaseCard(new CardInitMessage
{
    BaseCost = 0,
    Type = CardType.Attack,
    Rarity = CardRarity.Common,
    Target = TargetType.AnyEnemy,
}), IWithHangingConfigCard
{
    /// <inheritdoc />
    public override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
    [
        new DamageVar(7, ValueProp.Move).WithUpgrade(2),
        new TriggerCountVar(2)
    ];

    /// <inheritdoc />
    public HangingCardConfig HangingCardConfig => new(
        Card: this,
        HangingType: HangingType.Delay,
        CardTypeFilter: CardType.Attack,
        CardVisualEffect: new CardVisualEffect
        {
            HangGlowType = HangGlowType.Bad,
            TargetType = TargetType.AnyEnemy,
        }
    );

    /// <inheritdoc />
    public bool HangingSelfAfterPlay => true;

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var token = this.CreateHangingToken(async (context, play) =>
        {
            // 触发一次攻击到原目标或随机目标
            await DamageCmd.AutoAttack(this, play, tryRandomWhenTargetDie: true).Execute(context);

            var clonedCard = this.CreateClone();

            await CardPileCmd.AddGeneratedCardToCombat(clonedCard, PileType.Play, addedByPlayer: true);

            await CardCmd.AutoPlay(context, clonedCard, play.Target);
        });

        await HangingCardManager.HangCard(token, this);
    }
}