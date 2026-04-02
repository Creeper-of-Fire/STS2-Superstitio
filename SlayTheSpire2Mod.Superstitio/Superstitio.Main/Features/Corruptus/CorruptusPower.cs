using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

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

    /// <inheritdoc />
    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side != this.Owner.Side)
            return;

        await this.TriggerCorruptusDamage(choiceContext);
    }

    /// <summary>
    /// 触发腐朽伤害。
    /// 对拥有者造成等同于当前腐朽层数的不可阻挡伤害，随后移除腐朽效果。
    /// </summary>
    private async Task TriggerCorruptusDamage(PlayerChoiceContext choiceContext)
    {
        var power = this.Owner.GetPower<CorruptusPower>();
        if (power is not { Amount: > 0 })
            return;

        var allCorruptusBufferComponents = this.CombatState.IterateHookListeners()
            .OfType<ICorruptusBuffer>()
            .Select(buffer => buffer.CorruptusBufferComponent)
            .ToHashSet();

        try
        {
            foreach (var it in allCorruptusBufferComponents)
                it.IsProcessingCorruptusDamage = true;

            // 对自身造成等同于腐朽层数的伤害
            await CreatureCmd.Damage(
                choiceContext,
                this.Owner,
                power.Amount,
                ValueProp.Unblockable,
                this.Owner
            );

            // 伤害结算后移除腐朽效果
            await PowerCmd.Remove<CorruptusPower>(this.Owner);
        }
        finally
        {
            foreach (var it in allCorruptusBufferComponents)
                it.IsProcessingCorruptusDamage = false;
        }
    }
}