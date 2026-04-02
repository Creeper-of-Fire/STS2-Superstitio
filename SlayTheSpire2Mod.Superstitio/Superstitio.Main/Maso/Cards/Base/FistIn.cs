using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
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
using Superstitio.Main.Maso.Base;

namespace Superstitio.Main.Maso.Cards.Base;

/// <summary>
/// 选择一张手牌，失去其当前费用4-3倍的血量，强制打出。挂起，打出任意牌 3 次后，抽 1 张牌。
/// </summary>
public class FistIn() : MasoBaseCard(new CardInitMessage
{
    BaseCost = 0,
    Type = CardType.Skill,
    Rarity = CardRarity.Basic,
    Target = TargetType.Self
}), IWithHangingConfig
{
    /// <inheritdoc />
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new HpLossVar(BaseHpLoss),
        new HangingTriggerVar(Wait),
    ];

    private const int BaseHpLoss = 4;
    private const int UpgradeHpLoss = -1;
    private const int DrawCard = 1;
    private const int Wait = 3;

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
        var selectedCard = (await CardSelectCmd.FromHand(prefs: new CardSelectorPrefs(this.SelectionScreenPrompt, 1),
            context: choiceContext, player: this.Owner, filter: null, source: this)).FirstOrDefault();

        if (selectedCard != null)
        {
            int costEnergy = selectedCard.EnergyCost.GetAmountToSpend();

            await CreatureCmd.Damage(choiceContext, this.Owner.Creature, this.DynamicVars.HpLoss.BaseValue * costEnergy,
                ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move, this);

            if (!CombatManager.Instance.IsOverOrEnding && this.CombatState is not null)
            {
                if (selectedCard.TargetType == TargetType.AnyEnemy)
                {
                    var target = this.Owner.RunState.Rng.CombatTargets.NextItem(this.CombatState.HittableEnemies);
                    await CardCmd.AutoPlay(choiceContext, selectedCard, target);
                }
                else
                {
                    await CardCmd.AutoPlay(choiceContext, selectedCard, null);
                }
            }
        }

        // 创建挂起令牌
        var token = new AutoHangingCardTokenWithConfig(
            this.HangingCardConfig,
            base.GetResultPileType())
        {
            ShouldManualRemoveFromBattle = false,
            HangingAction = async (context, _) => { await CardPileCmd.Draw(context, DrawCard, this.Owner, fromHandDraw: true); }
        };

        // 挂起自身
        await HangingCardManager.HangCard(token, this);
    }

    /// <inheritdoc />
    protected override void OnUpgrade()
    {
        this.DynamicVars.HpLoss.UpgradeValueBy(UpgradeHpLoss);
    }
}