using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using Superstitio.Api.BaseLib.HangingCard;
using Superstitio.Api.Card;
using Superstitio.Api.DynamicVars;
using Superstitio.Api.Extensions;
using Superstitio.Api.HangingCard;
using Superstitio.Api.HangingCard.UI;
using Superstitio.Main.DynamicVars;
using Superstitio.Main.DynamicVars.Extensions;
using Superstitio.Main.Features.Felix;
using Superstitio.Main.Lupa.Base;

namespace Superstitio.Main.Lupa.Cards.Base;

/**
 * Title = "自慰"
 *
 * Description = """
 * 获得{Felix:diff()}点[pink]快感[/pink]。
 * {CardHangingDescription}
 * """
 *
 * HangingEffect = "抽{DrawCards:diff()}张牌"
 *
 * Flavor = "何以解忧？聊以自慰。"
 *
 * Sfw.Title = "蓄势待发"
 *
 * Sfw.Description= """
 * 获得{Felix:diff()}点[pink]狂热[/pink]。
 * {CardHangingDescription}
 * """
 *
 * Sfw.Flavor = "蓄势待发，只待一击。"
 */
public class Masturbate() : LupaBaseCard(new CardInitMessage
{
    BaseCost = 0,
    Type = CardType.Skill,
    Rarity = CardRarity.Basic,
    Target = TargetType.Self,
}), IWithHangingConfigCard
{
    /// <summary>
    /// 基础快感值
    /// </summary>
    private const int FelixGain = 4;

    /// <summary>
    /// 升级增加的快感值
    /// </summary>
    private const int FelixGainUpgrade = 2;

    /// <summary>
    /// 触发抽牌所需打出的牌数
    /// </summary>
    private const int TriggerCount = 2;

    /// <summary>
    /// 触发抽牌的牌数
    /// </summary>
    private const int DrawCard = 1;

    /// <inheritdoc />
    public override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
    [
        new FelixVar(FelixGain).WithUpgrade(FelixGainUpgrade).AddToolTips<FelixPower>(),
        new TriggerCountVar(TriggerCount),
        new DrawCardsVar(DrawCard)
    ];

    /// <inheritdoc />
    public HangingCardConfig HangingCardConfig => new(
        Card: this,
        HangingType: HangingType.Delay,
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
        // 获取快感值
        decimal felixAmount = this.DynamicVars.Felix.BaseFelix;

        // 使用 FelixManager 增加快感
        await FelixManager.ModifyFelix(this.Owner.Creature, felixAmount, this.Owner.Creature, cardPlay.Card);

        // 创建挂起令牌
        var token = this.CreateHangingToken(async (context, _) =>
        {
            await CardPileCmd.AutoDraw(context, this);
        });

        // 挂起自身
        await HangingCardManager.HangCard(token, this);
    }
}