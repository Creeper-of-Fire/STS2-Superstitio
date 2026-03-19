using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;

namespace Superstitio.Main;

/// <summary>
/// 模组的主初始化类。
/// </summary>
[ModInitializer("Initialize")]
public static class Plugin
{
    /// <summary>
    /// 模组名称
    /// </summary>
    public const string ModName = "Superstitio";

    /// <summary>
    /// 初始化模组，在游戏启动时调用。
    /// </summary>
    public static void Initialize()
    {
        Log.Info("[Superstitio] 已加载！");
        Harmony harmony = new Harmony(ModName);
        harmony.PatchAll();
    }

    /// <summary>
    /// 卸载模组，在游戏关闭或模组禁用时调用。
    /// </summary>
    public static void Unload()
    {
        Log.Info("[Superstitio] 已卸载！");
    }
}