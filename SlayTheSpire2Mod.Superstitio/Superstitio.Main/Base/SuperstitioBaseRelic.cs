using BaseLib.Abstracts;
using Superstitio.Main.Resource;

namespace Superstitio.Main.Base;

/// <summary>
/// Superstitio 模组遗物基类
/// 提供基于约定俗成的资源路径自动加载遗物图标的功能
/// </summary>
public abstract class SuperstitioBaseRelic : CustomRelicModel
{
    /// <summary>
    /// 获取小图标路径
    /// </summary>
    /// <inheritdoc />
    public override string PackedIconPath => ResourceUtils.GetRelicPortraitPath(this, ResourceUtils.RelicIconSize.Large);

    /// <summary>
    /// 获取轮廓图标路径
    /// </summary>
    protected override string PackedIconOutlinePath => ResourceUtils.GetRelicPortraitPath(this, ResourceUtils.RelicIconSize.Outline);

    /// <summary>
    /// 获取大图标路径
    /// </summary>
    protected override string BigIconPath => ResourceUtils.GetRelicPortraitPath(this, ResourceUtils.RelicIconSize.Large);
}