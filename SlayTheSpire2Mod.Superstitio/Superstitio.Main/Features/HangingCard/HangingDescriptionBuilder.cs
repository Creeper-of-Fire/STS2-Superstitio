using System.Diagnostics.CodeAnalysis;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using Superstitio.Main.DynamicVars;
using Superstitio.Main.DynamicVars.Extensions;
using static Superstitio.Main.Features.HangingCard.HangingStaticLocs;
using static Superstitio.Main.Utils.SuperstitioLocStringFactory;

// ReSharper disable InconsistentNaming

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释

namespace Superstitio.Main.Features.HangingCard;

/// <summary>
/// 挂起卡牌配置
/// </summary>
[method: SetsRequiredMembers]
public record HangingCardConfig(CardModel Card, HangingType HangingType, TriggerCountVar TriggerCount, CardType CardTypeFilter)
{
    [SetsRequiredMembers]
    public HangingCardConfig(CardModel Card, HangingType HangingType, CardType CardTypeFilter) :
        this(Card, HangingType, new TriggerCountVar(Card.DynamicVars.TriggerCount.IntValue), CardTypeFilter) { }

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
    public required TriggerCountVar TriggerCount { get; init; } = TriggerCount;

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
/// 本地化文本的基础模板
/// </summary>
public abstract record LocTemplate(string LocPrefix, params string[] Keys)
{
    // 每次访问获取一个新的 LocString 实例
    protected LocString CreateBaseLoc() => KeywordLocString(this.LocPrefix, this.Keys);
}

/// <summary>
/// 对应 [Hanging] general_description
/// "打出后，送入[orange]{SequenceName}[/orange]而非牌堆，直到效果耗尽。"
/// </summary>
public record LocGeneralDescription() : LocTemplate(HangingKey, "general_description")
{
    // 参数名直接对应 TOML 里的 {SequenceName}
    public LocString Fill(LocString SequenceName)
    {
        var loc = this.CreateBaseLoc();
        loc.Add(nameof(SequenceName), SequenceName);
        return loc;
    }
}

/// <summary>
/// 对应 [Hanging] card_description_frame
/// "[gold]{HangingKeyword}[/gold]：{HangingDescription}"
/// </summary>
public record LocCardDescriptionFrame() : LocTemplate(HangingKey, "card_description_frame")
{
    public LocString Fill(LocString HangingKeyword, LocString HangingDescription)
    {
        var loc = this.CreateBaseLoc();
        loc.Add(nameof(HangingKeyword), HangingKeyword);
        loc.Add(nameof(HangingDescription), HangingDescription);
        return loc;
    }
}

/// <summary>
/// 对应 [Hanging] keyword_title
/// "{HangingKeyword}效果"
/// </summary>
public record LocKeywordTitle() : LocTemplate(HangingKey, "keyword_title")
{
    public LocString Fill(LocString HangingKeyword)
    {
        var loc = this.CreateBaseLoc();
        loc.Add(nameof(HangingKeyword), HangingKeyword);
        return loc;
    }
}

/// <summary>
/// 对应 [Hanging.Follow] description / [Hanging.Delay] description
/// "后续{TriggerCount:diff()}次打出{CardType}时，{HangingEffect}。"
/// </summary>
public record LocHangingTypeDescription : LocTemplate
{
    public LocHangingTypeDescription(HangingType hangingType) : base(HangingKey, hangingType.ToString(), "description") { }

    // C# 参数名严格对应 TOML，谁也填不乱
    public LocString Fill(TriggerCountVar triggerCountVar, LocString CardType, LocString HangingEffect)
    {
        var loc = this.CreateBaseLoc();

        // 注意：Sts2 的动态变量(DynamicVar)通常依靠直接传入对象解析，
        // 它会自动匹配 TOML 里的 {TriggerCount:diff()}，所以这里保留原有 Add 逻辑
        loc.Add(triggerCountVar);

        // 普通的本地化文本替换，完美使用 nameof
        loc.Add(nameof(CardType), CardType);
        loc.Add(nameof(HangingEffect), HangingEffect);

        return loc;
    }
}

/// <summary>
/// 悬停提示标题的变量填补器
/// 对应本地化文件中的 {HangingKeyword}
/// </summary>
public record HangingKeywordTitleArgs(LocString HangingKeyword)
{
    public LocString Fill(LocString loc)
    {
        loc.Add(nameof(this.HangingKeyword), this.HangingKeyword);
        return loc;
    }
}

public static class HangingStaticLocs
{
    public static readonly string CardTypeName = nameof(CardType);

    public static readonly string HangingKey = "Hanging";

    public static LocString SequenceName => KeywordLocString(HangingKey, "Sequence", "name");
    public static LocString SequenceDescription => KeywordLocString(HangingKey, "Sequence", "description");
    
    public static IHoverTip SequenceHoverTip => new HoverTip(
        SequenceName,
        SequenceDescription
    );

    public static LocString GetTypeKeyword(HangingType type) => KeywordLocString(HangingKey, type.ToString(), "keyword");

    public static LocString GetCardTypeText(CardType type) => type switch
    {
        CardType.Attack => KeywordLocString(CardTypeName, "attack"),
        CardType.Skill => KeywordLocString(CardTypeName, "skill"),
        CardType.Power => KeywordLocString(CardTypeName, "power"),
        CardType.Status => KeywordLocString(CardTypeName, "status"),
        CardType.Curse => KeywordLocString(CardTypeName, "curse"),
        CardType.Quest => KeywordLocString(CardTypeName, "quest"),
        CardType.None => KeywordLocString(CardTypeName, "any"),
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
    };
}

/// <summary>
/// 挂起描述构建器
/// 用于统一处理挂起的本地化描述
/// </summary>
public static class HangingDescriptionBuilder
{
    public static LocString AddExtraArgsToDescription(LocString baseDescription, HangingCardConfig config)
    {
        var (hangingKeyword, hangingDescription) = BuildHangingDescription(config);

        // 像填表一样填入参数，利用 C# 具名参数，绝不弄混！
        var frame = new LocCardDescriptionFrame().Fill(
            HangingKeyword: hangingKeyword,
            HangingDescription: hangingDescription
        );

        baseDescription.Add("CardHangingDescription", frame);
        return baseDescription;
    }

    public static (LocString HangingKeyword, LocString HangingDescription) BuildHangingDescription(HangingCardConfig config)
    {
        var keyword = GetTypeKeyword(config.HangingType);

        // 利用模板 Fill() 方法，编译器会强迫你交出所有必须的变量
        var description = new LocHangingTypeDescription(config.HangingType).Fill(
            triggerCountVar: config.TriggerCount,
            CardType: GetCardTypeText(config.CardTypeFilter),
            HangingEffect: GetCardHangingEffect(config.Card)
        );

        return (keyword, description);
    }

    public static IEnumerable<IHoverTip> GetHoverTips(HangingCardConfig config, bool showHangingTotalDescription = false)
    {
        var (keyword, description) = BuildHangingDescription(config);

        var keywordTitle = new LocKeywordTitle().Fill(
            HangingKeyword: keyword
        );

        if (showHangingTotalDescription)
        {
            var generalDesc = new LocGeneralDescription().Fill(
                SequenceName: SequenceName
            );

            yield return new HoverTip(keyword, generalDesc);
            yield return SequenceHoverTip;
        }

        yield return new HoverTip(keywordTitle, description);
    }

    private static LocString GetCardHangingEffect(CardModel card)
    {
        var loc = new LocString("cards", $"{card.Id.Entry}.hangingEffect");
        card.DynamicVars.AddTo(loc);
        return loc;
    }
}