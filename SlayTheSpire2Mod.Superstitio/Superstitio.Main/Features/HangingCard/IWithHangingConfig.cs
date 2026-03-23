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