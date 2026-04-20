using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.TextEffects;
using Superstitio.Api.CustomKeywords;
using Superstitio.Api.Utils;

namespace Superstitio.Api.DynamicVars;

/// <summary>
/// 
/// </summary>
public class CardTitleVar(Type cardType, int initUpgradeTimes = 0) : DynamicVar(cardType.Name, initUpgradeTimes), IHighlightableDynamicVar
{
    /// <inheritdoc />
    public override string ToString() => CardUtils.GetUpgradeCardClone(cardType, this.IntValue).Title;

    /// <summary>
    /// 悬浮提示
    /// </summary>
    public IHoverTip HoverTip => HoverTipFactory.FromCard(CardUtils.GetUpgradeCardClone(cardType, this.IntValue));

    /// <inheritdoc />
    public string OverrideToHighlightedString(bool inverse)
    {
        return StsTextUtilities.HighlightChangeText(baseComparison: this.WasJustUpgraded ? 1 : 0, text: this.ToString());
    }
}

/// <summary>
/// 
/// </summary>
public class CardTitleVar<TCard>(int upgradeTimes = 0) : CardTitleVar(typeof(TCard), upgradeTimes) where TCard : CardModel;