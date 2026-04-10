using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using Superstitio.Main.DynamicVars.Extensions;

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
        public static async Task<T?> DecreByCard<T>(
            CardModel card,
            Creature target,
            decimal? amount = null,
            bool silent = false
        ) where T : PowerModel
        {
            return await PowerCmd.Apply<T>(
                target: target,
                amount: -(amount ?? card.DynamicVars.GetVarOrThrow(typeof(T).Name).BaseValue),
                applier: card.Owner.Creature,
                cardSource: card,
                silent: silent
            );
        }

        /// <summary>
        /// 
        /// </summary>
        public static async Task<T?> ApplyByCard<T>(
            CardModel card,
            Creature target,
            decimal? amount = null,
            bool silent = false
        ) where T : PowerModel
        {
            return await PowerCmd.Apply<T>(
                target: target,
                amount: amount ?? card.DynamicVars.GetVarOrThrow(typeof(T).Name).BaseValue,
                applier: card.Owner.Creature,
                cardSource: card,
                silent: silent
            );
        }
    }
}