using System.Reflection;
using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Entities.Powers;
using Superstitio.Main.Utils;

namespace Superstitio.Main.Base;


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
}

/// <summary>
/// 
/// </summary>
public abstract class SuperstitioBasePower(PowerInitMessage powerInitMessage) : CustomPowerModel
{
    /// <summary>
    /// Buff或Debuff
    /// </summary>
    public override PowerType Type { get; } = powerInitMessage.Type;

    /// <summary>
    /// 叠加类型，Counter表示可叠加，Single表示不可叠加
    /// </summary>
    public override PowerStackType StackType { get; } = powerInitMessage.InitStackType switch
    {
        PowerInitMessage.StackStyle.MultiCounter or PowerInitMessage.StackStyle.Normal => PowerStackType.Counter,
        _ => PowerStackType.Single
    };

    /// <summary>
    /// 是否每个都独立显示
    /// </summary>
    public override bool IsInstanced { get; } = powerInitMessage.InitStackType switch
    {
        PowerInitMessage.StackStyle.MultiCounter or PowerInitMessage.StackStyle.MultiSingular => true,
        _ => false
    };

    private string GetPowerPortraitPath()
    {
        var type = this.GetType();
        string imgPrefix = ResourceUtils.GetImgPrefix(type);

        // 获取自定义名称逻辑
        var nameAttr = type.GetCustomAttribute<CustomImgNameAttribute>();
        string fileName = nameAttr != null ? nameAttr.Name : type.Name;

        string path = $"{imgPrefix}/powers/{fileName}.png";

        if (ResourceLoader.Exists(path))
            return path;

        string defaultImg = $"{imgPrefix}/powers/default.png";

        return defaultImg;
    }

    /// <inheritdoc />
    public override string CustomBigIconPath => this.GetPowerPortraitPath();

    /// <inheritdoc />
    public override string CustomPackedIconPath => this.GetPowerPortraitPath();
}