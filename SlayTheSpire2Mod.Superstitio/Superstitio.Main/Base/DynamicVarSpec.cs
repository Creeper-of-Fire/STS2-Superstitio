using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using static MegaCrit.Sts2.Core.HoverTips.HoverTipFactory;

namespace Superstitio.Main.Base;

/// <summary>
/// 表示一个带有升级值的动态变量记录。
/// </summary>
public record DynamicVarSpec(DynamicVar DynamicVar)
{
    /// <summary>
    /// 动态变量。
    /// </summary>
    public DynamicVar DynamicVar { get; } = DynamicVar;

    /// <summary>
    /// 升级值。
    /// </summary>
    public decimal UpgradeValue { get; init; } = 0;

    /// <summary>
    /// 额外的提示内容。
    /// </summary>
    public IEnumerable<IHoverTip> ExtraHoverTips { get; init; } = [];

    /// <summary>
    /// 从 <see cref="DynamicVar"/> 隐式转换为 <see cref="DynamicVarSpec"/>，升级值为 0。
    /// </summary>
    /// <param name="dynamicVar">动态变量。</param>
    public static implicit operator DynamicVarSpec(DynamicVar dynamicVar) =>
        new(dynamicVar) { UpgradeValue = 0, ExtraHoverTips = [] };
}

/// <summary>
/// 为 <see cref="DynamicVar"/> 提供扩展方法。
/// </summary>
public static class DynamicVarExtensions
{
    extension(DynamicVar dynamicVar)
    {
        /// <summary>
        /// 创建一个带有指定升级值的 <see cref="DynamicVarSpec"/> 实例。
        /// </summary>
        public DynamicVarSpec WithUpgrade(decimal upgradeValue)
        {
            return new DynamicVarSpec(dynamicVar) { UpgradeValue = upgradeValue, ExtraHoverTips = [] };
        }

        /// <summary>
        /// 创建一个带有指定提示的 <see cref="DynamicVarSpec"/> 实例。
        /// </summary>
        public DynamicVarSpec AddToolTips(IEnumerable<IHoverTip> extraHoverTips)
        {
            return new DynamicVarSpec(dynamicVar) { UpgradeValue = 0, ExtraHoverTips = extraHoverTips };
        }
        
        /// <summary>
        /// 创建一个带有指定提示的 <see cref="DynamicVarSpec"/> 实例。
        /// </summary>
        public DynamicVarSpec AddToolTips(IHoverTip extraHoverTip)
        {
            return dynamicVar.AddToolTips([extraHoverTip]);
        }

        /// <summary>
        /// 创建一个带有指定提示的 <see cref="DynamicVarSpec"/> 实例。
        /// </summary>
        public DynamicVarSpec AddToolTips<TPower>() where TPower : PowerModel
        {
            return new DynamicVarSpec(dynamicVar) { ExtraHoverTips = [FromPower<TPower>()] };
        }
    }
    
    extension(EnergyVar dynamicVar)
    {
        /// <summary>
        /// 创建一个带有指定提示的 <see cref="DynamicVarSpec"/> 实例。
        /// </summary>
        public DynamicVarSpec AddToolTips(CardModel card)
        {
            return new DynamicVarSpec(dynamicVar) { UpgradeValue = 0, ExtraHoverTips = [ForEnergy(card)] };
        }
    }

    extension<TPower>(PowerVar<TPower> dynamicVar) where TPower : PowerModel
    {
        /// <summary>
        /// 创建一个带有指定提示的 <see cref="DynamicVarSpec"/> 实例。
        /// </summary>
        public DynamicVarSpec AddToolTips()
        {
            return new DynamicVarSpec(dynamicVar) { UpgradeValue = 0, ExtraHoverTips = [FromPower<TPower>()] };
        }
    }

    extension(DynamicVarSpec varSpec)
    {
        /// <summary>
        /// 创建一个带有指定升级值的 <see cref="DynamicVarSpec"/> 实例。
        /// </summary>
        public DynamicVarSpec WithUpgrade(decimal upgradeValue)
        {
            return varSpec with { UpgradeValue = upgradeValue };
        }

        /// <summary>
        /// 创建一个带有指定提示的 <see cref="DynamicVarSpec"/> 实例。
        /// </summary>
        public DynamicVarSpec AddToolTips(IEnumerable<IHoverTip> extraHoverTips)
        {
            return varSpec with { ExtraHoverTips = [..varSpec.ExtraHoverTips, ..extraHoverTips] };
        }

        /// <summary>
        /// 创建一个带有指定提示的 <see cref="DynamicVarSpec"/> 实例。
        /// </summary>
        public DynamicVarSpec AddToolTips(IHoverTip extraHoverTip)
        {
            return varSpec.AddToolTips([extraHoverTip]);
        }
        

        /// <summary>
        /// 创建一个带有指定提示的 <see cref="DynamicVarSpec"/> 实例。
        /// </summary>
        public DynamicVarSpec AddToolTips<TPower>() where TPower : PowerModel
        {
            return varSpec with { ExtraHoverTips = [..varSpec.ExtraHoverTips, FromPower<TPower>()] };
        }
    }
}