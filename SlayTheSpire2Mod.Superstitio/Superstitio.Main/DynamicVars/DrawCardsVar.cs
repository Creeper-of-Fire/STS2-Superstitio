using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Superstitio.Main.DynamicVars;

/// <summary>
/// 展示抽牌数
/// </summary>
/// <param name="cards"></param>
public class DrawCardsVar(int cards) : CardsVar(DefaultName, cards)
{
    /// <summary>
    /// 默认名称
    /// </summary>
    public const string DefaultName = nameof(DrawCardsVar);
}