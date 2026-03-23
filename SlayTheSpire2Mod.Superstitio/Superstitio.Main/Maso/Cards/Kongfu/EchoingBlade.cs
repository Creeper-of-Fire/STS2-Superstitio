using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Superstitio.Main.Base;
using Superstitio.Main.DynamicVars;
using Superstitio.Main.Features.HangingCard;

namespace Superstitio.Main.Maso.Cards.Kongfu;

/// <summary>
/// 回响之刃 - 打出后挂起自身，后续每次打出牌时触发一次额外攻击（可触发3次）
/// </summary>
public sealed class EchoingBlade() : MasoBaseCard(new CardInitMessage
{
    BaseCost = 2,
    Type = CardType.Attack,
    Rarity = CardRarity.Common,
    Target = TargetType.AnyEnemy,
}), IWithHangingConfig
{
    /// <summary>
    /// 基础触发次数
    /// </summary>
    private const int BaseTriggerCount = 3;

    /// <summary>
    /// 基础伤害
    /// </summary>
    private const int BaseDamage = 6;

    /// <summary>
    /// 升级增加的伤害
    /// </summary>
    private const int UpgradeDamage = 3;

    /// <inheritdoc />
    protected override HashSet<CardTag> CanonicalTags =>
    [
        CardTag.Strike
    ];

    /// <inheritdoc />
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(BaseDamage, ValueProp.Move),
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
        TriggerCount: new HangingTriggerVar(BaseTriggerCount),
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
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        // 执行本次攻击
        await DamageCmd.Attack(this.DynamicVars.Damage.BaseValue).FromCard(this)
            .Targeting(cardPlay.Target).WithHitFx("vfx/vfx_attack_slash").Execute(choiceContext);

        // 创建挂起令牌
        var token = new AutoHangingCardTokenWithConfig(this.HangingCardConfig, base.GetResultPileType())
        {
            ShouldManualRemoveFromBattle = false,
            HangingAction = async (context, play) =>
            {
                var target = play.Target;

                if (target is { IsAlive: true, IsEnemy: true })
                {
                    // 触发一次攻击到原目标
                    await DamageCmd.Attack(this.DynamicVars.Damage.BaseValue).FromCard(this)
                        .Targeting(target)
                        .WithHitFx("vfx/vfx_attack_slash")
                        .Execute(context);
                    return;
                }

                var combatState = this.CombatState;
                if (combatState is null)
                    return;

                // 如果原目标无效，选择随机敌人
                await DamageCmd.Attack(this.DynamicVars.Damage.BaseValue).FromCard(this)
                    .TargetingRandomOpponents(combatState)
                    .WithHitFx("vfx/vfx_attack_slash")
                    .Execute(context);
            }
        };

        await HangingCardManager.HangCard(token, this);
    }

    /// <inheritdoc />
    protected override void OnUpgrade()
    {
        this.DynamicVars.Damage.UpgradeValueBy(UpgradeDamage);
        // 升级后触发次数不变，保持3次
    }
}