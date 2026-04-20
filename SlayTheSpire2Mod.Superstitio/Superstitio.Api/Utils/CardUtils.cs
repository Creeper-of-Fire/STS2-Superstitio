using MegaCrit.Sts2.Core.Models;
using Superstitio.Api.Extensions;

namespace Superstitio.Api.Utils;

/// <summary>
/// 
/// </summary>
public static class CardUtils
{
    /// <summary>
    /// 获得一张升级了多次的卡牌的克隆。
    /// </summary>
    /// <param name="cardType"></param>
    /// <param name="upgradeTimes"></param>
    /// <returns></returns>
    public static CardModel GetUpgradeCardClone(Type cardType, int upgradeTimes)
    {
        var upgradedCard = ModelDb.Card(cardType).ToMutableCard();

        for (int i = 0; i < upgradeTimes; i++)
        {
            upgradedCard.UpgradeInternal();
            upgradedCard.FinalizeUpgradeInternal();
        }

        return upgradedCard;
    }
}