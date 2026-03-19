namespace Superstitio.Main;

using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;


/// <summary>
/// 模组的主初始化类。
/// </summary>
[ModInitializer("Initialize")]
public static class Plugin
{
    /// <summary>
    /// 初始化模组，在游戏启动时调用。
    /// </summary>
    public static void Initialize()
    {
        Log.Info("[Superstitio] 已加载！");
    }
    /// <summary>
    /// 卸载模组，在游戏关闭或模组禁用时调用。
    /// </summary>
    public static void Unload()
    {
        Log.Info("[Superstitio] 已卸载！");
    }
}