using System.Reflection;
using Superstitio.Main.ModSetting;

namespace Superstitio.Main.Utils;

/// <summary>
/// 自定义图片资源名称（不含后缀）。
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = true)]
public class CustomImgNameAttribute(string name) : Attribute
{
    /// <summary>
    /// 自定义的图片名称。
    /// </summary>
    public string Name { get; } = name;
}

/// <summary>
/// 标识该卡牌是否属于猎奇/Guro内容。
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = true)]
public class IsGuroAttribute(bool isGuro = true) : Attribute
{
    /// <summary>
    /// 是否是猎奇内容。
    /// </summary>
    public bool IsGuro { get; } = isGuro;
}

/// <summary>
/// 资源工具类
/// </summary>
public static class ResourceUtils
{
    /// <summary>
    /// 根据类型获取图片前缀，其会处理 SFW / NSFW / Guro 逻辑。
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static string GetImgPrefix(Type type)
    {
        const string prefix = $"res://{Plugin.ModName}/images";

        // 获取 SFW/Guro 状态逻辑
        var guroAttr = type.GetCustomAttribute<IsGuroAttribute>();
        bool isGuroCard = guroAttr?.IsGuro ?? false;

        bool useNSFWFolder;

        if (isGuroCard && !SuperstitioModConfig.IsGuroEnabled)
            useNSFWFolder = false;
        else
            useNSFWFolder = SuperstitioModConfig.IsNSFWEnabled;

        string sfw = useNSFWFolder ? "img" : "imgSFW";

        string imgPrefix = $"{prefix}/{sfw}";
        return imgPrefix;
    }
}