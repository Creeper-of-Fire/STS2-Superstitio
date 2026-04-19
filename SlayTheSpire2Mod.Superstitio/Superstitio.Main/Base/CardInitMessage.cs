using MegaCrit.Sts2.Core.Entities.Cards;

namespace Superstitio.Main.Base;

/// <summary>
/// 用于记录卡牌能量消耗及其升级变化的消息类型。
/// </summary>
public record CostMessage
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
    /// <returns>包含升级信息的新 CostMessage 实例。</returns>
    public CostMessage WithUpgrade(int upgradedCost)
    {
        return this with { UpgradedCost = upgradedCost };
    }

    /// <summary>
    /// 从 int 隐式转换为 CostMessage，只设置初始能量消耗。
    /// </summary>
    /// <param name="initialCost">初始能量消耗。</param>
    public static implicit operator CostMessage(int initialCost)
    {
        return new CostMessage { InitialCost = initialCost };
    }
}

/// <summary>
/// 提供扩展方法，用于设置升级后的能量消耗。
/// </summary>
public static class CostMessageExtensions
{
    extension(int cardBaseCost)
    {
        /// <summary>
        /// 设置升级后的能量消耗。
        /// </summary>
        /// <param name="upgradedCost">升级后的能量消耗值。</param>
        /// <returns>包含升级信息的新 CostMessage 实例。</returns>
        public CostMessage CostWithUpgrade(int upgradedCost)
        {
            return new CostMessage
            {
                InitialCost = cardBaseCost
            };
        }
    }
}

/// <summary>
/// 用于初始化卡牌基本属性的记录类型。
/// </summary>
public record CardInitMessage
{
    /// <summary>
    /// 卡牌的基础费用。
    /// </summary>
    public required CostMessage BaseCost { get; init; }

    /// <summary>
    /// 卡牌的类型。
    /// </summary>
    public required CardType Type { get; init; }

    /// <summary>
    /// 卡牌的稀有度。
    /// </summary>
    public required CardRarity Rarity { get; init; }

    /// <summary>
    /// 卡牌的使用目标类型。
    /// </summary>
    public required TargetType Target { get; init; }

    /// <summary>
    /// 指示该卡牌是否显示在卡牌库中。
    /// </summary>
    public bool ShowInCardLibrary { get; init; } = true;
}