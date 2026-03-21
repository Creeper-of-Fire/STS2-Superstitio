using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Superstitio.Main.DynamicVars;
using Superstitio.Main.Features.HangingCard;

namespace Superstitio.Main.Maso.Cards.Kongfu;

/// <summary>
/// 回响之刃 - 打出后挂起自身，后续每次打出攻击牌时触发一次额外攻击（可触发3次）
/// </summary>
public sealed class EchoingBlade() : MasoBaseCard(new()
{
    BaseCost = 2,
    Type = CardType.Attack,
    Rarity = CardRarity.Common,
    Target = TargetType.AnyEnemy,
})
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

    /// <summary>
    /// 回响之刃挂起令牌：记录被挂起的卡牌，后续打出攻击牌时触发额外攻击
    /// </summary>
    public record EchoingBladeToken : AutoHangingCardToken
    {
        /// <summary>
        /// 触发时造成的基础伤害值
        /// </summary>
        public required decimal DamageBaseValue { get; init; }

        /// <summary>
        /// 只响应攻击牌
        /// </summary>
        protected override bool ShouldRespond(PlayerChoiceContext context, CardPlay cardPlay) =>
            cardPlay.Card.Type == CardType.Attack;

        /// <inheritdoc />
        protected override async Task OnTrigger(PlayerChoiceContext context, CardPlay cardPlay)
        {
            var target = cardPlay.Target;

            if (target is { IsAlive: true })
            {
                // 触发一次攻击到原目标
                await DamageCmd.Attack(this.DamageBaseValue).FromCard(this.HangingCard)
                    .Targeting(target)
                    .WithHitFx("vfx/vfx_attack_slash")
                    .Execute(context);
                return;
            }

            var combatState = this.OriginalOwner.Creature.CombatState;
            if (combatState is null)
                return;

            // 如果原目标无效，选择随机敌人
            await DamageCmd.Attack(this.DamageBaseValue).FromCard(this.HangingCard)
                .TargetingRandomOpponents(combatState)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(context);
        }
    }

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        // 执行本次攻击
        await DamageCmd.Attack(this.DynamicVars.Damage.BaseValue).FromCard(this)
            .Targeting(cardPlay.Target).WithHitFx("vfx/vfx_attack_slash").Execute(choiceContext);

        // 创建挂起令牌
        var token = new EchoingBladeToken
        {
            HangingCard = this,
            OriginalOwner = this.Owner,
            RemainCount = BaseTriggerCount,
            InitialCount = BaseTriggerCount,
            ShouldManualRemoveFromBattle = false,
            ReturnPileType = base.GetResultPileType(),
            DamageBaseValue = this.DynamicVars.Damage.BaseValue, // 记录当前伤害值
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