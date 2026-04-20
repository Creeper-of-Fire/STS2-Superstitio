using MegaCrit.Sts2.Core.Localization;

namespace Superstitio.Api.Utils;

/// <summary>
/// 用于创建本地化字符串（LocString）的工厂类。
/// </summary>
public class LocStringFactory(string modPrefix)
{
    /// <summary>
    /// 移除命名空间前缀。
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static string RemovePrefix(string id)
    {
        string str = id;
        int startIndex = id.IndexOf('-') + 1;
        return str[startIndex..];
    }
    
    /// <summary>
    /// 通用扩展本地化表的名称。
    /// </summary>
    public const string GeneralExtendLocTable = "gameplay_ui";

    /// <summary>
    /// 关键词本地化表的名称（通常用于静态悬停提示）。
    /// </summary>
    public const string KeywordLocTable = "static_hover_tips";
    
    /// <summary>
    /// 获取当前工厂使用的模组前缀。
    /// </summary>
    public string ModPrefix { get; } = modPrefix;

    /// <summary>
    /// 创建一个指向特定本地化表的 <see cref="LocString"/>。
    /// </summary>
    /// <param name="locTable">本地化表</param>
    /// <param name="locPrefix">本地化键的前缀，将自动移除命名空间前缀。</param>
    /// <param name="locEntryKeys">本地化键的后续部分。</param>
    /// <returns>构建好的 <see cref="LocString"/> 实例。</returns>
    public LocString CreateLocString(string locTable,string locPrefix, params IEnumerable<string> locEntryKeys)
    {
        return new LocString(locTable, string.Join(".", [this.ModPrefix + RemovePrefix(locPrefix), ..locEntryKeys]));
    }

    /// <summary>
    /// 创建一个指向通用扩展本地化表的 <see cref="LocString"/>。
    /// </summary>
    /// <param name="locPrefix">本地化键的前缀，将自动移除命名空间前缀。</param>
    /// <param name="locEntryKeys">本地化键的后续部分。</param>
    /// <returns>构建好的 <see cref="LocString"/> 实例。</returns>
    public LocString ExtendLocString(string locPrefix, params IEnumerable<string> locEntryKeys)
    {
        return this.CreateLocString(GeneralExtendLocTable, locPrefix, locEntryKeys);
    }

    /// <summary>
    /// 创建一个指向关键词本地化表的 <see cref="LocString"/>。
    /// </summary>
    /// <param name="locPrefix">本地化键的前缀，将自动移除命名空间前缀。</param>
    /// <param name="locEntryKeys">本地化键的后续部分。</param>
    /// <returns>构建好的 <see cref="LocString"/> 实例。</returns>
    public LocString KeywordLocString(string locPrefix, params IEnumerable<string> locEntryKeys)
    {
        return this.CreateLocString(KeywordLocTable, locPrefix, locEntryKeys);
    }
}