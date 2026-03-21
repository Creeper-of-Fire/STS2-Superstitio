using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Superstitio.Main.DynamicVars;
using Superstitio.Main.DynamicVars.Extensions;
using Superstitio.Main.Features.HangingCard;
using Superstitio.Main.Features.Rage;

namespace Superstitio.Main.Maso.Cards.Base;

/// <summary>
/// 蓄势待发 - 0费技能，挂起自身，获得4(6)点怒火。打出任意2张牌后，抽1张牌。
/// </summary>
public class RageCharge() : MasoBaseCard(new()
{
    BaseCost = 0,
    Type = CardType.Skill,
    Rarity = CardRarity.Uncommon,
    Target = TargetType.Self,
})
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


    /// <summary>
    /// 挂起令牌：记录打出任意2张牌后抽1张牌
    /// </summary>
    public record GatheringMomentumToken : AutoHangingCardToken
    {
        /// <summary>
        /// 响应任意卡牌打出（不限制类型）
        /// </summary>
        protected override bool ShouldRespond(PlayerChoiceContext context, CardPlay cardPlay) => true;

        /// <inheritdoc />
        protected override async Task OnEnd(PlayerChoiceContext context, CardPlay cardPlay)
        {
            // 抽1张牌
            await CardPileCmd.Draw(context, 1, this.OriginalOwner, fromHandDraw: true);
        }
    }

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 获取怒火值
        decimal rageAmount = this.DynamicVars.Rage.BaseValue;

        // 使用 RageManager 增加怒火
        await RageManager.ModifyRage(this.Owner.Creature, rageAmount, this.Owner.Creature, cardPlay.Card);

        // 创建挂起令牌
        var token = new GatheringMomentumToken
        {
            HangingCard = this,
            OriginalOwner = this.Owner,
            RemainCount = TriggerThreshold,
            InitialCount = TriggerThreshold,
            ShouldManualRemoveFromBattle = false,
            ReturnPileType = base.GetResultPileType(),
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