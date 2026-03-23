using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Models;
using Superstitio.Main.Extensions;

namespace Superstitio.Main.Features.HangingCard;

/// <summary>
/// 在这里做一层抽象，实际上我们用 power 或者其他类似物实现功能，但是我们通过一个专门的悬挂 API 调用。
/// 虽然理论上我们可以做的非常复杂，但是我的建议是：只有一个钩子数据。
/// </summary>
public static class HangingCardManager
{
    /// <summary>
    /// 悬挂一张卡牌。
    /// 获取可变的 <see cref="HangingCardPower"/> 实例，执行挂起操作，并应用相应的力量命令。
    /// </summary>
    /// <param name="hangingCardToken">代表一次挂起的特殊类。</param>
    /// <param name="cardReason">导致悬挂原因的卡牌模型（可选）。</param>
    /// <returns>一个表示异步操作完成的任务。</returns>
    public static async Task HangCard(HangingCardToken hangingCardToken, CardModel? cardReason)
    {
        var hangingPower = ModelDb.GetMutablePower<HangingCardPower>();

        await hangingPower.HangCard(hangingCardToken);

        await PowerCmd.Apply(
            hangingPower,
            hangingCardToken.HangingCard.Owner.Creature,
            hangingCardToken.RemainCount,
            cardReason?.Owner.Creature,
            cardReason
        );
    }
}