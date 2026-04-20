using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Superstitio.Analyzer;

namespace Superstitio.Api.CustomKeywords;

/// <summary>
/// 实现此接口的对象将添加关键字。
/// </summary>
[AttachedTo(typeof(AbstractModel))]
public interface IWithDescriptionWords
{
    /// <summary>
    /// 描述中的关键字配置集合，其仅作为悬浮提示和描述存在。
    /// </summary>
    IEnumerable<IDescriptionWord> DescriptionWords { get; }
    
    /// <summary>
    /// 获取卡牌的基础动态变量集合。
    /// </summary>
    public IEnumerable<DynamicVar> GetCanonicalVars()
    {
        return this.DescriptionWords.Select(it => it.ToStringVar);
    }
}