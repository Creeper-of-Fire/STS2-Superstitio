using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using Superstitio.Main.DynamicVars.Extensions;

namespace Superstitio.Main.Extensions;

/// <summary>
/// 提供对 <see cref="CardPileCmd"/> 的扩展方法。
/// </summary>
public static class CardPileCmdExtensions
{
    extension(CardPileCmd)
    {
        /// <summary>
        /// 根据卡牌的动态变量自动抽牌
        /// </summary>
        /// <param name="choiceContext">玩家选择上下文</param>
        /// <param name="card">触发抽牌的卡牌模型</param>
        /// <param name="player">目标玩家，留空则默认为卡牌所有者</param>
        /// <returns>异步任务</returns>
        public static async Task AutoDraw(PlayerChoiceContext choiceContext, CardModel card, Player? player = null)
        {
            await CardPileCmd.Draw(choiceContext, card.DynamicVars.DrawCards.IntValue, player ?? card.Owner);
        }
    }
}