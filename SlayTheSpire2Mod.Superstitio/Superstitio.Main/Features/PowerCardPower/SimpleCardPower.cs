using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using Superstitio.Main.Base;

namespace Superstitio.Main.Features.PowerCardPower;

/// <summary>
/// 简单 Power 生成器，适用于专属某个卡的卡牌效果。
/// </summary>
public abstract class SimpleCardPower<TCardModel>(PowerInitMessage powerInitMessage) : SuperstitioBasePower(powerInitMessage) where TCardModel : CardModel
{
    /// <inheritdoc />
    public override LocString Title => ModelDb.Card<TCardModel>().TitleLocString;
}
