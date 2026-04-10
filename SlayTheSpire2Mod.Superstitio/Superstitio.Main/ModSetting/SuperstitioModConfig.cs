using BaseLib.Config;

namespace Superstitio.Main.ModSetting;

/// <summary>
/// 模组的配置
/// </summary>
[HoverTipsByDefault]
public class SuperstitioModConfig : SimpleModConfig
{
    /// <summary>
    /// 启用和谐模式。
    /// 开启后，所有可能引起不适的内容将被和谐处理。
    /// </summary>
    [ConfigSection("ContentFilter")]
    [ConfigHoverTip]
    public static bool EnableSafeForWork { get; set; } = true;

    /// <summary>
    /// 启用猎奇内容。
    /// 必须关闭和谐模式才能起效。
    /// </summary>
    [ConfigSection("ContentFilter")]
    [ConfigHoverTip]
    public static bool EnableGuro { get; set; } = false;

    /// <summary>
    /// 启用性能模式
    /// </summary>
    [ConfigSection("Performance")]
    [ConfigHoverTip]
    public static bool EnablePerformanceMode { get; set; } = true;

    /// <summary>
    /// 手动强制设置 SFW 状态。
    /// </summary>
    private const bool ManualSFWOverride = false;

    /// <summary>
    /// 获取实际生效的 NSFW 状态
    /// </summary>
    public static bool IsNSFWEnabled => !(ManualSFWOverride || EnableSafeForWork);

    /// <summary>
    /// 获取实际生效的猎奇角色状态
    /// </summary>
    public static bool IsGuroEnabled => IsNSFWEnabled && EnableGuro;
}