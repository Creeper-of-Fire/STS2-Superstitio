using Superstitio.Api.BaseLib;
using Superstitio.Api.Card;
using Superstitio.Main.Resource;

namespace Superstitio.Main.Base;

/// <summary>
/// 
/// </summary>
/// <param name="cardInitMessage"></param>
public abstract class SuperstitioBaseCard(CardInitMessage cardInitMessage) : BaseCard(cardInitMessage)
{
    /// <inheritdoc />
    public override string PortraitPath => ResourceUtils.GetCardPortraitPath(this);
}