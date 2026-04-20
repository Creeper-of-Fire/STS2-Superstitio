using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using Superstitio.Api.Power;
using Superstitio.Main.Base;

namespace Superstitio.Main.Features.Corruptus;

/// <summary>
/// 
/// </summary>
public class CorruptusPower() : SuperstitioBasePower(new PowerInitMessage
{
    Type = PowerType.Buff,
    InitStackType = PowerInitMessage.StackStyle.Normal
})
{
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
        await ProcessingCorruptusDamage(choiceContext, this.Owner, power.Amount);
    }

    /// <summary>
    /// 处理腐朽造成的伤害
    /// </summary>
    /// <param name="choiceContext"></param>
    /// <param name="target"></param>
    /// <param name="corruptusmount"></param>
    public static async Task ProcessingCorruptusDamage(PlayerChoiceContext choiceContext, Creature target, int corruptusmount)
    {
        try
        {
            target.IsProcessingCorruptusDamage = true;

            // 对自身造成等同于腐朽层数的伤害
            await CreatureCmd.Damage(
                choiceContext,
                target,
                corruptusmount,
                ValueProp.Unblockable,
                target
            );

            // 伤害结算后移除腐朽效果
            await PowerCmd.Remove<CorruptusPower>(target);
        }
        finally
        {
            target.IsProcessingCorruptusDamage = false;
        }
    }
}