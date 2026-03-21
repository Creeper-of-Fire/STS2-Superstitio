using System.Reflection;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using Superstitio.Main.Maso;
using Superstitio.Main.Maso.Pools;

namespace Superstitio.Main.SubPool.UI;

/// <summary>
/// UI 上选择好的卡牌池
/// </summary>
public static class SubPoolManager
{
    // TODO 完全没有做持久化啥的。

    // 1. 通用子池仓库：存放所有实例化后的 SubPool
    private static readonly List<SubPool> AllUniversalSubPools = [];

    // 2. 状态映射表：记录某个主池（如 MasoCardPool）启用了哪些子池
    private static readonly Dictionary<Type, HashSet<SubPool>> EnabledStateMap = new();

    /// <summary>
    /// 初始化：扫描程序集，把所有的 SubPool 找出来实例化，放入通用仓库
    /// 请在 Plugin.Initialize 中调用。
    /// </summary>
    public static void Initialize(Assembly assembly)
    {
        var poolTypes = assembly.GetTypes()
            .Where(t => typeof(SubPool).IsAssignableFrom(t) && !t.IsAbstract && !t.IsGenericType);

        foreach (var type in poolTypes)
        {
            // 注意！！！这是因为 SubPool 不是 AbstractModel，因此才可以直接创建实例，而非使用 Model.DB
            var poolInstance = (SubPool?)Activator.CreateInstance(type);

            if (poolInstance is null)
            {
                Log.Warn($"[MasoMod] 创建子池实例失败：{type.Name}。");
                continue;
            }

            AllUniversalSubPools.Add(poolInstance);

            // 默认行为：如果你希望所有子池初始状态都是“开启”的，可以在这里配置
            // 例如：默认给 MasoCardPool 开启所有子池
            SetPoolEnabled<MasoCardPool>(poolInstance, true);
        }
    }

    /// <summary>
    /// 供 UI 使用：获取所有通用子池
    /// </summary>
    public static IEnumerable<SubPool> GetAllSubPools() => AllUniversalSubPools;

    /// <summary>
    /// 供 主池/UI 使用：检查或获取状态
    /// </summary>
    public static IEnumerable<SubPool> GetEnabledSubPools<TCardPool>() where TCardPool : CardPoolModel
    {
        if (EnabledStateMap.TryGetValue(typeof(TCardPool), out var enabledPools))
            return enabledPools;
        return [];
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pool"></param>
    /// <typeparam name="TCardPool"></typeparam>
    /// <returns></returns>
    public static bool IsPoolEnabled<TCardPool>(SubPool pool) where TCardPool : CardPoolModel
    {
        return EnabledStateMap.TryGetValue(typeof(TCardPool), out var set) && set.Contains(pool);
    }

    /// <summary>
    /// 供 UI 使用：切换状态
    /// </summary>
    public static void SetPoolEnabled<TCardPool>(SubPool pool, bool isEnabled) where TCardPool : CardPoolModel
    {
        if (!EnabledStateMap.ContainsKey(typeof(TCardPool)))
            EnabledStateMap[typeof(TCardPool)] = [];

        if (isEnabled)
            EnabledStateMap[typeof(TCardPool)].Add(pool);
        else
            EnabledStateMap[typeof(TCardPool)].Remove(pool);
    }


    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// 是的，超级帅气的，C#14的拓展语法！
    /// </remarks>
    /// <param name="cardPool"></param>
    /// <typeparam name="TCardPool"></typeparam>
    extension<TCardPool>(IWithSubPool<TCardPool> cardPool) where TCardPool : CardPoolModel
    {
        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<CardModel> AllSubPoolCards =>
            GetEnabledSubPools<TCardPool>().SelectMany(x => x.PoolCards);

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<PotionModel> AllSubPoolPotions =>
            GetEnabledSubPools<TCardPool>().SelectMany(x => x.PoolPotions);
    }
}

/// <summary>
/// 
/// </summary>
/// <typeparam name="TCardPool"></typeparam>
public interface IWithSubPool<TCardPool> where TCardPool : CardPoolModel;