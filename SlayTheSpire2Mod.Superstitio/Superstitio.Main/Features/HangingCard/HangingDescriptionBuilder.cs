using System.Diagnostics.CodeAnalysis;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using Superstitio.Main.DynamicVars;
using static Superstitio.Main.Utils.SuperstitioLocStringFactory;

namespace Superstitio.Main.Features.HangingCard;

/// <summary>
/// 挂起卡牌配置
/// </summary>
[method: SetsRequiredMembers]
public record HangingCardConfig(CardModel Card, HangingType HangingType, HangingTriggerVar TriggerCount, CardType CardTypeFilter)
{
    /// <inheritdoc cref="HangingCardConfig"/>
    [SetsRequiredMembers]
    public HangingCardConfig(CardModel Card, HangingType HangingType, int TriggerCount, CardType CardTypeFilter) : this(
        Card: Card,
        HangingType: HangingType,
        TriggerCount: new HangingTriggerVar(triggers: TriggerCount),
        CardTypeFilter: CardTypeFilter
    ) { }

    /// <summary>
    /// 挂起的卡牌
    /// </summary>
    public required CardModel Card { get; init; } = Card;

    /// <summary>
    /// 挂起类型
    /// </summary>
    public required HangingType HangingType { get; init; } = HangingType;

    /// <summary>
    /// 触发次数（动态变量值）
    /// </summary>
    public required HangingTriggerVar TriggerCount { get; init; } = TriggerCount;

    /// <summary>
    /// 触发的卡牌类型过滤（ <see cref="CardType.None"/> 为任意牌）
    /// </summary>
    public required CardType CardTypeFilter { get; init; } = CardTypeFilter;
}

/// <summary>
/// 挂起类型
/// </summary>
public enum HangingType
{
    /// <summary>
    /// 伴随：每次触发时生效，直到次数耗尽
    /// </summary>
    Follow,

    /// <summary>
    /// 延缓：累积触发次数后生效一次
    /// </summary>
    Delay
}

/// <summary>
/// 挂起描述构建器
/// 用于统一处理挂起的本地化描述
/// </summary>
public static class HangingDescriptionBuilder
{
    /// <summary>
    /// 为本地化键添加挂起描述
    /// </summary>
    /// <param name="baseDescription"></param>
    /// <param name="config"></param>
    /// <returns></returns>
    public static LocString AddExtraArgsToDescription(LocString baseDescription, HangingCardConfig config)
    {
        var (hangingKeyword, hangingDescription) = BuildHangingDescription(config);

        var cardHangingDescription = GetHangingCardDescriptionFrame();
        cardHangingDescription.Add("HangingKeyword", hangingKeyword);
        cardHangingDescription.Add("HangingDescription", hangingDescription);

        baseDescription.Add("CardHangingDescription", cardHangingDescription);
        return baseDescription;
    }

    /// <summary>
    /// 构建挂起卡牌的挂起效果描述。
    /// </summary>
    /// <returns>完整的本地化描述 LocString</returns>
    public static (LocString HangingKeyword, LocString HangingDescription) BuildHangingDescription(HangingCardConfig config)
    {
        // 获取挂起类型的本地化键名
        string typeKey = config.HangingType switch
        {
            HangingType.Follow => "Follow",
            HangingType.Delay => "Delay",
            _ => "Delay"
        };

        // 创建基础 LocString
        var description = KeywordLocString("Hanging", typeKey, "description");
        var keyword = KeywordLocString("Hanging", typeKey, "keyword");

        // 添加动态变量
        description.Add(config.TriggerCount);

        // 添加卡牌类型（根据过滤条件决定显示什么）
        description.Add("CardType", GetCardTypeText(config.CardTypeFilter));

        // 添加序列名称
        description.Add("SequenceName", GetSequenceLocStrings().Name);

        // 添加挂起效果描述
        description.Add("HangingEffect", GetCardHangingEffect(config.Card));

        return (HangingKeyword: keyword, HangingDescription: description);
    }

    /// <summary>
    /// 获取挂起效果的悬停提示列表。
    /// </summary>
    /// <returns>挂起相关的所有悬停提示集合</returns>
    public static IEnumerable<IHoverTip> GetHoverTips(HangingCardConfig config, bool showHangingTotalDescription = false)
    {
        var sequenceLocStrings = GetSequenceLocStrings();
        var (hangingKeyword, hangingDescription) = BuildHangingDescription(config);
        var hangingGeneralDescription = GetHangingGeneralDescription();
        var hangingKeywordTitle = GetHangingKeywordTitle();
        hangingKeywordTitle.Add("HangingKeyword", hangingKeyword);

        if (showHangingTotalDescription)
        {
            yield return new HoverTip(hangingKeyword, hangingGeneralDescription);
            yield return new HoverTip(sequenceLocStrings.Name, sequenceLocStrings.Description);
        }

        yield return new HoverTip(hangingKeywordTitle, hangingDescription);
    }

    private static LocString GetHangingKeywordTitle()
    {
        return KeywordLocString("Hanging", "keyword_title");
    }

    private static LocString GetCardHangingEffect(CardModel card)
    {
        return new LocString("cards", $"{card.Id.Entry}.hanging_effect");
    }

    private static LocString GetHangingGeneralDescription()
    {
        var totalDescription = KeywordLocString("Hanging", "general_description");
        totalDescription.Add("SequenceName", GetSequenceLocStrings().Name);
        return totalDescription;
    }

    private static LocString GetHangingCardDescriptionFrame()
    {
        return KeywordLocString("Hanging", "card_description_frame");
    }

    /// <summary>
    /// 获取卡牌类型的本地化文本
    /// </summary>
    private static LocString GetCardTypeText(CardType cardType)
    {
        return cardType switch
        {
            CardType.Attack => KeywordLocString("CardType", "attack"),
            CardType.Skill => KeywordLocString("CardType", "skill"),
            CardType.Power => KeywordLocString("CardType", "power"),
            CardType.Status => KeywordLocString("CardType", "status"),
            CardType.Curse => KeywordLocString("CardType", "curse"),
            CardType.Quest => KeywordLocString("CardType", "quest"),
            CardType.None => KeywordLocString("CardType", "any"),
            _ => throw new ArgumentOutOfRangeException(nameof(cardType), cardType, null)
        };
    }


    /// <summary>
    /// 获取序列名称的本地化文本
    /// </summary>
    private static (LocString Name, LocString Description) GetSequenceLocStrings()
    {
        return (Name: KeywordLocString("Hanging", "Sequence", "name"), Description: KeywordLocString("Hanging", "Sequence", "description"));
    }
}