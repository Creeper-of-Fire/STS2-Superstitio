using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using Superstitio.Main.SubPool.UI;

namespace Superstitio.Main.Patches.PatchCardFactory;

/// <summary>
/// 卡牌列表过滤器的钩子
/// </summary>
public class HookCardListFilter
{
    /// <summary>
    /// 自定义卡牌过滤逻辑
    /// </summary>
    /// <param name="player"></param>
    /// <param name="cards"></param>
    /// <returns></returns>
    public static IEnumerable<CardModel> FilterCardList(Player player, IEnumerable<CardModel> cards)
    {
        return cards.SubPoolCardListFilter(player);
    }
}