using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Superstitio.Main.Base;

/// <summary>
/// 表示一个带有升级值的动态变量记录。
/// </summary>
public record DynamicVarWithUpgrade
{
    /// <summary>
    /// 动态变量。
    /// </summary>
    public required DynamicVar DynamicVar { get; init; }

    /// <summary>
    /// 升级值。
    /// </summary>
    public required decimal UpgradeValue { get; init; }

    /// <summary>
    /// 从 <see cref="DynamicVar"/> 隐式转换为 <see cref="DynamicVarWithUpgrade"/>，升级值为 0。
    /// </summary>
    /// <param name="dynamicVar">动态变量。</param>
    public static implicit operator DynamicVarWithUpgrade(DynamicVar dynamicVar) =>
        new() { DynamicVar = dynamicVar, UpgradeValue = 0 };

    /// <summary>
    /// 从元组 (DynamicVar, decimal) 隐式转换为 <see cref="DynamicVarWithUpgrade"/>。
    /// </summary>
    /// <param name="dynamicVar">包含动态变量和升级值的元组。</param>
    public static implicit operator DynamicVarWithUpgrade((DynamicVar DynamicVar, decimal UpgradeValue) dynamicVar) =>
        new() { DynamicVar = dynamicVar.DynamicVar, UpgradeValue = dynamicVar.UpgradeValue };
}

/// <summary>
/// 为 <see cref="DynamicVar"/> 提供扩展方法。
/// </summary>
public static class DynamicVarExtensions
{
    /// <summary>
    /// 创建一个带有指定升级值的 <see cref="DynamicVarWithUpgrade"/> 实例。
    /// </summary>
    /// <param name="dynamicVar"></param>
    /// <param name="upgradeValue">升级值。</param>
    /// <returns>包含原始动态变量和指定升级值的记录。</returns>
    public static DynamicVarWithUpgrade WithUpgrade(this DynamicVar dynamicVar, decimal upgradeValue) =>
        new() { DynamicVar = dynamicVar, UpgradeValue = upgradeValue };
}