using Godot;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Superstitio.Api.Utils;

namespace Superstitio.Api.CustomKeywords;

/// <summary>
/// 自定义关键词基类
/// </summary>
public record CustomWord
{
    /// <summary>
    /// 初始化自定义卡牌关键词
    /// </summary>
    /// <param name="LocStringFactory">本地化字符串工厂</param>
    /// <param name="Name">关键词名称,如果为 null 则使用类型名称</param>
    public CustomWord(LocStringFactory LocStringFactory, string? Name = null)
    {
        this.LocStringFactory = LocStringFactory;
        this.Name = Name ?? this.GetType().Name;
    }

    private LocStringFactory LocStringFactory { get; init; }

    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// 本地化标题
    /// </summary>
    public LocString Title => this.LocStringFactory.CardKeywordString(this.Name, nameof(this.Title));

    /// <summary>
    /// 描述
    /// </summary>
    public LocString Description => this.LocStringFactory.CardKeywordString(this.Name, nameof(this.Description));

    /// <summary>
    /// 图标纹理
    /// </summary>
    public Texture2D? Icon { get; init; } = null;

    /// <summary>
    /// 悬停提示
    /// </summary>
    public IHoverTip HoverTip => new HoverTip(this.Title, this.Description, this.Icon);

    /// <summary>
    /// 转换为动态变量，以便添加
    /// </summary>
    public StringVar ToStringVar => new(this.Name, this.Title.GetFormattedText());
}