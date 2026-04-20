using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Superstitio.Analyzer;
using Superstitio.Api.Extensions;

namespace Superstitio.Main.Features.Corruptus;

/// <summary>
/// 定义拥有腐朽缓冲组件的实体接口。
/// </summary>
[AttachedTo(typeof(AbstractModel))]
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
/// 用于在 <see cref="Creature"/> 上附加临时标记的扩展字段。
/// </summary>
internal static class CreatureCorruptusMarkers
{
    /// <summary>
    /// 用于标记特定生物是否正在处理腐朽伤害过程中的字段。
    /// </summary>
    private static readonly WeekField<Creature, bool> IsProcessingCorruptusDamageField = new(() => false);

    extension(Creature creature)
    {
        /// <summary>
        /// 获取/设置该生物当前是否正在处理腐朽伤害（用于防止其他 CorruptusBuffer 阻拦）。
        /// </summary>
        public bool IsProcessingCorruptusDamage
        {
            get => IsProcessingCorruptusDamageField.Get(creature);
            set => IsProcessingCorruptusDamageField.Set(creature, value);
        }
    }
}

/// <summary>
/// 腐朽缓冲组件。
/// 负责将生物受到的伤害转换为腐朽能力层数，并处理腐朽伤害的触发逻辑。
/// </summary>
public class CorruptusBufferComponent(ICorruptusBuffer corruptusBuffer)
{
    private ICorruptusBuffer CorruptusBuffer { get; } = corruptusBuffer;


    /// <summary>
    /// 获取拥有该组件的生物实体。
    /// </summary>
    private Creature OwnerCreature => this.CorruptusBuffer.OwnerCreature;

    private List<Func<Task>> PendingCorruptusTasks { get; set; } = [];

    /// <summary>
    /// 携带延迟任务队列的 decimal 包装类型。
    /// 用于在返回数值的同时，向任务队列中添加延迟执行的操作。
    /// </summary>
    /// <param name="Value">返回的数值。</param>
    /// <param name="PendingTasks">待处理任务队列的引用。</param>
    public record DecimalWithTask(decimal Value, List<Func<Task>>? PendingTasks)
    {
        /// <summary>
        /// 从 DecimalWithTask 隐式转换为 decimal。
        /// </summary>
        public static implicit operator decimal(DecimalWithTask self) => self.Value;

        /// <summary>
        /// 向关联的任务队列中添加异步延迟任务。
        /// 如果没有关联的任务队列，则什么都不做。
        /// </summary>
        /// <param name="task">要添加的异步任务。</param>
        /// <returns>返回自身，方便链式调用。</returns>
        public DecimalWithTask AfterModify(Func<Task> task)
        {
            this.PendingTasks?.Add(task);
            return this;
        }

        /// <summary>
        /// 向关联的任务队列中添加同步延迟任务。
        /// </summary>
        /// <param name="action">要执行的同步操作。</param>
        /// <returns>返回自身，方便链式调用。</returns>
        public DecimalWithTask AfterModify(Action action) => this.AfterModify(() =>
        {
            action();
            return Task.CompletedTask;
        });
    }

    /// <summary>
    /// 在伤害结算后期修改损失的生命值。
    /// 如果目标是拥有者且未在处理中，则将伤害量转换为腐朽层数，并抵消原伤害。
    /// </summary>
    /// <remarks>
    /// 推荐挂载点：和缓冲 <see cref="Buffer"/> 使用相同的钩子，即 <see cref="AbstractModel.ModifyHpLostAfterOstyLate"/>。
    /// </remarks>
    public DecimalWithTask ModifyHpLostAfterOstyLate(
        Creature target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource
    )
    {
        if (target != this.OwnerCreature || target.IsProcessingCorruptusDamage)
            return new DecimalWithTask(amount, null);

        return new DecimalWithTask(0M, this.PendingCorruptusTasks).AfterModify(async () =>
        {
            await CreatureCmd.TriggerAnim(target, "Hit", 0);

            await CorruptusManager.IncreaseCorruptus(
                target,
                amount,
                dealer,
                cardSource
            );
        });
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