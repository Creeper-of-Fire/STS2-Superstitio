using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Superstitio.Main.DynamicVars.Extensions;

namespace Superstitio.Main.DynamicVars;

/// <summary>
/// 展示抽牌数
/// </summary>
/// <param name="cards"></param>
public class DrawCardsVar(int cards) : CardsVar(DefaultName, cards)
{
    /// 默认名称
    public static readonly string DefaultName = DrawCardsVar.DynamicVarName;
}