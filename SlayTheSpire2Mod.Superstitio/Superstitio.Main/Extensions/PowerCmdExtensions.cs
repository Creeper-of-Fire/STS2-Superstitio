using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace Superstitio.Main.Extensions;

/// <summary>
/// <see cref="PowerCmd"/> 的扩展方法
/// </summary>
public static class PowerCmdExtensions
{
    extension(PowerCmd)
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="card"></param>
        /// <param name="target"></param>
        /// <param name="amount"></param>
        /// <param name="silent"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static async Task<T?> ApplyByCard<T>(
            CardModel card,
            Creature target,
            decimal amount,
            bool silent = false
        ) where T : PowerModel
            => await PowerCmd.Apply<T>(
                target: target,
                amount: amount,
                applier: card.Owner.Creature,
                cardSource: card,
                silent: silent
            );
    }
}