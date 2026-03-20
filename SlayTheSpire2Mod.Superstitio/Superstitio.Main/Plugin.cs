using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using Superstitio.Main.SubPool;
using Superstitio.Main.SubPool.UI;

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

        RegisterSubPools();
    }

    /// <summary>
    /// 扫描当前程序集中的 SubPool 并注册
    /// </summary>
    private static void RegisterSubPools()
    {
        var assembly = Assembly.GetExecutingAssembly();

        SubPoolMemberRegistry.RegisterSubPoolsFromAssembly(assembly);

        SubPoolManager.Initialize(assembly);
    }

    /// <summary>
    /// 卸载模组，在游戏关闭或模组禁用时调用。
    /// </summary>
    public static void Unload()
    {
        Log.Info("[Superstitio] 已卸载！");
    }
}