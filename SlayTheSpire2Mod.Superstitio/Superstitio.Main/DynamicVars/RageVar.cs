using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace Superstitio.Main.DynamicVars;

/// <summary>
/// 怒火动态变量
/// </summary>
public class RageVar(int rageAmount, string name = RageVar.DefaultName) : MegaCrit.Sts2.Core.Localization.DynamicVars.DynamicVar(name, rageAmount)
{
    /// <summary>
    /// 名称
    /// </summary>
    public const string DefaultName = nameof(RageVar);

    /// <summary>
    /// 基础怒火值
    /// </summary>
    public int BaseRage { get; set; } = rageAmount;

    /// <summary>
    /// 预览/实际显示的怒火值
    /// </summary>
    public int PreviewRage { get; private set; } = rageAmount;

    /// <inheritdoc />
    public override void UpdateCardPreview(
        CardModel card,
        CardPreviewMode previewMode,
        Creature? target,
        bool runGlobalHooks)
    {
        // 基础值（考虑升级）
        int currentRage = this.BaseRage;

        // 如果有附魔，处理附魔对怒火值的影响
        var enchantment = card.Enchantment;
        if (enchantment != null)
        {
            // 假设附魔可以增加怒火值
            // currentRage = (int)enchantment.EnchantRageValue(currentRage);
            if (!card.IsEnchantmentPreview)
                this.EnchantedValue = currentRage;
        }

        // 如果需要全局钩子修改怒火值，可以在这里调用
        // if (runGlobalHooks)
        // {
        //     currentRage = Hook.ModifyRageValue(card.CombatState, card, currentRage);
        // }

        this.PreviewRage = currentRage;
        this.PreviewValue = currentRage;
    }
}