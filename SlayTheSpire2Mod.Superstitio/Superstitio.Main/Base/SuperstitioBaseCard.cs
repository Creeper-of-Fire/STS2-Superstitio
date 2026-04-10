using System.Reflection;
using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Superstitio.Main.Extensions;
using Superstitio.Main.Features.HangingCard;
using Superstitio.Main.Utils;

namespace Superstitio.Main.Base;

/// <summary>
/// 
/// </summary>
/// <param name="cardInitMessage"></param>
public abstract class SuperstitioBaseCard(CardInitMessage cardInitMessage) : CustomCardModel(
    cardInitMessage.BaseCost,
    cardInitMessage.Type,
    cardInitMessage.Rarity,
    cardInitMessage.Target,
    cardInitMessage.ShowInCardLibrary,
    cardInitMessage.AutoAdd
)
{
    private static string GetCardTypeString(CardModel card)
    {
        if (card.Rarity == CardRarity.Basic)
            return "base";
        return card.Type switch
        {
            CardType.Attack => "attack",
            CardType.Skill => "skill",
            CardType.Power => "power",
            _ => "special"
        };
    }

    private string GetCardPortraitPath()
    {
        var type = this.GetType();
        string imgPrefix = ResourceUtils.GetImgPrefix(type);

        // 获取自定义名称逻辑
        var nameAttr = type.GetCustomAttribute<CustomImgNameAttribute>();
        string fileName = nameAttr != null ? nameAttr.Name : type.Name;

        string cardTypeStr = GetCardTypeString(this);

        string path = $"{imgPrefix}/cards/{cardTypeStr}/{fileName}.png";

        if (ResourceLoader.Exists(path))
            return path;

        string defaultImg = $"{imgPrefix}/cards/default.png";

        return defaultImg;
    }


    /// <inheritdoc />
    public override string PortraitPath => this.GetCardPortraitPath();

    /// <summary>
    /// 定义卡牌的标准标签集合。
    /// </summary>
    protected override HashSet<CardTag> CanonicalTags => [];

    /// <summary>
    /// 定义卡牌的动态变量集合。
    /// </summary>
    protected override IEnumerable<DynamicVar> CanonicalVars => this.InitVarsWithUpgrade.Select(it => it.DynamicVar);

    /// <summary>
    /// 为卡牌定义动态变量集合（带升级描述）。
    /// </summary>
    protected virtual IEnumerable<DynamicVarSpec> InitVarsWithUpgrade => [];

    /// <summary>
    /// 卡牌升级效果。
    /// </summary>
    protected override void OnUpgrade()
    {
        // 这里因为是重复获取，所以性能会低一点，但是，这只是在升级时重新获取一次，就不做缓存了，不缺这三瓜两枣。
        foreach (var dynamicVarWithUpgrade in this.InitVarsWithUpgrade)
        {
            if (this.DynamicVars.TryGetValue(dynamicVarWithUpgrade.DynamicVar.Name, out var dynamicVar))
                dynamicVar.UpgradeValueBy(dynamicVarWithUpgrade.UpgradeValue);
        }
    }

    /// <summary>
    /// 执行卡牌效果。
    /// </summary>
    /// <param name="choiceContext">玩家选择上下文。</param>
    /// <param name="cardPlay">执行相关信息。</param>
    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        return Task.CompletedTask;
    }


    /// <summary>
    /// 卡牌的风味文本
    /// </summary>
    public LocString Flavor => new("cards", this.Id.Entry + ".flavor");

    /// <inheritdoc />
    protected override void AddExtraArgsToDescription(LocString description)
    {
        base.AddExtraArgsToDescription(description);

        if (this is IWithHangingConfig withHangingConfig)
            HangingDescriptionBuilder.AddExtraArgsToDescription(description, withHangingConfig.HangingCardConfig);
    }

    /// <inheritdoc />
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips
        .TryAddTip(this.Flavor)
        .TryAddTip(this.InitVarsWithUpgrade.SelectMany(it => it.ExtraHoverTips))
        .TryAddTip(this is IWithHangingConfig withHangingConfig
            ? HangingDescriptionBuilder.GetHoverTips(withHangingConfig.HangingCardConfig, showHangingTotalDescription: true)
            : []);

    /// <inheritdoc />
    protected override PileType GetResultPileType()
    {
        if (this is IWithHangingConfigCard { HangingSelfAfterPlay: true })
            return PileType.None; // 挂起后不进入弃牌堆

        return base.GetResultPileType();
    }

    /// <summary>
    /// 基类打出后返回的牌堆
    /// </summary>
    public PileType BaseResultPileType => base.GetResultPileType();
}