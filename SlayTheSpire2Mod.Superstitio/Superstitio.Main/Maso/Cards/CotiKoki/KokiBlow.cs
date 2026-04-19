using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Superstitio.Main.Base;
using Superstitio.Main.DynamicVars;
using Superstitio.Main.DynamicVars.Extensions;
using Superstitio.Main.Extensions;
using Superstitio.Main.Features.Felix;
using Superstitio.Main.Features.HangingCard;
using Superstitio.Main.Features.HangingCard.UI;
using Superstitio.Main.Maso.Base;

namespace Superstitio.Main.Maso.Cards.CotiKoki;

/**
 * Title = "性交-侍奉：口交"
 *
 * Description = """
 * 造成{Damage:diff()}点伤害。
 * {CardHangingDescription}
 * """
 *
 * HangingEffect = "获得{Felix:diff()}点[pink]快感[/pink]"
 *
 * Flavor = "听说有一些天人会把自己的牙齿都拔掉，就为了给别人口交时方便。有点可怕。"
 *
 * Sfw.Title = "铁头撞击"
 *
 * Sfw.Flavor = "用钢铁般坚硬的头部进行攻击。有时会使对手畏缩。"
 */
public sealed class KokiBlow() : MasoBaseCard(new CardInitMessage
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
        new DamageVar(5, ValueProp.Move).WithUpgrade(3),
        new TriggerCountVar(3).WithUpgrade(1),
        new FelixVar(2).AddToolTips() // 获得
    ];

    /// <inheritdoc />
    public HangingCardConfig HangingCardConfig => new(
        Card: this,
        HangingType: HangingType.Follow,
        CardTypeFilter: CardType.None,
        CardVisualEffect: new CardVisualEffect
        {
            HangGlowType = HangGlowType.Good,
            TargetType = TargetType.Self,
        }
    );

    /// <inheritdoc />
    public bool HangingSelfAfterPlay => true;

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.AutoAttack(this, cardPlay).Execute(choiceContext);

        var token = this.CreateHangingToken(async (_, _) =>
        {
            await FelixManager.ModifyFelix(this.Owner.Creature, this.DynamicVars.Felix.BaseFelix, this.Owner.Creature, this);
        });

        await HangingCardManager.HangCard(token, this);
    }
}