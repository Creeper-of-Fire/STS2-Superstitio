using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Superstitio.Main.Extension;
using Superstitio.Main.Maso.HangingCard;

namespace Superstitio.Main.Maso.Cards.Kongfu;

/// <summary>
/// 悬刃 - 打出后挂起自身，后续每次打出攻击牌时抽1张牌（可触发2/3次）
/// </summary>

public sealed class SuspendingStrike() : MasoBaseCard(new()
{
    BaseCost = 1,
    Type = CardType.Attack,
    Rarity = CardRarity.Uncommon,
    Target = TargetType.AnyEnemy,
})
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
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await DamageCmd.Attack(this.DynamicVars.Damage.BaseValue).FromCard(this)
            .Targeting(cardPlay.Target).WithHitFx("vfx/vfx_attack_slash").Execute(choiceContext);

        var token = new SuspendingStrikeToken
        {
            HangingCard = this,
            OriginalOwner = this.Owner,
            RemainCount = BaseTriggerCount,
            InitialCount = BaseTriggerCount,
            ShouldManualRemoveFromBattle = false, // 不手动移除，而是在打出后自行移除。
            ReturnPileType = base.GetResultPileType(), // 回到的牌堆为父类默认值
        };

        await HangingCardManager.HangCard(token, this);
    }

    /// <inheritdoc />
    protected override void OnUpgrade()
    {
        this.DynamicVars.Damage.UpgradeValueBy(2M);
        // 升级后触发次数增加，需要更新动态变量
        this.DynamicVars.TriggerCount?.UpgradeValueBy(UpgradeTriggerCount);
    }
}

/// <summary>
/// 挂起令牌：记录被挂起的卡牌及其触发次数
/// </summary>
public record SuspendingStrikeToken : AutoHangingCardToken
{
    /// <summary>
    /// 只响应攻击牌
    /// </summary>
    /// <inheritdoc />
    protected override bool ShouldRespond(PlayerChoiceContext context, CardPlay cardPlay) =>
        cardPlay.Card.Type == CardType.Attack;

    /// <inheritdoc />
    protected override async Task OnTrigger(PlayerChoiceContext context, CardPlay cardPlay)
    {
        // 抽一张牌
        await CardPileCmd.Draw(context, 1, this.OriginalOwner, fromHandDraw: true);
    }
}