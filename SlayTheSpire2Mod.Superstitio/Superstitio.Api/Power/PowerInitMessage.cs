using MegaCrit.Sts2.Core.Entities.Powers;

namespace Superstitio.Api.Power;

/// <summary>
/// 用于初始化能力基本属性的记录类型
/// </summary>
public record PowerInitMessage
{
    /// <summary>
    /// 定义能力实例的初始化堆叠风格
    /// 决定了能力在多次施加时的实例化行为和UI显示方式
    /// </summary>
    public enum StackStyle
    {
        /// <summary>
        /// 可堆叠、堆叠显示
        /// 行为：多次施加时合并到同一个实例，Amount数值累加，UI显示累计数值
        /// </summary>
        Normal,

        /// <summary>
        /// 显示数量，但每次应用都产生不同的实例
        /// 行为：每次施加创建独立的实例，每个实例有自己的Amount数值，UI分别显示各自的数值
        /// 适用：多个独立来源的效果
        /// </summary>
        MultiCounter,

        /// <summary>
        /// 不可堆叠，不显示数量，不可重复应用
        /// 行为：只能存在一个实例，不显示Amount数值
        /// 适用：某些唯一的Buff/Debuff
        /// </summary>
        Singular,

        /// <summary>
        /// 不显示数量，每次应用都产生不同的实例
        /// 行为：每次施加创建独立的实例，但不显示Amount数值
        /// 适用：多个独立来源的光环效果，每个独立生效但不需要显示层数
        /// </summary>
        MultiSingular
    }

    /// <summary></summary>
    public required PowerType Type { get; init; }

    /// <summary></summary>
    public required StackStyle InitStackType { get; init; } = StackStyle.Normal;

    /// <summary>
    /// 叠加类型，Counter表示可叠加，Single表示不可叠加
    /// </summary>
    public PowerStackType StackType => this.InitStackType switch
    {
        StackStyle.MultiCounter or StackStyle.Normal => PowerStackType.Counter,
        _ => PowerStackType.Single
    };

    /// <summary>
    /// 是否每个都独立显示
    /// </summary>
    public bool IsInstanced => this.InitStackType switch
    {
        StackStyle.MultiCounter or StackStyle.MultiSingular => true,
        _ => false
    };
}