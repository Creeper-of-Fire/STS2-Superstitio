using System.Reflection;
using MegaCrit.Sts2.Core.Logging;

namespace Superstitio.Main.SubPool;

/// <summary>
/// SubPool 特性，用于标记某个卡牌属于哪个子卡池
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
public class AddToSubPoolAttribute : Attribute
{
    /// <summary>
    /// 
    /// </summary>
    public Type SubPoolType { get; }

    /// <inheritdoc />
    public AddToSubPoolAttribute(Type subPoolType)
    {
        if (!typeof(SubPool).IsAssignableFrom(subPoolType))
            throw new ArgumentException($"{subPoolType} 必须继承自 {nameof(SubPool)}");

        this.SubPoolType = subPoolType;
    }
}


/// <summary>
/// 管理所有通过 AddToSubPoolAttribute 注册的类型。
/// </summary>
public static class SubPoolMemberRegistry
{
    /// <summary>
    /// 使用一个字典来存储从子卡池类型到其所有子类型列表的映射。
    /// 键：子卡池类型（例如：typeof(SubCardPool1)）
    /// 值：标记有 [SubPool(PoolType)] 的特性类型列表（例如：[typeof(MasoBaseCard)]）
    /// </summary>
    private static Dictionary<Type, List<Type>> SubPools { get; } = new();

    /// <summary>
    /// 扫描给定的程序集，查找所有带有 [SubPool] 特性的类，并将它们注册到管理器中。
    /// </summary>
    /// <param name="assembly">要扫描的程序集。</param>
    public static void RegisterSubPoolsFromAssembly(Assembly assembly)
    {
        Log.Info($"[{Plugin.ModName}] 正在扫描程序集 '{assembly.GetName().Name}' 以查找子池...");
        int count = 0;
        try
        {
            // 遍历程序集中的所有类型
            foreach (var type in assembly.GetTypes())
            {
                // 确保类型是具体的类，而不是抽象类或接口
                if (!type.IsClass || type.IsAbstract)
                {
                    continue;
                }

                // 获取该类型上的所有 AddToSubPoolAttribute 实例
                var attributes = type.GetCustomAttributes<AddToSubPoolAttribute>(true);

                foreach (var attr in attributes)
                {
                    // 如果字典中还没有这个子卡池类型的键，则创建一个新列表
                    if (!SubPools.ContainsKey(attr.SubPoolType))
                    {
                        SubPools[attr.SubPoolType] = [];
                    }

                    // 将当前类型添加到对应子卡池类型的列表中
                    SubPools[attr.SubPoolType].Add(type);
                    count++;
                    // Log.Debug($"[{Plugin.ModName}] 已将 '{type.FullName}' 注册到池 '{attr.PoolType.FullName}'。");
                }
            }

            Log.Info($"[{Plugin.ModName}] 扫描完成。已注册 {count} 个子池条目。");
        }
        catch (ReflectionTypeLoadException ex)
        {
            // 这是一个常见的异常，当程序集引用的某些依赖项无法加载时发生。
            // 我们可以记录错误并继续处理已成功加载的类型。
            Log.Error($"[{Plugin.ModName}] 从程序集 '{assembly.GetName().Name}' 加载类型时出错。{ex.Message}");
            foreach (var loaderException in ex.LoaderExceptions)
            {
                Log.Error($"  - 加载异常：{loaderException?.Message}");
            }
        }
        catch (Exception ex)
        {
            Log.Error($"[{Plugin.ModName}] 在子池扫描过程中发生未知错误：{ex}");
        }
    }

    /// <summary>
    /// 根据子卡池类型获取所有已注册的子类型。
    /// </summary>
    /// <param name="poolType">子卡池类型。</param>
    /// <returns>一个包含所有子类型的只读集合。如果找不到该池，则返回一个空集合。</returns>
    public static IEnumerable<Type> GetSubTypes(Type poolType)
    {
        if (SubPools.TryGetValue(poolType, out var subTypes))
        {
            return subTypes.AsReadOnly();
        }

        // 返回一个空集合，以避免调用方需要处理 null 的情况
        return [];
    }

    /// <summary>
    /// 泛型版本的 GetSubTypes，使用更方便。
    /// </summary>
    /// <typeparam name="T">子卡池类型。</typeparam>
    /// <returns>一个包含所有子类型的只读集合。</returns>
    public static IEnumerable<Type> GetSubTypes<T>()
    {
        return GetSubTypes(typeof(T));
    }
}