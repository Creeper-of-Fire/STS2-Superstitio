using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using Superstitio.Main.Extensions;

namespace Superstitio.Main.Features.PowerCardPower;

/// <summary>
/// 能力卡的简单 Power 生成器
/// </summary>
public abstract class PowerCardPower<TCardModel> : CustomPowerModel where TCardModel : CardModel
{
    /// <inheritdoc />
    public override PowerType Type => PowerType.Buff;

    /// <inheritdoc />
    public override PowerStackType StackType => PowerStackType.Single;

    /// <inheritdoc />
    public override bool IsInstanced => true;

    /// <inheritdoc />
    public override LocString Title => ModelDb.Card<TCardModel>().TitleLocString;
}
