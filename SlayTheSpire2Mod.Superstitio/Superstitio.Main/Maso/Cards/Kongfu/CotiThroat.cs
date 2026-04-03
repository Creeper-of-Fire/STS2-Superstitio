using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Superstitio.Main.Base;
using Superstitio.Main.DynamicVars;
using Superstitio.Main.Extensions;
using Superstitio.Main.Features.HangingCard;
using Superstitio.Main.Maso.Base;

namespace Superstitio.Main.Maso.Cards.Kongfu;

/// <summary>
/// 打出后挂起自身，后续每次打出牌时触发一次额外攻击（可触发3次）
/// </summary>
public sealed class CotiThroat() : MasoBaseCard(new CardInitMessage
{
    BaseCost = 2,
    Type = CardType.Attack,
    Rarity = CardRarity.Common,
    Target = TargetType.AnyEnemy,
}), IWithHangingConfigCard
{
    /// <summary>
    /// 基础触发次数
    /// </summary>
    private const int TriggerCount = 3;

    /// <summary>
    /// 基础伤害
    /// </summary>
    private const int Damage = 6;

    /// <summary>
    /// 升级增加的伤害
    /// </summary>
    private const int DamageUpgrade = 3;

    /// <inheritdoc />
    protected override IEnumerable<DynamicVarWithUpgrade> InitVarsWithUpgrade =>
    [
        new DamageVar(Damage, ValueProp.Move).WithUpgrade(DamageUpgrade),
        new TriggerCountVar(TriggerCount) // 升级后触发次数不变，保持3次
    ];

    /// <inheritdoc />
    public HangingCardConfig HangingCardConfig => new(
        Card: this,
        HangingType: HangingType.Follow,
        TriggerCount: new TriggerCountVar(TriggerCount),
        CardTypeFilter: CardType.None
    );

    /// <inheritdoc />
    public bool HangingSelfAfterPlay => true;

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        // 执行本次攻击
        await DamageCmd.Attack(this.DynamicVars.Damage.BaseValue).FromCard(this)
            .Targeting(cardPlay.Target).WithHitFx("vfx/vfx_attack_slash").Execute(choiceContext);

        // 创建挂起令牌
        var token = this.CreateHangingToken(async (context, play) =>
        {
            if (this.CombatState is null)
                return;

            // 触发一次攻击到原目标
            await DamageCmd.Attack(this.DynamicVars.Damage.BaseValue).FromCard(this)
                .TryTargetingOrRandom(play.Target, this.CombatState)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(context);
        });

        await HangingCardManager.HangCard(token, this);
    }
}