using BaseLib.Abstracts;

namespace Superstitio.Main.Maso.Pools;

/// <summary>
/// 职业的药水池模型。
/// </summary>
public class MasoPotionPool : CustomPotionPoolModel
{
    /// <summary>
    /// 能量图标。
    /// </summary>
    public override string EnergyColorName => "ironclad";
}