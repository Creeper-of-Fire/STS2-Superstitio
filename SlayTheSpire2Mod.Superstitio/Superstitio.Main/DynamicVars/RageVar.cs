using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Superstitio.Api.DynamicVars.Extensions;
using Superstitio.Main.DynamicVars.Extensions;
using Superstitio.Main.Features.Felix;

namespace Superstitio.Main.DynamicVars;

/// <summary>
/// 快感动态变量
/// </summary>
public class FelixVar(int felixAmount, string? name = null) : PowerVar<FelixPower>(name ?? DefaultName, felixAmount)
{
    /// 默认名称
    public static readonly string DefaultName = FelixVar.DynamicVarName;

    /// <summary>
    /// 基础快感值
    /// </summary>
    public int BaseFelix { get; set; } = felixAmount;

    /// <summary>
    /// 预览/实际显示的快感值
    /// </summary>
    public int PreviewFelix { get; private set; } = felixAmount;

    /// <inheritdoc />
    public override void UpdateCardPreview(
        CardModel card,
        CardPreviewMode previewMode,
        Creature? target,
        bool runGlobalHooks)
    {
        // 基础值（考虑升级）
        int currentFelix = this.BaseFelix;

        // 如果有附魔，处理附魔对快感值的影响
        var enchantment = card.Enchantment;
        if (enchantment is not null)
        {
            // 假设附魔可以增加快感值
            // currentFelix = (int)enchantment.EnchantFelixValue(currentFelix);
            if (!card.IsEnchantmentPreview)
                this.EnchantedValue = currentFelix;
        }

        // 如果需要全局钩子修改快感值，可以在这里调用
        // if (runGlobalHooks)
        // {
        //     currentFelix = Hook.ModifyFelixValue(card.CombatState, card, currentFelix);
        // }

        this.PreviewFelix = currentFelix;
        this.PreviewValue = currentFelix;
    }
}