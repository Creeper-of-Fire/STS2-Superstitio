using MegaCrit.Sts2.Core.Entities.Cards;

namespace Superstitio.Api.Card;

/// <summary>
/// 用于初始化卡牌基本属性的记录类型。
/// </summary>
public record CardInitMessage
{
    /// <summary>
    /// 卡牌的基础费用。
    /// </summary>
    public required InitCostSpec BaseCost { get; init; }

    /// <summary>
    /// 卡牌的类型。
    /// </summary>
    public required CardType Type { get; init; }

    /// <summary>
    /// 卡牌的稀有度。
    /// </summary>
    public required CardRarity Rarity { get; init; }

    /// <summary>
    /// 卡牌的使用目标类型。
    /// </summary>
    public required TargetType Target { get; init; }

    /// <summary>
    /// 指示该卡牌是否显示在卡牌库中。
    /// </summary>
    public bool ShowInCardLibrary { get; init; } = true;
}