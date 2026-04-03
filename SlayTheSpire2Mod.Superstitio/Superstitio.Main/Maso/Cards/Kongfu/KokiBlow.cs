using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Superstitio.Main.Base;
using Superstitio.Main.DynamicVars;
using Superstitio.Main.DynamicVars.Extensions;
using Superstitio.Main.Features.Felix;
using Superstitio.Main.Features.HangingCard;
using Superstitio.Main.Maso.Base;

namespace Superstitio.Main.Maso.Cards.Kongfu;

/// <summary>
/// 打出后挂起自身，后续每次打出攻击牌时获得2点快感
/// </summary>
public sealed class KokiBlow() : MasoBaseCard(new CardInitMessage
{
    BaseCost = 0,
    Type = CardType.Attack,
    Rarity = CardRarity.Uncommon,
    Target = TargetType.AnyEnemy,
}), IWithHangingConfigCard
{
    /// <summary>
    /// 基础触发次数
    /// </summary>
    private const int TriggerCount = 3;

    /// <summary>
    /// 升级增加的触发次数
    /// </summary>
    private const int TriggerCountUpgrade = 1;


    private const int Damage = 5;

    private const int DamageUpgrade = 3;

    private const int FelixGive = 2;

    /// <inheritdoc />
    protected override IEnumerable<DynamicVarWithUpgrade> InitVarsWithUpgrade =>
    [
        new DamageVar(Damage, ValueProp.Move).WithUpgrade(DamageUpgrade),
        new TriggerCountVar(TriggerCount).WithUpgrade(TriggerCountUpgrade),
        new FelixVar(FelixGive)
    ];

    /// <inheritdoc />
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        ..base.ExtraHoverTips,
        HoverTipFactory.FromPower<FelixPower>()
    ];

    /// <inheritdoc />
    public HangingCardConfig HangingCardConfig => new(
        Card: this,
        HangingType: HangingType.Follow,
        TriggerCount: this.DynamicVars.TriggerCount,
        CardTypeFilter: CardType.Attack
    );

    /// <inheritdoc />
    public bool HangingSelfAfterPlay => true;

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await DamageCmd.Attack(this.DynamicVars.Damage.BaseValue).FromCard(this)
            .Targeting(cardPlay.Target).WithHitFx("vfx/vfx_attack_slash").Execute(choiceContext);

        var token = this.CreateHangingToken(async (_, _) =>
        {
            await FelixManager.ModifyFelix(this.Owner.Creature, this.DynamicVars.Felix.BaseValue, this.Owner.Creature, this);
        });

        await HangingCardManager.HangCard(token, this);
    }
}