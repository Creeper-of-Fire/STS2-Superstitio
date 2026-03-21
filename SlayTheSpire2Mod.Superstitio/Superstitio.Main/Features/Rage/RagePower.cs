using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;

namespace Superstitio.Main.Features.Rage;

/// <summary>
/// TODO 怒火占位符能力（临时）
/// </summary>
public class RagePower : CustomPowerModel
{
    /// <inheritdoc />
    public override PowerType Type => PowerType.Buff;

    /// <inheritdoc />
    public override PowerStackType StackType => PowerStackType.Counter;

    /// <inheritdoc />
    public override bool IsInstanced => false;
}

/// <summary>
/// 怒火管理器（占位符）
/// </summary>
public static class RageManager
{
    /// <summary>
    /// 修改怒火值
    /// </summary>
    /// <param name="target">目标角色</param>
    /// <param name="amount">变化的怒火值（正数增加，负数减少）</param>
    /// <param name="sourceCreature">来源生物（可选）</param>
    /// <param name="cardReason">导致变化的卡牌（可选）</param>
    /// <returns></returns>
    public static async Task ModifyRage(
        Creature target,
        decimal amount,
        Creature? sourceCreature = null,
        CardModel? cardReason = null)
    {
        if (amount == 0)
            return;

        // 更新怒火值
        await PowerCmd.Apply<RagePower>(
            target,
            amount,
            sourceCreature,
            cardReason
        );
    }
}