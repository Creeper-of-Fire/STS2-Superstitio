using Superstitio.Api.BaseLib;
using Superstitio.Api.Power;
using Superstitio.Main.Resource;

namespace Superstitio.Main.Base;

/// <summary>
/// 
/// </summary>
public abstract class SuperstitioBasePower(PowerInitMessage powerInitMessage) : BasePower(powerInitMessage)
{
    /// <inheritdoc />
    public override string CustomBigIconPath => ResourceUtils.GetPowerPortraitPath(this);

    /// <inheritdoc />
    public override string CustomPackedIconPath => ResourceUtils.GetPowerPortraitPath(this);
}