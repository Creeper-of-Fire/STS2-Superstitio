using MegaCrit.Sts2.Core.Models;

namespace Superstitio.Main.SubPool;

/// <summary>
/// 子池，每个子池有自身的卡牌、药水、遗物（？）。
/// </summary>
/// <remarks>
/// 非泛型基类。
/// </remarks>
public abstract class SubPool
{
    /// <summary>
    /// 这个池中包含的所有卡牌。
    /// </summary>
    public abstract IEnumerable<CardModel> PoolCards { get; }

    /// <summary>
    /// 这个池中包含的所有药水。
    /// </summary>
    public abstract IEnumerable<PotionModel> PoolPotions { get; }

    // 公共属性：图标、描述等
}

/// <summary>
/// 现代化泛型基类。
/// </summary>
/// <typeparam name="TSelf">子类自身类型</typeparam>
public abstract class SubPool<TSelf> : SubPool where TSelf : SubPool<TSelf>
{
    private IEnumerable<CardModel>? CachedCards { get; set; }

    /// <summary>
    /// 获取此池中包含的所有卡牌实例。
    /// 结果会被缓存，多次调用不会重复创建实例。
    /// </summary>
    public override IEnumerable<CardModel> PoolCards => this.CachedCards ??=
    [
        ..SubPoolMemberRegistry.GetSubTypes<TSelf>()
            .Select(type =>
            {
                // 获取该卡牌类在 StS2 中的 ModelId
                var id = ModelDb.GetId(type);
                // 从 ModelDb 缓存中获取单例实例（这些实例已由 ModelDb.Init 初始化）
                return ModelDb.GetById<CardModel>(id);
            })
    ];

    private IEnumerable<PotionModel>? CachedPotions { get; set; }

    /// <summary>
    /// 获取此池中包含的所有药水实例。
    /// 结果会被缓存，多次调用不会重复创建实例。
    /// </summary>
    public override IEnumerable<PotionModel> PoolPotions => this.CachedPotions ??=
    [
        ..SubPoolMemberRegistry.GetSubTypes<TSelf>()
            .Select(type =>
            {
                // 获取该药水类在 StS2 中的 ModelId
                var id = ModelDb.GetId(type);
                // 从 ModelDb 缓存中获取单例实例（这些实例已由 ModelDb.Init 初始化）
                return ModelDb.GetById<PotionModel>(id);
            })
    ];
}