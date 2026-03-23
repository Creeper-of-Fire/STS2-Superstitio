using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using Superstitio.Main.Base;
using Superstitio.Main.Maso.Pools;
using Superstitio.Main.SubPool;
using Superstitio.Main.Utils;

namespace Superstitio.Main.Maso.Cards;

/// <summary>
/// 
/// </summary>
[Pool(typeof(MasoCardPool))] // 没有这个Baselib会报错
[AddToSubPool(typeof(MasoSubPool))]
public abstract class MasoBaseCard(CardInitMessage cardInitMessage) : SuperstitioBaseCard(
    cardInitMessage
);

/// <inheritdoc />
public class MasoBaseCardTest() : MasoBaseCard(new CardInitMessage
{
    BaseCost = 0,
    Type = CardType.None,
    Rarity = CardRarity.None,
    Target = TargetType.None,
});