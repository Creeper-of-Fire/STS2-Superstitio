using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace Superstitio.Main.Features.Milk;

/// <summary>
/// 在下一次攻击后，给予玩家（施加者） %d 点 #y临时生命 。
/// 或者也许召唤一个召唤物？
/// TODO 临时生命还没有做好，目前没有效果。
/// </summary>
public class MilkPower : CustomPowerModel
{
    /// <inheritdoc />
    public override PowerType Type => PowerType.Buff;

    /// <inheritdoc />
    public override PowerStackType StackType => PowerStackType.Counter;

    /// <inheritdoc />
    public override bool IsInstanced => false;
}