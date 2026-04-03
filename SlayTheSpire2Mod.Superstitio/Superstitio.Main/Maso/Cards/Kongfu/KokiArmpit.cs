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

namespace Superstitio.Main.Maso.Cards.Kongfu;

/// <summary>
/// 造成伤害。打出一张本牌的复制品。
/// </summary>
public class KokiArmpit() : MasoBaseCard(new CardInitMessage
{
    BaseCost = 0,
    Type = CardType.Attack,
    Rarity = CardRarity.Common,
    Target = TargetType.AnyEnemy,
}), IWithHangingConfigCard
{

    /// <inheritdoc />
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
    [
        new DamageVar(7, ValueProp.Move).WithUpgrade(2),
        new TriggerCountVar(2)
    ];

    /// <inheritdoc />
    public HangingCardConfig HangingCardConfig => new(
        Card: this,
        HangingType: HangingType.Delay,
        CardTypeFilter: CardType.Attack
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

            await CardCmd.AutoPlay(context, clonedCard, play.Target);
        });

        await HangingCardManager.HangCard(token, this);
    }
}