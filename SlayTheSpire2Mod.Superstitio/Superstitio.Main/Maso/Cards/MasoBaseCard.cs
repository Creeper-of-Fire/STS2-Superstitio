using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using Superstitio.Main.SubPool;

namespace Superstitio.Main.Maso.Cards;

/// <summary>
/// 
/// </summary>
[Pool(typeof(MasoCardPool))] // 没有这个Baselib会报错
[AddToSubPool(typeof(MasoSubPool))]
public abstract class MasoBaseCard(
    int baseCost,
    CardType type,
    CardRarity rarity,
    TargetType target,
    bool showInCardLibrary = true,
    bool autoAdd = true)
    : CustomCardModel(baseCost, type, rarity, target, showInCardLibrary, autoAdd) { }

/// <inheritdoc />
public class MasoBaseCardTest() : MasoBaseCard(1, CardType.Attack, CardRarity.Ancient, TargetType.AllAllies);