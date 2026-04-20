using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Superstitio.Api.DynamicVars.Extensions;

/// <summary>
/// 动态变量集的扩展方法
/// </summary>
public static class DynamicVarSetExtensions
{
    extension(DynamicVarSet dynamicVarSet)
    {
        /// <summary>
        /// 获取指定类型的动态变量，若不存在或类型不匹配则抛出异常
        /// </summary>
        public TVar GetVarOrThrow<TVar>(string key) where TVar : DynamicVar
        {
            if (!dynamicVarSet.TryGetValue(key, out var dynamicVar))
                throw new ArgumentException($"未能找到键为 '{key}' 的动态变量。");

            if (dynamicVar is not TVar typedValue)
                throw new ArgumentException($"键为 '{key}' 的动态变量存在，但其类型不是 '{typeof(TVar).Name}'。");

            return typedValue;
        }

        /// <summary>
        /// 获取指定名称的动态变量，若不存在则抛出异常
        /// </summary>
        public DynamicVar GetVarOrThrow(string key)
        {
            if (!dynamicVarSet.TryGetValue(key, out var dynamicVar))
                throw new ArgumentException($"未能找到键为 '{key}' 的动态变量。");

            return dynamicVar;
        }
    }
}