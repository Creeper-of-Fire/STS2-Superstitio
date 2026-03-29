using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Localization;

namespace Superstitio.Main.Utils;

/// <summary>
/// <see cref="LocStringFactory"/> 的内部静态辅助类，提供基于当前程序集前缀的默认实例和方法。
/// </summary>
internal static class SuperstitioLocStringFactory
{
    /// <summary>
    /// 基于 <see cref="LocStringFactory"/> 类型自动获取的程序集前缀。
    /// </summary>
    public static readonly string SuperstitioBaseLibPrefix = typeof(LocStringFactory).GetPrefix();

    /// <summary>
    /// 使用默认前缀初始化的 <see cref="LocStringFactory"/> 单例实例。
    /// </summary>
    public static readonly LocStringFactory Instance = new(SuperstitioBaseLibPrefix);

    /// <summary>
    /// 使用默认实例创建一个指向通用扩展本地化表的 <see cref="LocString"/>。
    /// </summary>
    /// <param name="locPrefix">本地化键的前缀。</param>
    /// <param name="locEntryKeys">本地化键的后续部分。</param>
    /// <returns>构建好的 <see cref="LocString"/> 实例。</returns>
    public static LocString ExtendLocString(string locPrefix, params IEnumerable<string> locEntryKeys)
    {
        return Instance.ExtendLocString(locPrefix, locEntryKeys);
    }

    /// <summary>
    /// 使用默认实例创建一个指向关键词本地化表的 <see cref="LocString"/>。
    /// </summary>
    /// <param name="locPrefix">本地化键的前缀。</param>
    /// <param name="locEntryKeys">本地化键的后续部分。</param>
    /// <returns>构建好的 <see cref="LocString"/> 实例。</returns>
    public static LocString KeywordLocString(string locPrefix, params IEnumerable<string> locEntryKeys)
    {
        return  Instance.KeywordLocString(locPrefix, locEntryKeys);
    }
}