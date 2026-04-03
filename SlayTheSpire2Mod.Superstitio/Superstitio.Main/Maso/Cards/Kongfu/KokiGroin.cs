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

/**
 * TODO 旧效果：Cost 2 造成 18-24 点 *伤害 ， NL 根据手牌平均耗能， NL 减少 8 伤害每 [E] 。 NL （平均耗能不取整）
 * 目前打算换一个效果
 */
/// <summary>
/// 打出后挂起自身，后续每次打出牌时触发一次额外攻击（可触发3次）
/// </summary>
public sealed class KokiGroin() : MasoBaseCard(new CardInitMessage
{
    BaseCost = 2,
    Type = CardType.Attack,
    Rarity = CardRarity.Common,
    Target = TargetType.AnyEnemy,
}), IWithHangingConfigCard
{
    /// <inheritdoc />
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
    [
        new DamageVar(6, ValueProp.Move).WithUpgrade(3),
        new TriggerCountVar(3)
    ];

    /// <inheritdoc />
    public HangingCardConfig HangingCardConfig => new(
        Card: this,
        HangingType: HangingType.Follow,
        CardTypeFilter: CardType.None
    );

    /// <inheritdoc />
    public bool HangingSelfAfterPlay => true;

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 执行本次攻击
        await DamageCmd.AutoAttack(this, cardPlay).Execute(choiceContext);

        // 创建挂起令牌
        var token = this.CreateHangingToken(async (context, play) =>
        {
            // 触发一次攻击到原目标
            await DamageCmd.AutoAttack(this, play, tryRandomWhenTargetDie: true).Execute(context);
        });

        await HangingCardManager.HangCard(token, this);
    }
}