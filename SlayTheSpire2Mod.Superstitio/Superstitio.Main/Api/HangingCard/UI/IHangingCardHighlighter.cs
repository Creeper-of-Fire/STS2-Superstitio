using MegaCrit.Sts2.Core.Models;
using Superstitio.Analyzer;

namespace Superstitio.Api.HangingCard.UI;

/// <summary>
/// 实现此接口的卡牌在被 Hover 时可以高亮悬挂卡
/// </summary>
[AttachedTo(typeof(CardModel))]
public interface IHangingCardHighlighter
{
    /// <summary>
    /// 为指定的悬挂卡创建触发上下文
    /// </summary>
    Func<HangingCardToken, HangingTriggerContext, HangingTriggerResult?, HangingTriggerResult?> ChangeTriggerResult { get; }
}

/// <summary>
/// 简单高亮器的抽象基类，只需重写 CardFilter
/// </summary>
public interface ISimpleHangingCardHighlighter : IHangingCardHighlighter
{
    /// <summary>
    /// 过滤卡牌
    /// </summary>
    public Func<HangingCardToken, bool> TokenIsAble { get; }

    Func<HangingCardToken, HangingTriggerContext, HangingTriggerResult?, HangingTriggerResult?>
        IHangingCardHighlighter.ChangeTriggerResult => (hangingCard, context, originResult) =>
    {
        if (!this.TokenIsAble(hangingCard))
            return null;
        return this.SimpleChangeTriggerResult(hangingCard, context, originResult);
    };

    /// <summary>
    /// 为指定的悬挂卡创建触发上下文
    /// </summary>
    /// <code>
    /// 输入：
    /// 被查询的悬挂卡
    /// 当前悬挂上下文
    /// 原先的攻击方案
    /// </code>
    Func<HangingCardToken, HangingTriggerContext, HangingTriggerResult?, HangingTriggerResult?>
        SimpleChangeTriggerResult => (hangingCard, context, originResult) => new HangingTriggerResult(HangGlowType.Preview, null);
}