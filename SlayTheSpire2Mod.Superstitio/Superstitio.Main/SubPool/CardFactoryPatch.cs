using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;
using static Superstitio.Main.SubPool.UI.SubPoolManager;

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
// ReSharper disable InconsistentNaming

namespace Superstitio.Main.SubPool;

[HarmonyPatch(typeof(CardFactory))]
public static class CardFactoryPatch
{
    // 1. 卡牌奖励
    // 这个 Patch 不再使用，我们直接 Patch CardCreationOptions.GetPossibleCards
    // [HarmonyPrefix]
    // [HarmonyPatch(nameof(CardFactory.CreateForReward),
    //     new[] { typeof(Player), typeof(IEnumerable<CardModel>), typeof(CardCreationOptions) })]
    // public static void CreateForRewardPrefix(Player player, ref CardCreationOptions options)
    // {
    //     var allCards = options.GetPossibleCards(player);
    //     var filtered = FilterCardList(player, allCards);
    //     options = options.WithCustomPool(filtered);
    // }

    // 2. 商店（CardType 重载）
    [HarmonyPrefix]
    [HarmonyPatch(nameof(CardFactory.CreateForMerchant),
        new[] { typeof(Player), typeof(IEnumerable<CardModel>), typeof(CardType) })]
    public static void CreateForMerchantTypePrefix(Player player, ref IEnumerable<CardModel> options)
    {
        options = FilterCardList(player, options);
    }

    // 3. 商店（CardRarity 重载）
    [HarmonyPrefix]
    [HarmonyPatch(nameof(CardFactory.CreateForMerchant),
        new[] { typeof(Player), typeof(IEnumerable<CardModel>), typeof(CardRarity) })]
    public static void CreateForMerchantRarityPrefix(Player player, ref IEnumerable<CardModel> options)
    {
        options = FilterCardList(player, options);
    }

    // 4. 战斗生成（不重复）
    [HarmonyPrefix]
    [HarmonyPatch(nameof(CardFactory.GetDistinctForCombat))]
    public static void GetDistinctForCombatPrefix(Player player, ref IEnumerable<CardModel> cards)
    {
        cards = FilterCardList(player, cards);
    }

    // 5. 战斗生成（可重复）
    [HarmonyPrefix]
    [HarmonyPatch(nameof(CardFactory.GetForCombat))]
    public static void GetForCombatPrefix(Player player, ref IEnumerable<CardModel> cards)
    {
        cards = FilterCardList(player, cards);
    }

    // 6. 卡牌转换（带 options 重载）
    [HarmonyPrefix]
    [HarmonyPatch(nameof(CardFactory.CreateRandomCardForTransform),
        new[] { typeof(CardModel), typeof(IEnumerable<CardModel>), typeof(bool), typeof(Rng) })]
    public static void CreateRandomCardForTransformPrefix(CardModel original, ref IEnumerable<CardModel> options)
    {
        options = FilterCardList(original.Owner, options);
    }

    // 7. 获取转换选项
    [HarmonyPostfix]
    [HarmonyPatch(nameof(CardFactory.GetDefaultTransformationOptions))]
    public static void GetDefaultTransformationOptionsPostfix(
        CardModel original,
        bool isInCombat,
        ref IEnumerable<CardModel> __result)
    {
        __result = FilterCardList(original.Owner, __result);
    }
}

[HarmonyPatch(typeof(CardCreationOptions))]
public static class CardCreationOptionsPatch
{
    // 拦截 GetPossibleCards，直接过滤返回结果
    [HarmonyPostfix]
    [HarmonyPatch(nameof(CardCreationOptions.GetPossibleCards))]
    public static void GetPossibleCardsPostfix(
        CardCreationOptions __instance,
        Player player,
        ref IEnumerable<CardModel> __result)
    {
        // 过滤卡牌
        __result = FilterCardList(player, __result);
    }
}