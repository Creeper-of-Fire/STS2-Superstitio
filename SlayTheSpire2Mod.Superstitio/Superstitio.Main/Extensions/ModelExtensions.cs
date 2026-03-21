using MegaCrit.Sts2.Core.Models;

namespace Superstitio.Main.Extensions;

/// <summary>
/// 提供对各种 <see cref="AbstractModel"/> 的扩展方法。
/// </summary>
public static class CardModelExtensions
{
    /// <summary>
    /// 提供对 <see cref="CardModel"/> 的扩展方法。
    /// </summary>
    extension<TCardModel>(TCardModel cardModel) where TCardModel : CardModel, new()
    {
        /// <summary>从模型数据库中获取指定类型的卡牌模型实例。</summary>
        /// <remarks>注意，获取的是不可变实例（即样板）。</remarks>
        public static TCardModel Card()
        {
            return ModelDb.Card<TCardModel>();
        }
    }

    /// <summary>
    /// 提供对 <see cref="RelicModel"/> 的扩展方法。
    /// </summary>
    extension<TRelicModel>(TRelicModel relicModel) where TRelicModel : RelicModel, new()
    {
        /// <summary>从模型数据库中获取指定类型的遗物模型实例。</summary>
        /// <remarks>注意，获取的是不可变实例（即样板）。</remarks>
        public static TRelicModel Relic()
        {
            return ModelDb.Relic<TRelicModel>();
        }
    }

    /// <summary>
    /// 提供对 <see cref="PotionModel"/> 的扩展方法。
    /// </summary>
    extension<TPotionModel>(TPotionModel potionModel) where TPotionModel : PotionModel, new()
    {
        /// <summary>从模型数据库中获取指定类型的药水模型实例。</summary>
        /// <remarks>注意，获取的是不可变实例（即样板）。</remarks>
        public static TPotionModel Potion()
        {
            return ModelDb.Potion<TPotionModel>();
        }
    }


    /// <summary>
    /// 提供对 <see cref="AbstractModel"/> 的扩展方法。
    /// </summary>
    extension<TModel>(TModel cardModel) where TModel : AbstractModel, new()
    {
        /// <summary>
        /// 生成一个包含指定数量当前模型实例的序列。
        /// </summary>
        /// <param name="repeatTimes">要重复的次数。</param>
        /// <returns>一个包含 <paramref name="repeatTimes"/> 个当前模型实例的 <see cref="IEnumerable{TModel}"/>。</returns>
        public IEnumerable<TModel> Repeat(int repeatTimes)
        {
            return Enumerable.Repeat(cardModel, repeatTimes);
        }
    }
}