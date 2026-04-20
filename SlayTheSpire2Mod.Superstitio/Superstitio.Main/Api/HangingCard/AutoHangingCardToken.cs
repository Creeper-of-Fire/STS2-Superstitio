using System.Diagnostics.CodeAnalysis;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Superstitio.Api.HangingCard;

/// <summary>
/// 增强版挂起令牌 - 响应即减少计数，提供触发时和结束时两个钩子
/// </summary>
[method: SetsRequiredMembers]
public abstract record AutoHangingCardToken(
    int RemainCount,
    int InitialCount,
    CardModel HangingCard,
    Player OriginalOwner,
    PileType ReturnPileType
) : HangingCardToken(RemainCount, InitialCount, HangingCard, OriginalOwner, ReturnPileType)
{
    /// <summary>
    /// 过滤条件：判断是否应该响应这次卡牌打出
    /// </summary>
    /// <param name="context">玩家选择上下文</param>
    /// <param name="cardPlay">打出的卡牌信息</param>
    /// <returns>true=响应，false=忽略</returns>
    protected virtual bool ShouldRespond(PlayerChoiceContext context, CardPlay cardPlay) => true;

    /// <summary>
    /// 触发钩子：每次响应时执行（在计数减少之前）
    /// </summary>
    /// <param name="context">玩家选择上下文</param>
    /// <param name="cardPlay">打出的卡牌信息</param>
    /// <returns>Task</returns>
    public virtual Task OnTrigger(PlayerChoiceContext context, CardPlay cardPlay)
        => Task.CompletedTask;

    /// <summary>
    /// 结束钩子：当剩余计数归零时执行（在释放卡牌之前）
    /// </summary>
    /// <param name="context">玩家选择上下文</param>
    /// <param name="cardPlay">最后一次触发的卡牌信息</param>
    /// <returns>Task</returns>
    public virtual Task OnEnd(PlayerChoiceContext context, CardPlay cardPlay)
        => Task.CompletedTask;

    /// <summary>
    /// 重写父类的 AfterCardPlayed，实现响应即减少的通用逻辑
    /// </summary>
    public sealed override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        // 打出自身（不是复制）的那次，忽略。
        if (cardPlay.Card == this.HangingCard)
            return;

        // 1. 过滤检查
        if (!this.ShouldRespond(context, cardPlay))
            return;

        // 2. 计数检查
        if (this.RemainCount <= 0)
            return;

        // 3. 触发钩子（在减少之前）
        await this.OnTrigger(context, cardPlay);

        // 4. 减少计数
        this.RemainCount--;

        if (this.RemainCount > 0)
            return;

        // 5. 如果归零，执行结束钩子并释放卡牌
        await this.OnEnd(context, cardPlay);
        await this.UnHangCard();
    }
}