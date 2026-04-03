using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace Superstitio.Main.Features.Corruptus;

/// <summary>
/// 腐朽效果管理器，提供增加和减少腐朽层数的方法。
/// </summary>
public static class CorruptusManager
{
    /// <summary>
    /// 为目标生物增加指定的腐朽层数。
    /// </summary>
    /// <param name="target">目标生物。</param>
    /// <param name="amount">要增加的腐朽层数。</param>
    /// <param name="applier">施加腐朽效果的来源生物（可选）。</param>
    /// <param name="cardSource">来源卡牌（可选）。</param>
    /// <param name="silent">如果为 true，则不显示视觉效果或日志。</param>
    public static async Task<CorruptusPower?> IncreaseCorruptus(
        Creature target,
        decimal amount,
        Creature? applier,
        CardModel? cardSource,
        bool silent = false)
    {
        if (amount <= 0)
            return null;

        return await PowerCmd.Apply<CorruptusPower>(
            target,
            amount,
            applier,
            cardSource,
            silent
        );
    }

    /// <summary>
    /// 为目标生物减少指定的腐朽层数。
    /// </summary>
    /// <param name="target">目标生物。</param>
    /// <param name="amount">要减少的腐朽层数。</param>
    /// <param name="remover">移除腐朽效果的来源生物（可选）。</param>
    /// <param name="cardSource">来源卡牌（可选）。</param>
    /// <param name="silent">如果为 true，则不显示视觉效果或日志。</param>
    public static async Task<CorruptusPower?> DecreaseCorruptus(
        Creature target,
        decimal amount,
        Creature? remover,
        CardModel? cardSource,
        bool silent = false)
    {
        if (amount <= 0)
            return null;

        return await PowerCmd.Apply<CorruptusPower>(
            target,
            -amount,
            remover,
            cardSource,
            silent
        );
    }
}