using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Localization;

namespace Superstitio.Main.Utils;

/// <summary>
/// 用于创建本地化字符串（LocString）的工厂类。
/// </summary>
public class LocStringFactory(string baseLibPrefix)
{
    /// <summary>
    /// 通用扩展本地化表的名称。
    /// </summary>
    public const string GeneralExtendLocTable = "gameplay_ui";

    /// <summary>
    /// 关键词本地化表的名称（通常用于静态悬停提示）。
    /// </summary>
    public const string KeywordLocTable = "static_hover_tips";
    
    /// <summary>
    /// 获取当前工厂使用的基础库前缀。
    /// </summary>
    public string BaseLibPrefix { get; } = baseLibPrefix;

    /// <summary>
    /// 创建一个指向通用扩展本地化表的 <see cref="LocString"/>。
    /// </summary>
    /// <param name="locPrefix">本地化键的前缀，将自动移除命名空间前缀。</param>
    /// <param name="locEntryKeys">本地化键的后续部分。</param>
    /// <returns>构建好的 <see cref="LocString"/> 实例。</returns>
    public LocString ExtendLocString(string locPrefix, params IEnumerable<string> locEntryKeys)
    {
        return new LocString(GeneralExtendLocTable, string.Join(".", [this.BaseLibPrefix + locPrefix.RemovePrefix(), ..locEntryKeys]));
    }

    /// <summary>
    /// 创建一个指向关键词本地化表的 <see cref="LocString"/>。
    /// </summary>
    /// <param name="locPrefix">本地化键的前缀，将自动移除命名空间前缀。</param>
    /// <param name="locEntryKeys">本地化键的后续部分。</param>
    /// <returns>构建好的 <see cref="LocString"/> 实例。</returns>
    public LocString KeywordLocString(string locPrefix, params IEnumerable<string> locEntryKeys)
    {
        return new LocString(KeywordLocTable, string.Join(".", [this.BaseLibPrefix + locPrefix.RemovePrefix(), ..locEntryKeys]));
    }
}