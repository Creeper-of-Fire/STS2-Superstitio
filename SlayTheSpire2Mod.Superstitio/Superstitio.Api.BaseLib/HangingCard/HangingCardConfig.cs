using System.Diagnostics.CodeAnalysis;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using Superstitio.Api.HangingCard;

namespace Superstitio.Api.BaseLib.HangingCard;

/// <summary>
/// 挂起卡牌配置
/// </summary>
[method: SetsRequiredMembers]
public record HangingCardConfig(
    CardModel Card,
    HangingType HangingType,
    CardType CardTypeFilter,
    CardVisualEffect? CardVisualEffect,
    TriggerCountVar? TriggerCount = null
)
{
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
    public required TriggerCountVar TriggerCount { get; init; } =
        TriggerCount ?? new TriggerCountVar(Card.DynamicVars.TriggerCount.IntValue);

    /// <summary>
    /// 触发的卡牌类型过滤（ <see cref="CardType.None"/> 为任意牌）
    /// </summary>
    public required CardType CardTypeFilter { get; init; } = CardTypeFilter;

    /// <summary>
    /// 卡牌的视觉效果类型
    /// </summary>
    /// <returns></returns>
    public required CardVisualEffect? CardVisualEffect { get; init; } = CardVisualEffect;
}