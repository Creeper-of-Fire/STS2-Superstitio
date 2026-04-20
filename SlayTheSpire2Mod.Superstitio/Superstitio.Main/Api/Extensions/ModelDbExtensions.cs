using MegaCrit.Sts2.Core.Models;

namespace Superstitio.Api.Extensions;

/// <summary>
/// 模型库的扩展方法
/// </summary>
public static class ModelDbExtensions
{
    /// <summary>
    /// 将规范的 Card 模型转换为可变的实例
    /// </summary>
    /// <typeparam name="T">Card 类型</typeparam>
    /// <param name="card">规范的 Card 实例</param>
    /// <returns>可变的 Card 实例</returns>
    public static T ToMutableCard<T>(this T card) where T : CardModel
    {
        ArgumentNullException.ThrowIfNull(card);

        var mutable = card.ToMutable();

        if (mutable is not T typedMutable)
            throw new InvalidCastException($"期望类型为 {typeof(T).Name}，但实际得到 {mutable.GetType().Name}");

        return typedMutable;
    }

    /// <summary>
    /// 将规范的 Relic 模型转换为可变的实例
    /// </summary>
    /// <typeparam name="T">Relic 类型</typeparam>
    /// <param name="relic">规范的 Relic 实例</param>
    /// <returns>可变的 Relic 实例</returns>
    public static T ToMutableRelic<T>(this T relic) where T : RelicModel
    {
        ArgumentNullException.ThrowIfNull(relic);

        var mutable = relic.ToMutable();

        if (mutable is not T typedMutable)
            throw new InvalidCastException($"期望类型为 {typeof(T).Name}，但实际得到 {mutable.GetType().Name}");

        return typedMutable;
    }

    /// <summary>
    /// 将规范的 Power 模型转换为可变的实例
    /// </summary>
    /// <typeparam name="T">Power 类型</typeparam>
    /// <param name="power">规范的 Power 实例</param>
    /// <param name="initialAmount">初始层数</param>
    /// <returns>可变的 Power 实例</returns>
    public static T ToMutablePower<T>(this T power, int initialAmount = 0) where T : PowerModel
    {
        ArgumentNullException.ThrowIfNull(power);

        var mutable = power.ToMutable(initialAmount);

        if (mutable is not T typedMutable)
            throw new InvalidCastException($"期望类型为 {typeof(T).Name}，但实际得到 {mutable.GetType().Name}");

        return typedMutable;
    }
    
    extension(ModelDb)
    {
        /// <summary>
        /// 从模型数据库中获取指定类型的可变 Card 实例。
        /// </summary>
        /// <typeparam name="TCardModel">Card 模型的类型。</typeparam>
        /// <returns>初始化的可变 Card 实例。</returns>
        public static TCardModel GetMutableCard<TCardModel>() where TCardModel : CardModel
        {
            return ModelDb.Card<TCardModel>().ToMutableCard();
        }

        /// <summary>
        /// 从模型数据库中获取指定类型的可变 Relic 实例。
        /// </summary>
        /// <typeparam name="TRelicModel">Relic 模型的类型。</typeparam>
        /// <returns>初始化的可变 Relic 实例。</returns>
        public static TRelicModel GetMutableRelic<TRelicModel>() where TRelicModel : RelicModel
        {
            return ModelDb.Relic<TRelicModel>().ToMutableRelic();
        }

        /// <summary>
        /// 从模型数据库中获取指定类型的可变 Power 实例。
        /// </summary>
        /// <typeparam name="TPowerModel">Power 模型的类型。</typeparam>
        /// <returns>初始化的可变 Power 实例。</returns>
        public static TPowerModel GetMutablePower<TPowerModel>() where TPowerModel : PowerModel
        {
            return ModelDb.Power<TPowerModel>().ToMutablePower();
        }

        /// <summary>
        /// 根据类型获取卡牌模型单例。而非使用泛型，便于反射等操作。
        /// </summary>
        /// <param name="cardType">卡牌类的类型。</param>
        /// <returns>对应的 <see cref="CardModel"/> 实例。</returns>
        public static CardModel Card(Type cardType)
        {
            // 获取该卡牌类在 StS2 中的 ModelId
            var id = ModelDb.GetId(cardType);
            // 从 ModelDb 缓存中获取单例实例
            return ModelDb.GetById<CardModel>(id);
        }

        /// <summary>
        /// 根据类型获取药水模型单例。而非使用泛型，便于反射等操作。
        /// </summary>
        /// <param name="potionType">药水类的类型。</param>
        /// <returns>对应的 <see cref="PotionModel"/> 实例。</returns>
        public static PotionModel Potion(Type potionType)
        {
            // 获取该药水类在 StS2 中的 ModelId
            var id = ModelDb.GetId(potionType);
            // 从 ModelDb 缓存中获取单例实例
            return ModelDb.GetById<PotionModel>(id);
        }

        /// <summary>
        /// 根据类型获取遗物模型单例。而非使用泛型，便于反射等操作。
        /// </summary>
        /// <param name="relicType">遗物类的类型。</param>
        /// <returns>对应的 <see cref="RelicModel"/> 实例。</returns>
        public static RelicModel Relic(Type relicType)
        {
            // 获取该遗物类在 StS2 中的 ModelId
            var id = ModelDb.GetId(relicType);
            // 从 ModelDb 缓存中获取单例实例
            return ModelDb.GetById<RelicModel>(id);
        }
    }
}