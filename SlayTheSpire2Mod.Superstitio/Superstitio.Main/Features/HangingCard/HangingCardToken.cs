using System.Diagnostics.CodeAnalysis;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using Superstitio.Main.Features.HangingCard.UI;

namespace Superstitio.Main.Features.HangingCard;

/// <summary>
/// 一次挂起对应的令牌。它会主动遥控对应的载体，载体只需要实现 <see cref="IHangingCarrier"/> 接口并接受遥控即可。
/// </summary>
[method: SetsRequiredMembers]
public abstract record HangingCardToken(
    int RemainCount,
    int InitialCount,
    CardModel HangingCard,
    Player OriginalOwner,
    PileType ReturnPileType
)
{
    /// <summary>
    /// 剩余计数
    /// </summary>
    public required int RemainCount { get; set; } = RemainCount;

    /// <summary>
    /// 初始计数（用于显示）
    /// </summary>
    public required int InitialCount { get; init; } = InitialCount;

    /// <summary>
    /// 被挂起的卡牌
    /// </summary>
    public required CardModel HangingCard { get; init; } = HangingCard;

    /// <summary>
    /// 卡牌的原始拥有者
    /// </summary>
    public required Player OriginalOwner { get; init; } = OriginalOwner;

    /// <summary>
    /// 返回的牌堆
    /// </summary>
    public required PileType ReturnPileType { get; init; } = ReturnPileType;

    /// <summary>
    /// 描述
    /// </summary>
    public abstract LocString Description { get; }

    /// <summary>
    /// 提供的额外悬浮提示
    /// </summary>
    public abstract IEnumerable<IHoverTip> ExtraHoverTips { get; }

    /// <summary>
    /// 挂起后，是否手动从战斗中移除
    /// 如果禁用，则由 ResultPileType 决定卡牌的去处
    /// </summary>
    /// <remarks>
    /// 这是由于，部分卡是挂起自身的，如果手动使用 <see cref="CardPileCmd.RemoveFromCombat(CardModel,bool)"/>> ，则可能会导致问题。
    /// </remarks>
    public bool ShouldManualRemoveFromBattle { get; init; } = true;

    /// <summary>
    /// 悬挂载体引用（在挂起时设置）
    /// </summary>
    public IHangingCarrier? Carrier { get; set; }

    /// <summary>
    /// 当一张牌被打出时，触发的效果。
    /// </summary>
    public abstract Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay);


    private CombatState? CombatState => this.OriginalOwner.Creature.CombatState;

    /// <summary>
    /// 挂起卡牌的视觉效果
    /// </summary>
    public HangingCardTokenDisplayer? Displayer { get; set; }

    /// <summary>
    /// 挂起卡牌
    /// </summary>
    public async Task HangCard(IHangingCarrier hangingCarrier)
    {
        // 双向绑定
        this.Carrier = hangingCarrier;
        this.Carrier.HangingCardToken = this;

        // 从战斗中移除卡牌
        if (this.ShouldManualRemoveFromBattle)
        {
            await CardPileCmd.RemoveFromCombat(this.HangingCard);
        }

        this.Displayer = new HangingCardTokenDisplayer(this);
    }


    /// <summary>
    /// 释放卡牌
    /// </summary>
    public async Task UnHangCard()
    {
        this.Displayer?.UnHangCard();
        this.Displayer = null;

        if (this.CombatState is null)
            return;

        // 创建卡牌副本并加入到需要回到的牌库
        var returnedCard = this.CombatState.CreateCard(this.HangingCard.CanonicalInstance, this.OriginalOwner);
        // 设置 oldPile，以让动画/UI正常进行
        // 我们手动把牌塞进 PlayPile 的 List 里。
        // 使用 silent: true，以免触发 C# 事件或 UI 更新信号。
        var playPile = PileType.Play.GetPile(this.OriginalOwner);
        playPile.AddInternal(returnedCard, silent: true);
        await CardPileCmd.Add(returnedCard, this.ReturnPileType);

        // 可选：播放返还特效
        // await PlayReturnVFX();

        if (this.Carrier is null)
            return;

        // 触发载体的挂起终止钩子
        await this.Carrier.HangingTerminate();

        // 双向解除绑定
        this.Carrier.HangingCardToken = null;
        this.Carrier = null;
    }

    /// <summary>
    /// 根据“当前悬停的手牌”和“当前指向的目标”，计算悬挂卡的视觉状态。
    /// </summary>
    public abstract HangingTriggerResult? GetTriggerResult(HangingTriggerContext context);
}

/// <summary>
/// 悬挂载体接口 - 对 <see cref="HangingCardToken"/> 提供生命周期管理能力
/// </summary>
public interface IHangingCarrier
{
    /// <summary>
    /// 当前挂起的令牌
    /// </summary>
    HangingCardToken? HangingCardToken { get; set; }

    /// <summary>
    /// 结束挂起（由 Token 主动调用）
    /// </summary>
    Task HangingTerminate();
}