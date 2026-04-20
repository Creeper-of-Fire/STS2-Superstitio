using BaseLib.Utils;
using Superstitio.Main.Base;
using Superstitio.Api.Card;
using Superstitio.Main.SubPool;
using SuperstitioBaseCard = Superstitio.Main.Base.SuperstitioBaseCard;

namespace Superstitio.Main.Lupa.Base;

/// <inheritdoc />
[Pool(typeof(LupaCardPool))]
[AddToSubPool(typeof(LupaSubPool))]
public class DefendLupa : BaseDefend;

/// <inheritdoc />
[Pool(typeof(LupaCardPool))]
[AddToSubPool(typeof(LupaSubPool))]
public sealed class StrikeLupa : BaseStrike;

/// <summary>
/// <see cref="T:Superstitio.Main.Lupa.LupaCharacter">LupaCharacter</see> 的卡牌，自动做了卡池标记。
/// </summary>
[Pool(typeof(LupaCardPool))] // 没有这个Baselib会报错
[AddToSubPool(typeof(LupaSubPool))]
public abstract class LupaBaseCard(CardInitMessage cardInitMessage) : SuperstitioBaseCard(cardInitMessage);