using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using Superstitio.Api.Power;
using Superstitio.Main.Base;

namespace Superstitio.Main.Features.PowerCardPower;

/// <summary>
/// 卡牌能力标题的来源方式
/// </summary>
public enum SimpleCardPowerTitleSource
{
    /// <summary>
    /// 使用关联卡牌的标题作为能力标题
    /// </summary>
    FromCard,

    /// <summary>
    /// 使用基类默认的标题（通常由派生类自行实现）
    /// </summary>
    FallbackToBase
}

/// <summary>
/// 简单 Power 生成器，适用于专属某个卡的卡牌效果。
/// </summary>
public abstract class SimpleCardPower<TCardModel>(
    PowerInitMessage powerInitMessage,
    SimpleCardPowerTitleSource titleStyle = SimpleCardPowerTitleSource.FromCard
) : SuperstitioBasePower(powerInitMessage) where TCardModel : CardModel
{
    /// <inheritdoc />
    public override LocString Title => titleStyle switch
    {
        SimpleCardPowerTitleSource.FromCard => ModelDb.Card<TCardModel>().TitleLocString,
        SimpleCardPowerTitleSource.FallbackToBase => base.Title,
        _ => throw new ArgumentOutOfRangeException(nameof(titleStyle), titleStyle, null)
    };
}