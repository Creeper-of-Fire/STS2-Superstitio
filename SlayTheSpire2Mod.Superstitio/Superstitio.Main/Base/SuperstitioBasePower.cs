using System.Reflection;
using BaseLib.Abstracts;
using Godot;
using Superstitio.Main.Utils;

namespace Superstitio.Main.Base;

/// <summary>
/// 
/// </summary>
public abstract class SuperstitioBasePower : CustomPowerModel
{
    private string GetPowerPortraitPath()
    {
        var type = this.GetType();
        string imgPrefix = ResourceUtils.GetImgPrefix(type);

        // 获取自定义名称逻辑
        var nameAttr = type.GetCustomAttribute<CustomImgNameAttribute>();
        string fileName = nameAttr != null ? nameAttr.Name : type.Name;

        string path = $"{imgPrefix}/powers/{fileName}.png";

        if (ResourceLoader.Exists(path))
            return path;

        string defaultImg = $"{imgPrefix}/powers/default.png";

        return defaultImg;
    }

    /// <inheritdoc />
    public override string CustomBigIconPath => this.GetPowerPortraitPath();

    /// <inheritdoc />
    public override string CustomPackedIconPath => this.GetPowerPortraitPath();
}