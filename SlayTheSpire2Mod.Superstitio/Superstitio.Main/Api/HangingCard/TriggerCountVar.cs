using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Superstitio.Api.DynamicVars.Extensions;

namespace Superstitio.Api.HangingCard;

/// <summary>
/// 触发次数动态变量
/// </summary>
public class TriggerCountVar(int triggers, string? name = null) : IntVar(name ?? DefaultName, triggers)
{
    /// 默认名称
    public static readonly string DefaultName = TriggerCountVar.DynamicVarName;
}

/// <summary>
/// 为动态变量集添加获取触发次数动态变量扩展方法
/// </summary>
public static class TriggerCountVarExtensions
{
    extension(DynamicVarSet dynamicVarSet)
    {
        /// <summary>
        /// 获取触发次数动态变量
        /// </summary>
        public TriggerCountVar TriggerCount => dynamicVarSet.GetVarOrThrow<TriggerCountVar>(TriggerCountVar.DefaultName);
    }
}