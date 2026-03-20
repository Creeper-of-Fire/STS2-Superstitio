using System.Diagnostics.CodeAnalysis;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Superstitio.Main.Extension;

/// <summary>
/// 触发次数动态变量
/// </summary>
public class HangingTriggerVar(int triggers, string name = HangingTriggerVar.DefaultName) : DynamicVar(name, triggers)
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

/// <summary>
/// 动态变量集的扩展方法
/// </summary>
public static class DynamicVarSetExtensions
{
    /// <summary>
    /// 动态变量集的扩展方法
    /// </summary>
    extension(DynamicVarSet dynamicVarSet)
    {
        /// <summary>
        /// 获取指定类型的动态变量
        /// </summary>
        public bool TryGetVar<TVar>(string key, [MaybeNullWhen(false)] out TVar value) where TVar : DynamicVar
        {
            bool result = dynamicVarSet.TryGetValue(key, out var dynamicVar);
            value = dynamicVar as TVar;
            return result;
        }

        /// <summary>
        /// 获取触发次数动态变量
        /// </summary>
        public HangingTriggerVar? TriggerCount =>
            dynamicVarSet.TryGetVar<HangingTriggerVar>(HangingTriggerVar.DefaultName, out var triggerVar) ? triggerVar : null;
    }
}