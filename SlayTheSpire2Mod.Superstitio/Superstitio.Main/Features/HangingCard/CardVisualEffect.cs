using System.Diagnostics.CodeAnalysis;
using MegaCrit.Sts2.Core.Entities.Cards;
using Superstitio.Main.Features.HangingCard.UI;

namespace Superstitio.Main.Features.HangingCard;

/// <summary>
/// 挂起卡牌的视觉配置
/// </summary>
public sealed record CardVisualEffect
{
    /// <summary>
    /// 
    /// </summary>
    public required HangGlowType? HangGlowType { get; init; }

    /// <summary>
    /// 
    /// </summary>
    public required TargetType? TargetType { get; init; }
}