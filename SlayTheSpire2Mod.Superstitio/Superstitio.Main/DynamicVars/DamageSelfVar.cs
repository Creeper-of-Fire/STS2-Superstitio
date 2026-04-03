using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Superstitio.Main.DynamicVars.Extensions;

namespace Superstitio.Main.DynamicVars;

/// <summary>
/// 对自身造成伤害
/// </summary>
public class DamageSelfVar(decimal damage, ValueProp props, string? name = null) : DamageVar(name ?? DefaultName, damage, props)
{
    /// 默认名称
    public static readonly string DefaultName = DamageSelfVar.DynamicVarName;

    /// <inheritdoc />
    public override void UpdateCardPreview(
        CardModel card,
        CardPreviewMode previewMode,
        Creature? target,
        bool runGlobalHooks)
    {
        base.UpdateCardPreview(card, previewMode, card.Owner.Creature, runGlobalHooks);
    }
}