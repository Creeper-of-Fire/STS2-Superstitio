using System.Diagnostics.CodeAnalysis;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;

namespace Superstitio.Main.Extensions;

/// <summary>
/// 为 <see cref="CardPlay"/> 和相关类型提供扩展方法。
/// </summary>
public static class CardPlayExtensions
{
    extension(CardPlay? cardPlay)
    {
        /// <summary>
        /// 获取卡牌播放的目标生物，如果目标为空则抛出异常。
        /// </summary>
        /// <returns>目标生物实例。</returns>
        /// <exception cref="ArgumentNullException">当 <see cref="CardPlay.Target"/> 为 null 时抛出。</exception>
        public Creature GetTargetOrThrow()
        {
            ArgumentNullException.ThrowIfNull(cardPlay?.Target);
            return cardPlay.Target;
        }
    }

    extension(CardPlay cardPlay)
    {
        /// <summary>
        /// 得到卡牌的打出者/拥有者
        /// </summary>
        public Player CardPlayer => cardPlay.Card.Owner;
    }
}