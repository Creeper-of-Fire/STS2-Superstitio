using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Superstitio.Main.Features.Milk;

/// <summary>
/// 在下一次攻击后，给予被攻击者[blue]{Amount}[/blue]点临时生命。（目前以格挡代替）
/// TODO 临时生命还没有做好。或者也许召唤一个召唤物？
/// </summary>
public class MilkPower : CustomPowerModel
{
    /// <inheritdoc />
    public override PowerType Type => PowerType.Debuff;

    /// <inheritdoc />
    public override PowerStackType StackType => PowerStackType.Counter;

    /// <inheritdoc />
    public override bool IsInstanced => false;

    /// <inheritdoc />
    public override async Task AfterAttack(AttackCommand command)
    {
        if (command.Attacker != this.Owner)
            return;
        foreach (var result in command.Results)
        {
            await CreatureCmd.GainBlock(result.Receiver, this.Amount, ValueProp.Unpowered, null, fast: true);
        }
    }
}