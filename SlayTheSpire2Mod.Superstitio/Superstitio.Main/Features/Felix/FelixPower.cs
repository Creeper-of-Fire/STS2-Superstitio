using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using Superstitio.Main.Base;

namespace Superstitio.Main.Features.Felix;

/// <summary>
/// 快感
/// </summary>
public class FelixPower() : SuperstitioBasePower(new PowerInitMessage
{
    Type = PowerType.Buff,
    InitStackType = PowerInitMessage.StackStyle.Normal
})
{
    private const int AngerThreshold = 10;

    private int MaxThresholdReachedThisTurn { get; set; } = 0;

    /// <inheritdoc />
    public override async Task AfterPowerAmountChanged(PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power is not FelixPower felixPower || felixPower != this)
            return;

        // 1. 计算当前层数代表的理论触发次数 (例如 25层 = 2次)
        int currentTotalMilestones = Math.Max(0, this.Amount) / AngerThreshold;

        // 2. 计算需要新触发的次数
        // 如果当前里程碑 大于 本回合记录的最高里程碑，说明跨过了新的10层
        int triggersNeeded = currentTotalMilestones - this.MaxThresholdReachedThisTurn;

        if (triggersNeeded > 0)
        {
            // 立即更新高水位线，防止在异步等待期间重复触发
            this.MaxThresholdReachedThisTurn = currentTotalMilestones;

            // 3. 使用 for 循环进行有限次数的触发
            // for 循环的次数在进入时已经确定，不存在死循环风险
            for (int i = 0; i < triggersNeeded; i++)
            {
                await this.TriggerAngry(this, applier, cardSource);
            }
        }

        // 注意：这里我们故意【不】在 Amount 降低时减少 TriggeredCount
        // 这就完美避免了 45 -> 50 (触发第5次) -> 45 (失去5点) -> 50 (再次达到50) 时的重复触发。
        // 玩家想要触发第6次，就必须实打实地达到 60 点。
    }

    /// <summary>
    /// 触发快感状态的方法
    /// </summary>
    private async Task TriggerAngry(FelixPower felixPower, Creature? applier, CardModel? cardSource)
    {
        var runState = this.Owner.Player?.RunState;
        var combatState = this.Owner.CombatState;
        if (runState is null || combatState is null)
            return;
        // 调用达到顶峰后的钩子方法
        await Hook.AfterClimaxReached(
            runState: runState,
            combatState: combatState,
            powerOwner: this.Owner,
            felixPower: felixPower,
            applier: applier,
            cardSource: cardSource
        );
        // 可以在这里添加视觉和音效反馈
        if (this.Owner.IsPlayer)
        {
            // 触发特效
        }
    }

    /// <summary>
    /// 回合结束时触发
    /// 注意：具体重写的方法名(OnTurnEnd / OnEndOfTurn)需根据 Sts2.Core 的实际 API 调整
    /// </summary>
    /// <inheritdoc />
    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side != this.Owner.Side)
            return;

        this.MaxThresholdReachedThisTurn = 0;

        // 回合结束失去所有快感
        await PowerCmd.Remove(this);
    }
}

/// <summary>
/// 达到顶峰后事件接口
/// 允许监听者在角色达到顶峰时执行自定义逻辑
/// </summary>
public interface IAfterClimaxReached
{
    /// <summary>
    /// 当达到顶峰时调用
    /// </summary>
    /// <param name="powerOwner">快感Buff的所有者</param>
    /// <param name="felixPower">Buff自身</param>
    /// <param name="applier"></param>
    /// <param name="cardSource"></param>
    /// <returns></returns>
    Task AfterClimaxReached(Creature powerOwner, FelixPower felixPower, Creature? applier, CardModel? cardSource);
}

/// <summary>
/// 钩子扩展类 - 提供达到顶峰后的事件分发功能
/// </summary>
public static class HookExtension
{
    extension(Hook)
    {
        /// <summary>
        /// 分发达到顶峰事件给所有监听者
        /// </summary>
        public static async Task AfterClimaxReached(IRunState runState, CombatState? combatState, Creature powerOwner,
            FelixPower felixPower, Creature? applier, CardModel? cardSource)
        {
            foreach (var model in runState.IterateHookListeners(combatState))
            {
                if (model is not IAfterClimaxReached thresholdReachedModel)
                    continue;
                await PowerCmd.Apply<ClimaxRecordPower>(powerOwner, 1, applier, cardSource);
                await thresholdReachedModel.AfterClimaxReached(powerOwner, felixPower, applier, cardSource);
                model.InvokeExecutionFinished();
            }
        }
    }
}

/// <summary>
/// 顶峰记录用 Power
/// </summary>
public class ClimaxRecordPower() : SuperstitioBasePower(new PowerInitMessage
{
    Type = PowerType.Buff,
    InitStackType = PowerInitMessage.StackStyle.Normal
})
{
    /// <inheritdoc />
    protected override bool IsVisibleInternal => false;
}

/// <summary>
/// 快感管理器（占位符）
/// </summary>
public static class FelixManager
{
    /// <summary>
    /// 修改快感值
    /// </summary>
    /// <param name="target">目标角色</param>
    /// <param name="amount">变化的快感值（正数增加，负数减少）</param>
    /// <param name="sourceCreature">来源生物（可选）</param>
    /// <param name="cardReason">导致变化的卡牌（可选）</param>
    /// <returns></returns>
    public static async Task ModifyFelix(
        Creature target,
        decimal amount,
        Creature? sourceCreature = null,
        CardModel? cardReason = null)
    {
        if (amount == 0)
            return;

        // 更新快感值
        await PowerCmd.Apply<FelixPower>(
            target,
            amount,
            sourceCreature,
            cardReason
        );
    }

    /// <summary>
    /// 获取目标的顶峰记录
    /// </summary>
    /// <param name="target">目标生物</param>
    /// <returns>包含其所有顶峰记录的 enumerable，可进一步过滤/处理</returns>
    public static IEnumerable<PowerReceivedEntry> GetClimaxRecord(Creature target)
    {
        return CombatManager.Instance.History.Entries.OfType<PowerReceivedEntry>()
            .Where(it => it.Power is ClimaxRecordPower && it.Actor == target);
    }
}