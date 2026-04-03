using Superstitio.Main.Base;

namespace Superstitio.Main.Features.HangingCard;

/// <summary>
/// 一张卡，具有挂起效果
/// </summary>
public interface IWithHangingConfig
{
    /// <summary>
    /// 挂起卡的配置
    /// </summary>
    public HangingCardConfig HangingCardConfig { get; }
}

/// <summary>
/// 一张卡，具有挂起效果
/// 提供额外的配置能力
/// </summary>
public interface IWithHangingConfigCard : IWithHangingConfig
{
    /// <summary>
    /// 是否在打出后挂起自身
    /// </summary>
    public bool HangingSelfAfterPlay { get; }
}