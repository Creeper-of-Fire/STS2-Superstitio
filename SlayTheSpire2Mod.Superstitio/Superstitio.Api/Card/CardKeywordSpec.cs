using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using Superstitio.Analyzer;

namespace Superstitio.Api.Card;

/// <summary>
/// 表示一个带有升级行为的卡牌关键字规范。
/// </summary>
public record CardKeywordSpec
{
    /// <summary>
    /// 定义关键字在升级时的添加/移除行为类型。
    /// </summary>
    public enum AddType
    {
        /// <summary>
        /// 普通类型：关键字在升级前后均存在，无特殊变化。
        /// </summary>
        Normal,

        /// <summary>
        /// 升级后移除：关键字在升级前存在，升级后被移除。
        /// </summary>
        RemoveAfterUpgrade,

        /// <summary>
        /// 仅升级后添加：关键字在升级前不存在，仅在升级后添加。
        /// </summary>
        OnlyAddAfterUpgrade
    }

    /// <summary>
    /// 卡牌关键字。
    /// </summary>
    public CardKeyword? Keyword { get; init; }

    /// <summary>
    /// 获取或设置关键字在升级时的行为类型。
    /// </summary>
    public AddType UpgradeBehavior { get; init; } = AddType.Normal;

    /// <summary>
    /// 获取一个值，指示该关键字是否在升级后被移除。
    /// </summary>
    public bool RemoveAfterUpgrade => this.UpgradeBehavior == AddType.RemoveAfterUpgrade;

    /// <summary>
    /// 获取一个值，指示该关键字是否仅在升级后添加。
    /// </summary>
    public bool OnlyAddAfterUpgrade => this.UpgradeBehavior == AddType.OnlyAddAfterUpgrade;

    /// <summary>
    /// 获取一个值，指示该关键字在升级前是否存在。
    /// </summary>
    public bool ExistsBeforeUpgrade => this.UpgradeBehavior != AddType.OnlyAddAfterUpgrade;

    /// <summary>
    /// 获取一个值，指示该关键字在升级后是否存在。
    /// </summary>
    public bool ExistsAfterUpgrade => this.UpgradeBehavior != AddType.RemoveAfterUpgrade;

    /// <summary>
    /// 从 <see cref="CardKeyword"/> 隐式转换为 <see cref="CardKeywordSpec"/>。
    /// </summary>
    /// <param name="keyword">卡牌关键字。</param>
    public static implicit operator CardKeywordSpec(CardKeyword keyword) =>
        new() { Keyword = keyword, UpgradeBehavior = AddType.Normal };
}

/// <summary>
/// 为 <see cref="CardKeyword"/> 提供扩展方法。
/// </summary>
public static class CardKeywordExtensions
{
    extension(CardKeyword keyword)
    {
        /// <summary>
        /// 创建一个在升级后被移除的关键字规范。
        /// </summary>
        public CardKeywordSpec RemoveOnUpgrade()
        {
            return new CardKeywordSpec
            {
                Keyword = keyword,
                UpgradeBehavior = CardKeywordSpec.AddType.RemoveAfterUpgrade
            };
        }

        /// <summary>
        /// 创建一个在升级后被添加的关键字规范。
        /// </summary>
        public CardKeywordSpec OnlyAddOnUpgrade()
        {
            return new CardKeywordSpec
            {
                Keyword = keyword,
                UpgradeBehavior = CardKeywordSpec.AddType.OnlyAddAfterUpgrade
            };
        }
    }


    extension(IEnumerable<CardKeywordSpec> specs)
    {
        /// <summary>
        /// 从关键字规范集合中获取升级前就存在的基础关键字。
        /// </summary>
        /// <returns>升级前存在的关键字集合。</returns>
        public IEnumerable<CardKeyword> GetBaseKeywords()
        {
            return specs
                .Where(spec => spec.ExistsBeforeUpgrade)
                .Select(spec => spec.Keyword)
                .OfType<CardKeyword>();
        }
    }

    extension(CardModel cardModel)
    {
        /// <inheritdoc cref="CardModel.RemoveKeyword"/>
        public void RemoveKeyword(CardKeywordSpec spec)
        {
            if (spec.Keyword != null)
                cardModel.RemoveKeyword(spec.Keyword.Value);
        }

        /// <inheritdoc cref="CardModel.AddKeyword"/>
        public void AddKeyword(CardKeywordSpec spec)
        {
            if (spec.Keyword != null)
                cardModel.AddKeyword(spec.Keyword.Value);
        }
    }
}

/// <summary>
/// 实现此接口的卡牌将在升级时自动应用关键字的添加/移除行为。
/// 由 <see cref="CardInitUtils.OnUpgrade"/> 自动处理。
/// </summary>
[AttachedTo(typeof(CardModel))]
public interface ICardWithCardKeywordSpecs
{
    /// <summary>
    /// 关键字配置集合，每个配置定义了升级时的添加或移除行为。
    /// </summary>
    IEnumerable<CardKeywordSpec> InitCardKeywords { get; }

    /// <summary>
    /// 获取卡牌的基础关键字集合（仅包含升级前就存在的关键字）。
    /// </summary>
    public IEnumerable<CardKeyword> GetCanonicalCardKeywords()
    {
        return this.InitCardKeywords.GetBaseKeywords();
    }
}