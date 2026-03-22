using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;

namespace Superstitio.Main.Features.Rage;

/// <summary>
/// 怒火爆发带来的减费效果
/// 层数 = 减费数值 = 可使用的次数
/// </summary>
public class RageDiscountPower : CustomPowerModel
{
    /// <inheritdoc />
    public override PowerType Type => PowerType.Buff;

    /// <inheritdoc />
    public override PowerStackType StackType => PowerStackType.Counter;

    /// <inheritdoc />
    public override bool IsInstanced => false;

    private bool IsCheckingRageDiscount { get; set; } = false;

    /// <summary>
    /// 战斗中修改费用的钩子
    /// </summary>
    public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        if (this.IsCheckingRageDiscount)
        {
            modifiedCost = originalCost;
            Log.Info("不修改能量1");
            return false;
        }

        if (card.EnergyCost.CostsX || card.Keywords.Contains(CardKeyword.Unplayable))
        {
            modifiedCost = originalCost;
            Log.Info("不修改能量2");
            return false;
        }

        // originalCost 是进入此 Hook 时的费用
        if (originalCost <= 0)
        {
            Log.Info("不修改能量3");
            modifiedCost = originalCost;
            return false;
        }

        modifiedCost = Math.Max(0, originalCost - this.Amount);
        Log.Info($"修改能量{modifiedCost}");
        return true;
    }

    /// <summary>
    /// 当卡牌打出后，判定是否消耗层数
    /// </summary>
    /// <inheritdoc />
    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var card = cardPlay.Card;

        if (card.EnergyCost.CostsX || card.Keywords.Contains(CardKeyword.Unplayable))
        {
            return;
        }

        // 我们需要知道：如果没有这个 RageDiscountPower，这张牌要花多少钱？
        this.IsCheckingRageDiscount = true; // 临时关掉折扣

        decimal costWithoutMe;

        try
        {
            // 重新获取包含所有其他全局修正但不包含本能力的费用
            costWithoutMe = card.EnergyCost.GetWithModifiers(CostModifiers.All);
        }
        finally
        {
            this.IsCheckingRageDiscount = false; // 恢复折扣
        }

        // 如果没有我，这张牌的费用本来是 > 0 的，说明我确实生效了
        if (costWithoutMe > 0)
        {
            this.Flash();
            await PowerCmd.Decrement(this);
        }
    }
}