using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Superstitio.Api.DynamicVars.Extensions;

namespace Superstitio.Main.DynamicVars.Extensions;

/// <summary>
/// 动态变量集的扩展方法
/// </summary>
public static class DynamicVarSetExtensions
{
    extension(DynamicVarSet dynamicVarSet)
    {
        /// <summary>
        /// 获取快感动态变量
        /// </summary>
        public FelixVar Felix => dynamicVarSet.GetVarOrThrow<FelixVar>(FelixVar.DefaultName);

        /// <summary>
        /// 获取自伤动态变量
        /// </summary>
        public DamageSelfVar DamageSelf => dynamicVarSet.GetVarOrThrow<DamageSelfVar>(DamageSelfVar.DefaultName);
    }
}