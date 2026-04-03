using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Superstitio.Main.Features.Corruptus;

/// <summary>
/// 定义拥有腐朽缓冲组件的实体接口。
/// </summary>
public interface ICorruptusBuffer
{
    /// <summary>
    /// 获取腐朽缓冲组件实例。
    /// </summary>
    public CorruptusBufferComponent CorruptusBufferComponent { get; }

    /// <summary>
    /// 获取拥有该缓冲的生物实体。
    /// </summary>
    public Creature OwnerCreature { get; }
}

/// <summary>
/// 腐朽缓冲组件。
/// 负责将生物受到的伤害转换为腐朽能力层数，并处理腐朽伤害的触发逻辑。
/// </summary>
public class CorruptusBufferComponent(ICorruptusBuffer corruptusBuffer)
{
    private ICorruptusBuffer CorruptusBuffer { get; } = corruptusBuffer;

    /// <summary>
    /// 标记是否正在处理腐朽伤害，防止递归触发。
    /// </summary>
    public bool IsProcessingCorruptusDamage { get; set; } = false;

    /// <summary>
    /// 获取拥有该组件的生物实体。
    /// </summary>
    private Creature OwnerCreature => this.CorruptusBuffer.OwnerCreature;

    private List<Func<Task>> PendingCorruptusTasks { get; set; } = [];
    
    /// <summary>
    /// 在伤害结算后期修改损失的生命值。
    /// 如果目标是拥有者且未在处理中，则将伤害量转换为腐朽层数，并抵消原伤害。
    /// </summary>
    /// <remarks>
    /// 推荐挂载点：和缓冲 <see cref="Buffer"/> 使用相同的钩子，即 <see cref="AbstractModel.ModifyHpLostAfterOstyLate"/>。
    /// </remarks>
    public decimal ModifyHpLostAfterOstyLate(
        Creature target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (this.IsProcessingCorruptusDamage)
            return amount;

        if (target != this.OwnerCreature)
            return amount;

        this.PendingCorruptusTasks.Add(async () => 
        {
            await CreatureCmd.TriggerAnim(target, "Hit", 0);
            
            await CorruptusManager.IncreaseCorruptus(
                target, 
                amount, 
                dealer, 
                cardSource
            );
        });

        return 0M;
    }
    
    /// <summary>
    /// 在伤害结算后期处理完成时，处理所有等待的腐朽应用任务。
    /// </summary>
    /// 推荐挂载点：和缓冲 <see cref="Buffer"/> 使用相同的钩子，即 <see cref="AbstractModel.AfterModifyingHpLostAfterOsty"/>。
    public async Task AfterModifyingHpLostAfterOsty()
    {
        foreach (var taskFunc in this.PendingCorruptusTasks)
        {
            await taskFunc();
        }
    
        this.PendingCorruptusTasks.Clear();
    }
}