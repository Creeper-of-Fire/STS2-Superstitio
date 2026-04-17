using BaseLib.Extensions;
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
 * Title = "性交-侍奉：素股"
 *
 * Description = """
 * 造成{Damage:diff()}点伤害。
 * {CardHangingDescription}
 * """
 *
 * HangingEffect = "造成{Damage:diff()}点伤害"
 *
 * Flavor = "看上去就像是被十个人侵犯到溢出来了，实际上只是射在外面啦。"
 *
 * Sfw.Title = "震波震震"
 *
 * Sfw.Flavor = "弹弹弹弹。"
 */
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