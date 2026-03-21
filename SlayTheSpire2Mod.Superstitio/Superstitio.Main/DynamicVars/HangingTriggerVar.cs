using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace Superstitio.Main.DynamicVars;

/// <summary>
/// 触发次数动态变量
/// </summary>
public class HangingTriggerVar(int triggers, string name = HangingTriggerVar.DefaultName) : MegaCrit.Sts2.Core.Localization.DynamicVars.DynamicVar(name, triggers)
{
    /// <summary>
    /// 名称
    /// </summary>
    public const string DefaultName = nameof(HangingTriggerVar);

    /// <summary>
    /// 基础触发次数
    /// </summary>
    public int BaseTriggers { get; set; } = triggers;

    /// <summary>
    /// 预览/实际显示的触发次数
    /// </summary>
    public int PreviewTriggers { get; private set; } = triggers;

    /// <inheritdoc />
    public override void UpdateCardPreview(
        CardModel card,
        CardPreviewMode previewMode,
        Creature? target,
        bool runGlobalHooks)
    {
        // 基础值（考虑升级）
        int currentTriggers = this.BaseTriggers;

        // 如果有附魔，可以在这里处理附魔对触发次数的影响
        var enchantment = card.Enchantment;
        if (enchantment != null)
        {
            // 如果有附魔增加触发次数的逻辑，可以在这里实现
            // currentTriggers = (int)enchantment.EnchantTriggerCount(currentTriggers);
            if (!card.IsEnchantmentPreview)
                this.EnchantedValue = currentTriggers;
        }

        // 如果需要全局钩子修改触发次数，可以在这里调用
        // if (runGlobalHooks)
        // {
        //     currentTriggers = Hook.ModifyTriggerCount(card.CombatState, card, currentTriggers);
        // }

        this.PreviewTriggers = currentTriggers;
        this.PreviewValue = currentTriggers;
    }
}