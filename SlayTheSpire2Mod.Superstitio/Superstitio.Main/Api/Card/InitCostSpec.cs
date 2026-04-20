using MegaCrit.Sts2.Core.Models;
using Superstitio.Analyzer;

namespace Superstitio.Api.Card;

/// <summary>
/// 用于记录卡牌能量消耗及其升级变化的消息类型。
/// </summary>
public record InitCostSpec
{
    /// <summary>
    /// 卡牌的初始能量消耗。
    /// </summary>
    public required int InitialCost { get; init; }

    /// <summary>
    /// 卡牌升级后的能量消耗（可为空，表示未升级或无升级变化）。
    /// </summary>
    public int UpgradedCost { get; private init; }

    /// <summary>
    /// 设置升级后的能量消耗。
    /// </summary>
    /// <param name="upgradedCost">升级后的能量消耗值。</param>
    /// <returns>包含升级信息的新实例。</returns>
    public InitCostSpec WithUpgrade(int upgradedCost)
    {
        return this with { UpgradedCost = upgradedCost };
    }

    /// <summary>
    /// 从 <see cref="int"/> 隐式转换为 <see cref="InitCostSpec"/>，只设置初始能量消耗。
    /// </summary>
    /// <param name="initialCost">初始能量消耗。</param>
    public static implicit operator InitCostSpec(int initialCost)
    {
        return new InitCostSpec { InitialCost = initialCost };
    }
}

/// <summary>
/// 提供扩展方法，用于设置升级后的能量消耗。
/// </summary>
public static class InitCostSpecExtensions
{
    extension(int cardBaseCost)
    {
        /// <summary>
        /// 设置升级后的能量消耗。
        /// </summary>
        /// <param name="upgradedCost">升级后的能量消耗值。</param>
        /// <returns>包含升级信息的新 <see cref="InitCostSpec"/> 实例。</returns>
        public InitCostSpec CostWithUpgrade(int upgradedCost)
        {
            return new InitCostSpec
            {
                InitialCost = cardBaseCost
            };
        }
    }
}

/// <summary>
/// 实现此接口的卡牌将在升级时自动应用费用变化。
/// 由 <see cref="CardInitUtils.OnUpgrade"/> 自动处理。
/// </summary>
[AttachedTo(typeof(CardModel))]
public interface ICardWithSuperstitioCost
{
    /// <summary>
    /// 卡牌的初始费用配置，包含升级后的费用变化。
    /// </summary>
    CardInitMessage CardInitMessage { get; }
}