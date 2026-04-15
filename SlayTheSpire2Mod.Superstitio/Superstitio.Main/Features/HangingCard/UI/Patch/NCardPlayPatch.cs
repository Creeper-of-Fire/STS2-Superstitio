using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace Superstitio.Main.Features.HangingCard.UI.Patch;

/// <summary>
/// 捕获卡牌瞄准状态
/// </summary>
public static class HangingSelectionBridge
{
    /// <summary>
    /// 是否处于任何形式的瞄准/准备打出状态（单体、全体、对己、无目标）
    /// </summary>
    public static bool IsAnyTargetingActive { get; private set; }
    
    /// <summary>
    /// 
    /// </summary>
    public static event Action? CardSelectionStateChanged;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="active"></param>
    public static void SetTargetingState(bool active)
    {
        if (IsAnyTargetingActive == active) return;
        IsAnyTargetingActive = active;
        CardSelectionStateChanged?.Invoke();
    }
}

/// <summary>
/// 
/// </summary>
[HarmonyPatch(typeof(NCardPlay))]
public static class NCardPlayPatch
{
    [HarmonyPatch("ShowMultiCreatureTargetingVisuals")]
    [HarmonyPostfix]
    static void Post_Show() => HangingSelectionBridge.SetTargetingState(true);

    [HarmonyPatch("HideTargetingVisuals")]
    [HarmonyPostfix]
    static void Post_Hide() => HangingSelectionBridge.SetTargetingState(false);
}