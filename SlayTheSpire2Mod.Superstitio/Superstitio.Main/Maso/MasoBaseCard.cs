using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Superstitio.Main.SubPool;

namespace Superstitio.Main.Maso;

/// <summary>
/// 用于初始化卡牌基本属性的记录类型。
/// </summary>
public record CardInitMessage
{
    /// <summary>
    /// 卡牌的基础费用。
    /// </summary>
    public required int BaseCost { get; init; }

    /// <summary>
    /// 卡牌的类型。
    /// </summary>
    public required CardType Type { get; init; }

    /// <summary>
    /// 卡牌的稀有度。
    /// </summary>
    public required CardRarity Rarity { get; init; }

    /// <summary>
    /// 卡牌的使用目标类型。
    /// </summary>
    public required TargetType Target { get; init; }

    /// <summary>
    /// 指示该卡牌是否显示在卡牌库中。
    /// </summary>
    public bool ShowInCardLibrary { get; init; } = true;

    /// <summary>
    /// <see cref="BaseLib"/> 模组：指示该卡牌是否自动添加/注册。
    /// </summary>
    public bool AutoAdd { get; init; } = true;
}

/// <summary>
/// 
/// </summary>
[Pool(typeof(MasoCardPool))] // 没有这个Baselib会报错
[AddToSubPool(typeof(MasoSubPool))]
public abstract class MasoBaseCard(CardInitMessage cardInitMessage) : CustomCardModel(
    cardInitMessage.BaseCost,
    cardInitMessage.Type,
    cardInitMessage.Rarity,
    cardInitMessage.Target,
    cardInitMessage.ShowInCardLibrary,
    cardInitMessage.AutoAdd
)
{
    /// <summary>
    /// 定义卡牌的标准标签集合。
    /// </summary>
    protected override HashSet<CardTag> CanonicalTags => [];

    /// <summary>
    /// 定义卡牌的动态变量集合。
    /// </summary>
    protected override IEnumerable<DynamicVar> CanonicalVars => [];

    /// <summary>
    /// 执行卡牌效果。
    /// </summary>
    /// <param name="choiceContext">玩家选择上下文。</param>
    /// <param name="cardPlay">执行相关信息。</param>
    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// 卡牌升级效果。
    /// </summary>
    protected override void OnUpgrade() { }
}

/// <inheritdoc />
public class MasoBaseCardTest() : MasoBaseCard(new()
{
    BaseCost = 0,
    Type = CardType.None,
    Rarity = CardRarity.None,
    Target = TargetType.None,
});