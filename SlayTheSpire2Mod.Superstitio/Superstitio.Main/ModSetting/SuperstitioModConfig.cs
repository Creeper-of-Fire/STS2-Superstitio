using BaseLib.Config;
using BaseLib.Config.UI;
using Godot;
using Superstitio.Main.Features.SafeForWork;

namespace Superstitio.Main.ModSetting;

/// <summary>
/// 模组的配置
/// </summary>
[HoverTipsByDefault]
public class SuperstitioModConfig : SimpleModConfig
{
    // 存储 UI 引用以便联动
    private static NConfigOptionRow? NsfwImgRow { get; set; }
    private static NConfigOptionRow? GuroContentRow { get; set; }
    private static NConfigOptionRow? GuroImgRow { get; set; }

    /// <summary>
    /// 开启不适合办公场合的内容 (NSFW)。
    /// 默认关闭。关闭时将强制使用和谐版本。
    /// </summary>
    [ConfigSection("ContentFilter")]
    [ConfigHoverTip]
    public static bool EnableNotSafeForWork
    {
        get;
        set
        {
            if (field == value)
                return;
            field = value;
            ApplyLocVariant();
            RefreshAllUI();
        }
    } = false;

    /// <summary>
    /// 是否显示 NSFW 图片资源。
    /// 仅在开启 EnableNotSafeForWork 时可选。
    /// </summary>
    [ConfigSection("ContentFilter")]
    [ConfigHoverTip]
    public static bool ShowNsfwImages { get; set; } = false;

    /// <summary>
    /// 是否允许启用猎奇/血腥内容。
    /// 仅在开启 EnableNotSafeForWork 时可选。
    /// </summary>
    [ConfigSection("ContentFilter")]
    [ConfigHoverTip]
    public static bool EnableGuroContent
    {
        get;
        set
        {
            if (field == value)
                return;
            field = value;
            ApplyLocVariant();
            RefreshAllUI();
        }
    } = false;

    /// <summary>
    /// 是否显示猎奇/血腥图片。
    /// 仅在 EnableGuroContent 开启时可选。
    /// </summary>
    [ConfigSection("ContentFilter")]
    [ConfigHoverTip]
    public static bool ShowGuroImages { get; set; } = false;

    /// <summary>
    /// 启用性能模式
    /// </summary>
    [ConfigSection("Performance")]
    [ConfigHoverTip]
    public static bool EnablePerformanceMode { get; set; } = false;

    /// <summary>
    /// 最终逻辑判定：是否使用 NSFW 资源路径 (图片用)
    /// </summary>
    public static bool IsNSFWImgEnabled => EnableNotSafeForWork && ShowNsfwImages;

    /// <summary>
    /// 最终逻辑判定：是否激活 Guro 内容 (本地化用)
    /// </summary>
    public static bool IsGuroContentEnabled => EnableNotSafeForWork && EnableGuroContent;

    /// <summary>
    /// 最终逻辑判定：是否显示 Guro 图片 (图片用)
    /// </summary>
    public static bool IsGuroImgEnabled => IsGuroContentEnabled && ShowGuroImages;

    /// <inheritdoc />
    public override void SetupConfigUI(Control optionContainer)
    {
        base.SetupConfigUI(optionContainer);

        // 绑定 UI 引用
        foreach (var child in optionContainer.GetChildren())
        {
            if (child is not NConfigOptionRow row) continue;

            string name = row.Name.ToString();
            if (name.Contains(nameof(ShowNsfwImages))) NsfwImgRow = row;
            else if (name.Contains(nameof(EnableGuroContent))) GuroContentRow = row;
            else if (name.Contains(nameof(ShowGuroImages))) GuroImgRow = row;
        }

        RefreshAllUI();
    }

    /// <summary>
    /// 瀑布流式刷新 UI 禁用状态
    /// </summary>
    private static void RefreshAllUI()
    {
        bool nsfwAllowed = EnableNotSafeForWork;
        bool guroUnlocked = nsfwAllowed && EnableGuroContent;

        // 只有开启了 NSFW 总开关，下面的才可用
        SetRowDisabled(NsfwImgRow, !nsfwAllowed);
        SetRowDisabled(GuroContentRow, !nsfwAllowed);
        
        // 只有开启了总开关且开启了猎奇内容，图片开关才可用
        SetRowDisabled(GuroImgRow, !guroUnlocked);
    }

    private static void SetRowDisabled(NConfigOptionRow? row, bool disabled)
    {
        if (row is null || !GodotObject.IsInstanceValid(row)) return;

        var control = row.SettingControl;
        if (!GodotObject.IsInstanceValid(control)) return;

        // 交互限制
        if (control is BaseButton btn) btn.Disabled = disabled;
        control.FocusMode = disabled ? Control.FocusModeEnum.None : Control.FocusModeEnum.All;

        // 视觉反馈
        row.Modulate = disabled ? new Color(0.5f, 0.5f, 0.5f, 0.6f) : new Color(1, 1, 1, 1);
        row.MouseFilter = disabled ? Control.MouseFilterEnum.Ignore : Control.MouseFilterEnum.Stop;
    }

    /// <summary>
    /// 
    /// </summary>
    public static void ApplyLocVariant()
    {
        var variant = GetCurrentVariant();
        LocVariantManager.ApplyVariant(variant, Plugin.ModName);
    }

    private static LocVariantManager.LocVariant GetCurrentVariant()
    {
        if (!EnableNotSafeForWork)
            return LocVariantManager.LocVariant.Sfw;
        if (EnableGuroContent)
            return LocVariantManager.LocVariant.Guro;
        return LocVariantManager.LocVariant.Nsfw;
    }
}