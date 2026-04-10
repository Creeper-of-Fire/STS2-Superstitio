using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Models;
using Superstitio.Main.SubPool;
using Superstitio.Main.SubPool.UI;
using Superstitio.Main.Utils;

namespace Superstitio.Main.Lupa.Base;

/// <summary>
/// 职业的卡牌池模型。
/// </summary>
public class LupaCardPool : CustomCardPoolModel, IWithSubPool<LupaCardPool>
{
    /// <summary>
    /// 卡池的 ID。必须唯一，防撞车。
    /// </summary>
    public override string Title => $"{nameof(LupaCardPool)}_{Plugin.ModGuid}";

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
    public override Color DeckEntryCardColor => Color.Color8(255, 105, 180);

    /// <summary>
    /// 卡牌颜色
    /// </summary>
    public override Color ShaderColor => this.DeckEntryCardColor.CalculateRequiredInputColor();

    /// <summary>
    /// 卡池是否是无色。例如事件、状态等卡池就是无色的。
    /// </summary>
    public override bool IsColorless => false;

    // /// <summary>
    // /// 完全可以替换这里，达成卡池选择的效果。
    // /// </summary>
    // public override IEnumerable<CardModel> AllCards => this.GetAllSubPoolCards;
}

/// <summary>
/// Lupa的卡牌池
/// </summary>
public class LupaSubPool : SubPool<LupaSubPool>;

/// <summary>
/// 职业的药水池模型。
/// </summary>
public class LupaPotionPool : CustomPotionPoolModel
{
    /// <summary>
    /// 能量图标。
    /// </summary>
    public override string EnergyColorName => "ironclad";
}

/// <summary>
/// 职业的遗物池模型。
/// </summary>
public class LupaRelicPool : CustomRelicPoolModel
{
    /// <summary>
    /// 能量图标。
    /// </summary>
    public override string EnergyColorName => "ironclad";
}