using MegaCrit.Sts2.Core.Models;
using Superstitio.Main.Extensions;

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
    /// 子池的唯一标识符，用于序列化和查找
    /// </summary>
    public abstract string Id { get; }

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
    /// <summary>
    /// 使用类型名作为唯一标识符
    /// </summary>
    public override string Id => typeof(TSelf).Name;
    
    /// <summary>
    /// 获取此池中包含的所有卡牌实例。
    /// 结果会被缓存，多次调用不会重复创建实例。
    /// </summary>
    public override IEnumerable<CardModel> PoolCards => field ??=
    [
        ..SubPoolMemberRegistry.GetSubTypes<TSelf>()
            .Select(ModelDb.Card)
    ];

    /// <summary>
    /// 获取此池中包含的所有药水实例。
    /// 结果会被缓存，多次调用不会重复创建实例。
    /// </summary>
    public override IEnumerable<PotionModel> PoolPotions => field ??=
    [
        ..SubPoolMemberRegistry.GetSubTypes<TSelf>()
            .Select(ModelDb.Potion)
    ];
}