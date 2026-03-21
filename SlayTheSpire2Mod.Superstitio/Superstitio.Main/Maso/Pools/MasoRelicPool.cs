using BaseLib.Abstracts;

namespace Superstitio.Main.Maso.Pools;

/// <summary>
/// 职业的遗物池模型。
/// </summary>
public class MasoRelicPool : CustomRelicPoolModel
{
    /// <summary>
    /// 能量图标。
    /// </summary>
    public override string EnergyColorName => "ironclad";
}