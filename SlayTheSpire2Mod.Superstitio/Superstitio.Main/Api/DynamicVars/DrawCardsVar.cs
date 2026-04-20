using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Superstitio.Api.DynamicVars.Extensions;

namespace Superstitio.Api.DynamicVars;

/// <summary>
/// 展示抽牌数
/// </summary>
/// <param name="cards"></param>
public class DrawCardsVar(int cards) : CardsVar(DefaultName, cards)
{
    /// 默认名称
    public static readonly string DefaultName = DrawCardsVar.DynamicVarName;
}

/// <summary>
/// 动态变量集的扩展方法
/// </summary>
public static class DrawCardsVarExtensions
{
    extension(DynamicVarSet dynamicVarSet)
    {
        /// <summary>
        /// 获取抽牌数量动态变量
        /// </summary>
        public DrawCardsVar DrawCards => dynamicVarSet.GetVarOrThrow<DrawCardsVar>(DrawCardsVar.DefaultName);
    }
}