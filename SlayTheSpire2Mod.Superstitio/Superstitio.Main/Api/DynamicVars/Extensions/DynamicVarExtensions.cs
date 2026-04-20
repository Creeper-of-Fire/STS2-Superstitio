using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Superstitio.Api.DynamicVars.Extensions;

/// <summary>
/// 提供动态变量名称相关的扩展方法。
/// </summary>
public static class DynamicVarExtensions
{
    extension<TVar>(TVar) where TVar : DynamicVar
    {
        /// <summary>
        /// 获取动态变量的名称（移除类型名末尾的 "Var"）。
        /// </summary>
        public static string DynamicVarName => typeof(TVar).Name.Replace("Var", "");
    }
}