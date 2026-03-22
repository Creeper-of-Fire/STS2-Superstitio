using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using Superstitio.Main.Maso.Pools;
using Superstitio.Main.SubPool.UI;
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释

namespace Superstitio.Main.SubPool;

[HarmonyPatch(typeof(CardFactory))]
public static class CardFactoryPatch
{

    // 1. 卡牌奖励
    [HarmonyPrefix]
    [HarmonyPatch(nameof(CardFactory.CreateForReward), typeof(Player), typeof(IEnumerable<CardModel>), typeof(CardCreationOptions))]
    public static void CreateForRewardPrefix(Player player, ref CardCreationOptions options)
    {
        var allCards = options.GetPossibleCards(player);
        var filtered = FilterCardList(player, allCards);
        options = options.WithCustomPool(filtered);
    }

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

    // 统一过滤逻辑
    private static IEnumerable<CardModel> FilterCardList(Player player, IEnumerable<CardModel> cards)
    {
        // 获取玩家启用的子池（可以从遗物获取，也可以从其他配置）
        var relic = player.Relics.FirstOrDefault(r => r is IHoldCardPoolSelection) as IHoldCardPoolSelection;
        if (relic == null) return cards;

        // 获取启用的子池ID列表
        var enabledSubPoolIds = relic.SelectedSubPoolIds;
        
        // 过滤卡牌：只要卡牌属于任意一个启用的子池
        return cards.Where(c => IsInAnySelectedPool(c, enabledSubPoolIds));
    }

    private static bool IsInAnySelectedPool(CardModel card, IEnumerable<string> subPoolIds)
    {
        var canonicalCard = card.CanonicalInstance;
    
        foreach (var poolId in subPoolIds)
        {
            var subPool = SubPoolManager.GetSubPoolById(poolId);
            if (subPool != null && subPool.PoolCards.Any(pc => pc.CanonicalInstance == canonicalCard))
                return true;
        }
    
        return false;
    }
}

