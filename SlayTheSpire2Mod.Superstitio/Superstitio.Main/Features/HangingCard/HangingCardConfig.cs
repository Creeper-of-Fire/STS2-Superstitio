using System.Diagnostics.CodeAnalysis;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using Superstitio.Main.DynamicVars;
using Superstitio.Main.DynamicVars.Extensions;

namespace Superstitio.Main.Features.HangingCard;

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
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