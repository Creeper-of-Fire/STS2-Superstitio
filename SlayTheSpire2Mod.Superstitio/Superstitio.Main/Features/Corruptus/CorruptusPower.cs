using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace Superstitio.Main.Features.Corruptus;

/// <summary>
/// TODO 腐朽占位符能力（临时）
/// </summary>
public class CorruptusPower : CustomPowerModel
{
    /// <inheritdoc />
    public override PowerType Type => PowerType.Buff;

    /// <inheritdoc />
    public override PowerStackType StackType => PowerStackType.Counter;

    /// <inheritdoc />
    public override bool IsInstanced => false;
}