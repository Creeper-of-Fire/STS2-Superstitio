using BaseLib.Utils;
using Superstitio.Main.Base;
using Superstitio.Api.Card;
using Superstitio.Main.SubPool;
using SuperstitioBaseCard = Superstitio.Main.Base.SuperstitioBaseCard;

namespace Superstitio.Main.Maso.Base;

/// <inheritdoc />
[Pool(typeof(MasoCardPool))]
[AddToSubPool(typeof(MasoSubPool))]
public class DefendMaso : BaseDefend;

/// <inheritdoc />
[Pool(typeof(MasoCardPool))]
[AddToSubPool(typeof(MasoSubPool))]
public sealed class StrikeMaso : BaseStrike;

/// <summary>
/// <see cref="T:Superstitio.Main.Maso.MasoCharacter">MasoCharacter</see> 的卡牌，自动做了卡池标记。
/// </summary>
[Pool(typeof(MasoCardPool))] // 没有这个Baselib会报错
[AddToSubPool(typeof(MasoSubPool))]
public abstract class MasoBaseCard(CardInitMessage cardInitMessage) : SuperstitioBaseCard(cardInitMessage);