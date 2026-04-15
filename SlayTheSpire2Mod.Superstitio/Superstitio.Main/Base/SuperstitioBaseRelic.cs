using System.Reflection;
using BaseLib.Abstracts;
using Godot;
using Superstitio.Main.Utils;

namespace Superstitio.Main.Base;

/// <summary>
/// 遗物图标尺寸枚举
/// </summary>
public enum RelicIconSize
{
    /// <summary>
    /// 小图标
    /// </summary>
    Small,

    /// <summary>
    /// 轮廓图标
    /// </summary>
    Outline,

    /// <summary>
    /// 大图标
    /// </summary>
    Large
}

/// <summary>
/// Superstitio 模组遗物基类
/// 提供基于约定俗成的资源路径自动加载遗物图标的功能
/// </summary>
public abstract class SuperstitioBaseRelic : CustomRelicModel
{
    /// <summary>
    /// 根据指定的图标尺寸获取遗物肖像资源路径
    /// 优先查找特定命名的图片，若不存在则回退到 default.png
    /// </summary>
    /// <param name="size">图标尺寸类型</param>
    /// <returns>资源路径字符串</returns>
    private string GetRelicPortraitPath(RelicIconSize size)
    {
        var type = this.GetType();
        string imgPrefix = ResourceUtils.GetImgPrefix(type);

        // 获取自定义名称逻辑
        var nameAttr = type.GetCustomAttribute<CustomImgNameAttribute>();
        string fileName = nameAttr is not null ? nameAttr.Name : type.Name;

        string sizeString = size switch
        {
            RelicIconSize.Small => "small",
            RelicIconSize.Outline => "outline",
            RelicIconSize.Large => "large",
            _ => throw new ArgumentOutOfRangeException(nameof(size), size, null)
        };

        string path = $"{imgPrefix}/relics/{sizeString}/{fileName}.png";

        if (ResourceLoader.Exists(path))
            return path;

        string defaultImg = $"{imgPrefix}/relics/{sizeString}/default.png";

        return defaultImg;
    }

    /// <summary>
    /// 获取小图标路径
    /// </summary>
    public override string PackedIconPath => this.GetRelicPortraitPath(RelicIconSize.Large);

    /// <summary>
    /// 获取轮廓图标路径
    /// </summary>
    protected override string PackedIconOutlinePath => this.GetRelicPortraitPath(RelicIconSize.Outline);

    /// <summary>
    /// 获取大图标路径
    /// </summary>
    protected override string BigIconPath => this.GetRelicPortraitPath(RelicIconSize.Large);
}