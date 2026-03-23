using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Superstitio.Main.Base;
using Superstitio.Main.DynamicVars;
using Superstitio.Main.DynamicVars.Extensions;
using Superstitio.Main.Features.HangingCard;

namespace Superstitio.Main.Maso.Cards.Kongfu;

/// <summary>
/// 悬刃 - 打出后挂起自身，后续每次打出攻击牌时抽1张牌（可触发2/3次）
/// </summary>
public sealed class SuspendingStrike() : MasoBaseCard(new CardInitMessage
{
    BaseCost = 1,
    Type = CardType.Attack,
    Rarity = CardRarity.Uncommon,
    Target = TargetType.AnyEnemy,
}), IWithHangingConfig
{
    /// <summary>
    /// 基础触发次数
    /// </summary>
    private const int BaseTriggerCount = 2;

    /// <summary>
    /// 升级增加的触发次数
    /// </summary>
    private const int UpgradeTriggerCount = 1;

    /// <inheritdoc />
    protected override HashSet<CardTag> CanonicalTags =>
    [
        CardTag.Strike
    ];

    /// <inheritdoc />
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(4, ValueProp.Move),
        new HangingTriggerVar(BaseTriggerCount),
    ];

    /// <inheritdoc />
    protected override PileType GetResultPileType()
    {
        return PileType.None;
    }

    /// <inheritdoc />
    public HangingCardConfig HangingCardConfig => new(
        Card: this,
        HangingType: HangingType.Follow,
        TriggerCount: this.DynamicVars.TriggerCount,
        CardTypeFilter: CardType.Attack
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
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await DamageCmd.Attack(this.DynamicVars.Damage.BaseValue).FromCard(this)
            .Targeting(cardPlay.Target).WithHitFx("vfx/vfx_attack_slash").Execute(choiceContext);

        var token = new AutoHangingCardTokenWithConfig(this.HangingCardConfig, base.GetResultPileType())
        {
            ShouldManualRemoveFromBattle = false, // 不手动移除，而是在打出后自行移除。
            HangingAction = async (context, _) =>
            {
                // 抽一张牌
                await CardPileCmd.Draw(context, 1, this.Owner, fromHandDraw: true);
            },
        };

        await HangingCardManager.HangCard(token, this);
    }

    /// <inheritdoc />
    protected override void OnUpgrade()
    {
        this.DynamicVars.Damage.UpgradeValueBy(2M);
        // 升级后触发次数增加，需要更新动态变量
        this.DynamicVars.TriggerCount.UpgradeValueBy(UpgradeTriggerCount);
    }
}