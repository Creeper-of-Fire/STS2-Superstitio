namespace Superstitio.Api.HangingCard;

/// <summary>
/// 挂起类型
/// </summary>
public enum HangingType
{
    /// <summary>
    /// 伴随：每次触发时生效，直到次数耗尽
    /// </summary>
    Follow,

    /// <summary>
    /// 延缓：累积触发次数后生效一次
    /// </summary>
    Delay
}