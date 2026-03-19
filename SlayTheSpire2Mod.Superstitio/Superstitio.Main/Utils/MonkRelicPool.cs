using BaseLib.Abstracts;
using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;

namespace Superstitio.Main.Utils;

/// <summary>
/// 占位符职业的卡牌池模型。
/// </summary>
public class MonkCardPool : CustomCardPoolModel
{
    /// <summary>
    /// 卡池的 ID。必须唯一，防撞车。
    /// </summary>
    public override string Title => "test-cea173d4-6898-4dd6-a026-62c8c9383655";

    /// <summary>
    /// 卡池的能量图标。
    /// </summary>
    /// <remarks>
    /// 暂时不支持加载，建议暂时使用原版，或者通过更改 <see cref="CardModel.EnergyIcon"/> 修改。
    /// </remarks>
    public override string EnergyColorName => "ironclad";

    /// <summary>
    /// 卡池的主题色。
    /// </summary>
    public override Color DeckEntryCardColor => new(0.5f, 0.5f, 1f);

    /// <summary>
    /// 卡池是否是无色。例如事件、状态等卡池就是无色的。
    /// </summary>
    public override bool IsColorless => false;
}

/// <summary>
/// 占位符职业的药水池模型。
/// </summary>
public class MonkPotionPool : CustomPotionPoolModel
{
    /// <summary>
    /// 能量图标。
    /// </summary>
    public override string EnergyColorName => "ironclad";
}

/// <summary>
/// 占位符职业的遗物池模型。
/// </summary>
public class MonkRelicPool : CustomRelicPoolModel
{
    /// <summary>
    /// 能量图标。
    /// </summary>
    public override string EnergyColorName => "ironclad";
}

/// <summary>
/// 占位符职业的卡牌模型。
/// </summary>
[Pool(typeof(MonkCardPool))]
public class MonkCard() : CustomCardModel(0, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy);

/// <summary>
/// 占位符职业的遗物模型。
/// </summary>
[Pool(typeof(MonkRelicPool))]
public class MonkRelic : CustomRelicModel
{
    /// <inheritdoc />
    public override RelicRarity Rarity => RelicRarity.Starter;
}