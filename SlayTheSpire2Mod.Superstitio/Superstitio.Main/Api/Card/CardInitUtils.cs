using MegaCrit.Sts2.Core.Models;

namespace Superstitio.Api.Card;

/// <summary>
/// 卡牌初始化工具类，提供统一的升级处理逻辑。
/// </summary>
public static class CardInitUtils
{
    /// <summary>
    /// 统一处理卡牌升级逻辑。
    /// 自动应用费用变化、动态变量数值变化以及关键字的添加/移除。
    /// </summary>
    /// <param name="card">正在升级的卡牌实例。</param>
    public static void OnUpgrade(CardModel card)
    {
        ApplyCostUpgrade(card);
        ApplyDynamicVarUpgrade(card);
        ApplyKeywordUpgrade(card);
    }

    /// <summary>
    /// 应用费用升级。
    /// </summary>
    private static void ApplyCostUpgrade(CardModel card)
    {
        if (card is not ICardWithSuperstitioCost cardWithCost)
            return;

        int upgradedCost = cardWithCost.CardInitMessage.BaseCost.UpgradedCost;
        if (upgradedCost != 0)
        {
            card.EnergyCost.UpgradeBy(upgradedCost);
        }
    }

    /// <summary>
    /// 应用动态变量的数值升级。
    /// </summary>
    private static void ApplyDynamicVarUpgrade(CardModel card)
    {
        if (card is not ICardWithDynamicVarSpecs cardWithVars)
            return;

        foreach (var varSpec in cardWithVars.InitVarsWithUpgrade)
        {
            if (!card.DynamicVars.TryGetValue(varSpec.DynamicVar.Name, out var dynamicVar))
                continue;

            if (varSpec.UpgradeValue != 0)
            {
                dynamicVar.UpgradeValueBy(varSpec.UpgradeValue);
            }
        }
    }

    /// <summary>
    /// 应用关键字的升级（添加或移除）。
    /// </summary>
    private static void ApplyKeywordUpgrade(CardModel card)
    {
        if (card is not ICardWithCardKeywordSpecs cardWithKeywords)
            return;

        foreach (var keywordSpec in cardWithKeywords.InitCardKeywords)
        {
            switch (keywordSpec.UpgradeBehavior)
            {
                case CardKeywordSpec.AddType.RemoveAfterUpgrade:
                    card.RemoveKeyword(keywordSpec);
                    break;

                case CardKeywordSpec.AddType.OnlyAddAfterUpgrade:
                    card.AddKeyword(keywordSpec);
                    break;

                case CardKeywordSpec.AddType.Normal:
                default:
                    // 普通类型无需处理
                    break;
            }
        }
    }
}