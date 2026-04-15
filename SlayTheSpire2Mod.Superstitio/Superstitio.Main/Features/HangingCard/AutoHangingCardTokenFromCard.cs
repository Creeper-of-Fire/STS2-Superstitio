using System.Diagnostics.CodeAnalysis;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using Superstitio.Main.Base;
using Superstitio.Main.Features.HangingCard.UI;

namespace Superstitio.Main.Features.HangingCard;

/// <summary>
/// 为实现了 <see cref="IWithHangingConfigCard"/> 接口的卡牌提供创建挂起令牌的扩展方法。
/// </summary>
public static class IWithHangingConfigCardExtensions
{
    /// <summary>
    /// 基于卡牌的挂起配置创建一个 <see cref="AutoHangingCardTokenWithConfig"/> 实例。
    /// </summary>
    /// <typeparam name="TCard">卡牌类型，必须继承自 <see cref="SuperstitioBaseCard"/> 并实现 <see cref="IWithHangingConfigCard"/> 接口。</typeparam>
    /// <param name="card">当前卡牌实例。</param>
    /// <param name="hangingAction">当挂起条件触发时执行的异步动作。</param>
    /// <param name="resultPileType">当挂起结束时，返回什么地方，可为空，此时默认为 <see cref="SuperstitioBaseCard.BaseResultPileType"/></param>
    /// <param name="hangGlowType"></param>
    /// <param name="effectTargetType"></param>
    /// <returns>一个配置好的 <see cref="AutoHangingCardTokenWithConfig"/> 实例。</returns>
    public static AutoHangingCardTokenFromCard<TCard> CreateHangingToken<TCard>(
        this TCard card,
        Func<PlayerChoiceContext, CardPlay, Task> hangingAction,
        PileType? resultPileType = null,
        HangGlowType? hangGlowType = null,
        TargetType? effectTargetType = null
    ) where TCard : SuperstitioBaseCard, IWithHangingConfigCard
    {
        return card.CreateHangingToken(hangingAction, resultPileType ?? card.BaseResultPileType, hangGlowType, effectTargetType);
    }

    /// <summary>
    /// 基于卡牌的挂起配置创建一个 <see cref="AutoHangingCardTokenWithConfig"/> 实例。
    /// </summary>
    /// <typeparam name="TCard">卡牌类型，必须继承自 <see cref="SuperstitioBaseCard"/> 并实现 <see cref="IWithHangingConfigCard"/> 接口。</typeparam>
    /// <param name="card">当前卡牌实例。</param>
    /// <param name="hangingAction">当挂起条件触发时执行的异步动作。</param>
    /// <param name="resultPileType">当挂起结束时，返回什么地方</param>
    /// <param name="hangGlowType"></param>
    /// <param name="effectTargetType"></param>
    /// <returns>一个配置好的 <see cref="AutoHangingCardTokenWithConfig"/> 实例。</returns>
    public static AutoHangingCardTokenFromCard<TCard> CreateHangingToken<TCard>(
        this TCard card,
        Func<PlayerChoiceContext, CardPlay, Task> hangingAction,
        PileType resultPileType,
        HangGlowType? hangGlowType = null,
        TargetType? effectTargetType = null
    ) where TCard : CardModel, IWithHangingConfigCard
    {
        var hangingToken = new AutoHangingCardTokenFromCard<TCard>(card, resultPileType)
        {
            // 如果卡牌在打出后挂起自身，则手动禁用“挂起后，由 Token 手动从战斗中移除"功能，转而让游戏自动在打出后送入 ResultPileType
            ShouldManualRemoveFromBattle = !card.HangingSelfAfterPlay,
            HangingAction = hangingAction
        };

        if (effectTargetType is not null)
        {
            hangingToken = hangingToken with
            {
                TargetType = effectTargetType.Value
            };
        }

        if (hangGlowType is not null)
        {
            hangingToken = hangingToken with
            {
                HangGlowType = hangGlowType.Value
            };
        }

        return hangingToken;
    }
}

/// <inheritdoc />
[method: SetsRequiredMembers]
public record AutoHangingCardTokenFromCard<TCard>(
    TCard OriginCard,
    PileType ReturnPileType
) : AutoHangingCardTokenWithConfig(
    HangingCardConfig: OriginCard.HangingCardConfig,
    ReturnPileType: ReturnPileType
) where TCard : CardModel, IWithHangingConfigCard
{
    /// <summary>
    /// 触发本次挂起的卡牌
    /// </summary>
    public TCard OriginCard { get; init; } = OriginCard;

    /// <summary>
    /// 卡牌的目标类型
    /// </summary>
    public TargetType TargetType { get; init; } = OriginCard.TargetType;

    /// <summary>
    /// 
    /// </summary>
    public HangGlowType HangGlowType { get; init; } = OriginCard.TargetType switch
    {
        TargetType.None => HangGlowType.Special,
        TargetType.Self => HangGlowType.Good,
        TargetType.AnyEnemy => HangGlowType.Bad,
        TargetType.AllEnemies => HangGlowType.Bad,
        TargetType.RandomEnemy => HangGlowType.Bad,
        TargetType.AnyPlayer => HangGlowType.Good,
        TargetType.AnyAlly => HangGlowType.Bad,
        TargetType.AllAllies => HangGlowType.Good,
        TargetType.TargetedNoCreature => HangGlowType.Special,
        TargetType.Osty => HangGlowType.Good,
        _ => HangGlowType.Special
    };

    private NCreature? ChooseTarget(NCreature? hoveredCreature)
    {
        switch (this.TargetType)
        {
            case TargetType.Self:
                return NCombatRoom.Instance?.GetCreatureNode(this.OriginalOwner.Creature);
            case TargetType.AnyEnemy:
            case TargetType.AllEnemies:
            case TargetType.RandomEnemy:
                return hoveredCreature?.Entity.IsEnemy == true ? hoveredCreature : null;
            case TargetType.AnyPlayer:
            case TargetType.AnyAlly:
            case TargetType.AllAllies:
                return hoveredCreature?.Entity.IsPlayer == true ? hoveredCreature : null;
            case TargetType.TargetedNoCreature:
                return hoveredCreature;
            case TargetType.Osty:
                return NCombatRoom.Instance?.GetCreatureNode(this.OriginalOwner.Osty ?? this.OriginalOwner.Creature);
            default:
                return null;
        }
    }

    /// <inheritdoc />
    protected override HangingTriggerResult CreateTriggerResult(HangingTriggerContext context)
    {
        if (!context.IsTargetingActive)
            return new HangingTriggerResult(this.HangGlowType, null);
        return new HangingTriggerResult(this.HangGlowType, this.ChooseTarget(context.HoveredCreature));
    }
}