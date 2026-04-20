using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Superstitio.Api.BaseLib.HangingCard;
using Superstitio.Api.Card;
using Superstitio.Api.Extensions;

namespace Superstitio.Api.BaseLib;

/// <summary>
/// 基础的卡牌类
/// </summary>
/// <param name="cardInitMessage"></param>
public abstract class BaseCard(CardInitMessage cardInitMessage) : CustomCardModel(
    cardInitMessage.BaseCost.InitialCost,
    cardInitMessage.Type,
    cardInitMessage.Rarity,
    cardInitMessage.Target,
    cardInitMessage.ShowInCardLibrary
), ICardWithSuperstitioCost, ICardWithDynamicVarSpecs, ICardWithKeywordSpecs, IShowBaseResultPileType
{
    /// <inheritdoc />
    public CardInitMessage CardInitMessage { get; } = cardInitMessage;

    /// <summary>
    /// 定义卡牌的标准标签集合。
    /// </summary>
    protected override HashSet<CardTag> CanonicalTags => [];

    /// <summary>
    /// 定义卡牌的关键字集合。
    /// </summary>
    public sealed override IEnumerable<CardKeyword> CanonicalKeywords =>
        this.GetCanonicalKeywords();

    /// <summary>
    /// 定义带升级行为的关键字
    /// </summary>
    public virtual IEnumerable<CardKeywordSpec> InitKeywordsWithUpgrade => [];

    /// <summary>
    /// 定义卡牌的动态变量集合。
    /// </summary>
    protected sealed override IEnumerable<DynamicVar> CanonicalVars =>
        this.GetCanonicalVars();

    /// <summary>
    /// 为卡牌定义动态变量集合（带升级描述）。
    /// </summary>
    public virtual IEnumerable<DynamicVarSpec> InitVarsWithUpgrade => [];

    /// <summary>
    /// 卡牌升级效果。
    /// </summary>
    protected override void OnUpgrade()
    {
        CardInitUtils.OnUpgrade(this);
    }

    /// <summary>
    /// 执行卡牌效果。
    /// </summary>
    /// <param name="choiceContext">玩家选择上下文。</param>
    /// <param name="cardPlay">执行相关信息。</param>
    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// 卡牌的风味文本
    /// </summary>
    public LocString Flavor => new("cards", this.Id.Entry + ".flavor");

    /// <inheritdoc />
    protected override void AddExtraArgsToDescription(LocString description)
    {
        base.AddExtraArgsToDescription(description);

        if (this is IWithHangingConfig withHangingConfig)
            HangingDescriptionBuilder.AddExtraArgsToDescription(description, withHangingConfig.HangingCardConfig);
    }

    /// <inheritdoc />
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips
        .TryAddTip(this.Flavor)
        .TryAddTip(this.InitVarsWithUpgrade.SelectMany(it => it.ExtraHoverTips))
        .TryAddTip(this is IWithHangingConfig withHangingConfig
            ? HangingDescriptionBuilder.GetHoverTips(withHangingConfig.HangingCardConfig, showHangingTotalDescription: true)
            : []);

    /// <inheritdoc />
    protected override PileType GetResultPileType()
    {
        if (this is IWithHangingConfigCard { HangingSelfAfterPlay: true })
            return PileType.None; // 挂起后不进入弃牌堆

        return base.GetResultPileType();
    }

    /// <summary>
    /// 基类打出后返回的牌堆
    /// </summary>
    public PileType BaseResultPileType => base.GetResultPileType();
}