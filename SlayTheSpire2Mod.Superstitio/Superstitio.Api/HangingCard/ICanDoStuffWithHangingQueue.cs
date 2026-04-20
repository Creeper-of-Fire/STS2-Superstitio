using MegaCrit.Sts2.Core.Models;
using Superstitio.Analyzer;
using Superstitio.Api.HangingCard.UI;

namespace Superstitio.Api.HangingCard;

/// <summary>
/// 标记接口：实现此接口的卡牌在打出时不会触发悬挂卡牌的响应效果。
/// 用于防止递归触发或自我触发。
/// </summary>
[AttachedTo(typeof(CardModel))]
public interface INoTriggerHangingOnPlay;

/// <summary>
/// 组合接口：表示该卡牌可以与悬挂队列进行交互。
/// 继承此接口的卡牌既不会在打出时触发悬挂卡牌，也能在悬停时高亮相关的悬挂卡。
/// </summary>
public interface ICanDoStuffWithHangingQueue : INoTriggerHangingOnPlay, IHangingCardHighlighter;