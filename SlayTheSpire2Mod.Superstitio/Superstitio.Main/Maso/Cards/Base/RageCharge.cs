using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Superstitio.Main.Base;
using Superstitio.Main.DynamicVars;
using Superstitio.Main.DynamicVars.Extensions;
using Superstitio.Main.Features.HangingCard;
using Superstitio.Main.Features.Rage;

namespace Superstitio.Main.Maso.Cards.Base;

/// <summary>
/// 蓄势待发 - 0费技能，挂起自身，获得4(6)点怒火。打出任意2张牌后，抽1张牌。
/// </summary>
public class RageCharge() : MasoBaseCard(new CardInitMessage
{
    BaseCost = 0,
    Type = CardType.Skill,
    Rarity = CardRarity.Uncommon,
    Target = TargetType.Self,
}), IWithHangingConfig
{
    /// <summary>
    /// 基础怒火值
    /// </summary>
    private const int BaseRageGain = 4;

    /// <summary>
    /// 升级增加的怒火值
    /// </summary>
    private const int UpgradeRageIncrease = 2;

    /// <summary>
    /// 触发抽牌所需打出的牌数
    /// </summary>
    private const int TriggerThreshold = 2;

    /// <inheritdoc />
    protected override HashSet<CardTag> CanonicalTags => [];

    /// <inheritdoc />
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new RageVar(BaseRageGain),
        new HangingTriggerVar(TriggerThreshold),
    ];

    /// <inheritdoc />
    protected override PileType GetResultPileType()
    {
        return PileType.None; // 挂起后不进入弃牌堆
    }

    /// <inheritdoc />
    public HangingCardConfig HangingCardConfig => new(
        Card: this,
        HangingType: HangingType.Delay,
        TriggerCount: this.DynamicVars.TriggerCount,
        CardTypeFilter: CardType.None
    );

    /// <inheritdoc />
    protected override void AddExtraArgsToDescription(LocString description)
    {
        base.AddExtraArgsToDescription(description);
        HangingDescriptionBuilder.AddExtraArgsToDescription(
            description,
            this.HangingCardConfig
        );
    }

    /// <inheritdoc />
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        ..base.ExtraHoverTips,
        ..HangingDescriptionBuilder.GetHoverTips(this.HangingCardConfig, showHangingTotalDescription: true),
    ];

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 获取怒火值
        decimal rageAmount = this.DynamicVars.Rage.BaseValue;

        // 使用 RageManager 增加怒火
        await RageManager.ModifyRage(this.Owner.Creature, rageAmount, this.Owner.Creature, cardPlay.Card);

        // 创建挂起令牌
        var token = new AutoHangingCardTokenWithConfig(
            this.HangingCardConfig,
            base.GetResultPileType())
        {
            ShouldManualRemoveFromBattle = false,
            HangingAction = async (context, _) =>
            {
                // 抽 1 张牌
                await CardPileCmd.Draw(context, 1, this.Owner, fromHandDraw: true);
            }
        };

        // 挂起自身
        await HangingCardManager.HangCard(token, this);
    }

    /// <inheritdoc />
    protected override void OnUpgrade()
    {
        // 升级：增加获得的怒火值
        this.DynamicVars.Rage.UpgradeValueBy(UpgradeRageIncrease);
    }
}