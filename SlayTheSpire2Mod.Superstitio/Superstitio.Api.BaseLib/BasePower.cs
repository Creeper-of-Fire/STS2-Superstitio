using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Powers;
using Superstitio.Api.Power;

namespace Superstitio.Api.BaseLib;

/// <summary>
/// 基础的 Power
/// </summary>
/// <param name="powerInitMessage"></param>
public abstract class BasePower(PowerInitMessage powerInitMessage) : CustomPowerModel
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
}