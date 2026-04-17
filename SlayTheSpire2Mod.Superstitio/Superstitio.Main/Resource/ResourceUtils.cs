using System.Reflection;
using Godot;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using Superstitio.Main.ModSetting;

namespace Superstitio.Main.Resource;

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

        if (isGuroCard && !SuperstitioModConfig.IsGuroImgEnabled)
            useNSFWFolder = false;
        else
            useNSFWFolder = SuperstitioModConfig.IsNSFWImgEnabled;

        string sfw = useNSFWFolder ? "img" : "imgSFW";

        string imgPrefix = $"{prefix}/{sfw}";
        return imgPrefix;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="power"></param>
    /// <returns></returns>
    public static string GetPowerPortraitPath(PowerModel power)
    {
        var type = power.GetType();
        string imgPrefix = GetImgPrefix(type);

        // 获取自定义名称逻辑
        var nameAttr = type.GetCustomAttribute<CustomImgNameAttribute>();
        string fileName = nameAttr is not null ? nameAttr.Name : type.Name;

        string path = $"{imgPrefix}/powers/{fileName}.png";

        if (ResourceLoader.Exists(path))
            return path;

        string defaultImg = $"{imgPrefix}/powers/default.png";

        return defaultImg;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="card"></param>
    /// <returns></returns>
    public static string GetCardPortraitPath(CardModel card)
    {
        var type = card.GetType();
        string imgPrefix = GetImgPrefix(type);

        // 获取自定义名称逻辑
        var nameAttr = type.GetCustomAttribute<CustomImgNameAttribute>();
        string fileName = nameAttr is not null ? nameAttr.Name : type.Name;

        string cardTypeStr = GetCardTypeString(card);

        string path = $"{imgPrefix}/cards/{cardTypeStr}/{fileName}.png";

        if (ResourceLoader.Exists(path))
            return path;

        string defaultImg = $"{imgPrefix}/cards/default.png";

        return defaultImg;

        string GetCardTypeString(CardModel cardModel)
        {
            if (cardModel.Rarity == CardRarity.Basic)
                return "base";
            return card.Type switch
            {
                CardType.Attack => "attack",
                CardType.Skill => "skill",
                CardType.Power => "power",
                _ => "special"
            };
        }
    }

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
    /// 根据指定的图标尺寸获取遗物肖像资源路径
    /// 优先查找特定命名的图片，若不存在则回退到 default.png
    /// </summary>
    /// <param name="relic"></param>
    /// <param name="size"></param>
    /// <returns>资源路径字符串</returns>
    public static string GetRelicPortraitPath(RelicModel relic, RelicIconSize size)
    {
        var type = relic.GetType();
        string imgPrefix = GetImgPrefix(type);

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
}