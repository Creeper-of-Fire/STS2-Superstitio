using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Superstitio.Main.Base;
using Superstitio.Main.DynamicVars;
using Superstitio.Main.Extensions;
using Superstitio.Main.Features.HangingCard;
using Superstitio.Main.Features.HangingCard.UI;
using Superstitio.Main.Maso.Base;

namespace Superstitio.Main.Maso.Cards.CotiKoki;

/**
 * Title = "性交-侍奉：足交"
 *
 * Description = """
 * 造成{Damage:diff()}点伤害。
 * {CardHangingDescription}
 * """
 *
 * HangingEffect = "抽{DrawCards:diff()}张牌"
 *
 * Flavor = "最好是射鞋里，个人来说还是比较喜欢黏糊糊地走路。"
 *
 * Sfw.Title = "回旋踢"
 *
 * Sfw.Flavor = """
 * [purple]造成15点伤害。
 * 抽2张牌。[/purple]
 * """
 */
public sealed class KokiFoot() : MasoBaseCard(new CardInitMessage
{
    BaseCost = 1,
    Type = CardType.Attack,
    Rarity = CardRarity.Common,
    Target = TargetType.AnyEnemy,
}), IWithHangingConfigCard
{
    /// <summary>
    /// 基础触发次数
    /// </summary>
    private const int TriggerCount = 2;

    /// <summary>
    /// 升级增加的触发次数
    /// </summary>
    private const int TriggerCountUpgrade = 1;

    private const int Damage = 5;

    private const int DamageUpgrade = 3;

    /// <inheritdoc />
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
    [
        new DamageVar(Damage, ValueProp.Move).WithUpgrade(DamageUpgrade),
        new TriggerCountVar(TriggerCount).WithUpgrade(TriggerCountUpgrade),
        new DrawCardsVar(1)
    ];

    /// <inheritdoc />
    public HangingCardConfig HangingCardConfig => new(
        Card: this,
        HangingType: HangingType.Follow,
        CardTypeFilter: CardType.Attack,
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

        var token = this.CreateHangingToken(async (context, _) =>
            {
                await CardPileCmd.AutoDraw(context, this);
            }
        );
        await HangingCardManager.HangCard(token, this);
    }
}