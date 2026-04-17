using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;

namespace Superstitio.Main.SubPool.UI;

/// <summary>
/// 针对角色选择界面 (NCharacterSelectScreen) 的 Harmony 补丁类。
/// 主要用于注入自定义的子池选择 UI 组件，并根据用户选择的角色动态更新其可见性。
/// </summary>
[HarmonyPatch(typeof(NCharacterSelectScreen))]
public static class CharacterSelectPatches
{
    /// <summary>
    /// 自定义的子池选择器 UI 实例。
    /// </summary>
    private static MasoSubPoolSelector? _masoSelector;

    /// <summary>
    /// 在角色选择界面的 _Ready 方法执行后运行。
    /// 负责查找原版的 InfoPanel，实例化自定义 UI 并将其注入到指定位置。
    /// </summary>
    /// <param name="__instance">当前 NCharacterSelectScreen 的实例。</param>
    [HarmonyPatch(nameof(NCharacterSelectScreen._Ready))]
    [HarmonyPostfix]
    public static void PostfixReady(NCharacterSelectScreen __instance)
    {
        // 使用 Harmony 的 Traverse 访问私有变量 _infoPanel
        var infoPanel = Traverse.Create(__instance).Field("_infoPanel").GetValue<Control>();

        // 寻找 infoPanel 里的 VBoxContainer（它是原版文字排列的地方）
        // 源码中显示路径是 "InfoPanel/VBoxContainer"
        var vbox = infoPanel?.GetNodeOrNull<VBoxContainer>("VBoxContainer");

        if (vbox is null)
            return;

        // 创建我们的 UI 实例，Godot 会自动调用 _Ready
        _masoSelector = new MasoSubPoolSelector();

        vbox.AddChildSafely(_masoSelector);
        // 将我们的 UI 移动到描述文本之后，血量/金币显示之前
        vbox.MoveChild(_masoSelector, 2);
        Log.Info("[MasoMod] 成功注入子池选择 UI 到面板。");
    }

    /// <summary>
    /// 在角色选择事件 (SelectCharacter) 发生后运行。
    /// 根据当前选中的角色模型，动态更新自定义 UI 的可见性状态。
    /// </summary>
    /// <param name="characterModel">当前被选中的角色模型数据。</param>
    [HarmonyPatch(nameof(NCharacterSelectScreen.SelectCharacter))]
    [HarmonyPostfix]
    public static void PostfixSelectCharacter(CharacterModel characterModel)
    {
        // 这里的 characterModel 就是当前点击的角色模型
        _masoSelector?.UpdateVisibility(characterModel);
    }

    // 3. 游戏开始时的处理（可选）
    // [HarmonyPatch(nameof(NCharacterSelectScreen.OnEmbarkPressed))]
    // [HarmonyPrefix]
    // public static void OnEmbark(NCharacterSelectScreen __instance)
    // {
    //     // 可以在这里最后检查一次哪些子池被选中了，并锁定到当前的 RunState 中
    // }
}