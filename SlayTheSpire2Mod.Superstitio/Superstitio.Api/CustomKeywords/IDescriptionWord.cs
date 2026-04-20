using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Superstitio.Api.CustomKeywords;

/// <summary>
/// 一个“词”，其代表了对应的关键字和字符串动态变量。
/// </summary>
public interface IDescriptionWord
{
    /// <summary>
    /// 悬停提示
    /// </summary>
    public IHoverTip HoverTip { get; }

    /// <summary>
    /// 转换为动态变量，以便添加
    /// </summary>
    public StringVar ToStringVar{ get; }
}