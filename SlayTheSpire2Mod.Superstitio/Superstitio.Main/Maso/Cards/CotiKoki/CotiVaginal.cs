using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Superstitio.Main.Base;
using Superstitio.Main.Extensions;
using Superstitio.Main.Features.HangingCard;
using Superstitio.Main.Features.HangingCard.UI;
using Superstitio.Main.Maso.Base;

namespace Superstitio.Main.Maso.Cards.CotiKoki;

/**
 * Title = "性交-插入：阴道"
 *
 * Description = "对敌人造成{Damage:diff()}点伤害。触发所有[orange]伴随[/orange]效果。"
 *
 * Flavor = "朴实无华。如果没有其他东西助兴的话就会困得想睡觉。"
 *
 * Sfw.Title = "燕形拳"
 *
 * Sfw.Flavor = "矫健如燕，在空中穿梭，引动后续的所有杀招。"
 */
public class CotiVaginal() : MasoBaseCard(new CardInitMessage
{
    BaseCost = 1,
    Type = CardType.Attack,
    Rarity = CardRarity.Uncommon,
    Target = TargetType.AnyEnemy,
}), ISimpleHangingCardHighlighter, ICanDoStuffWithHangingQueue
{
    /// <inheritdoc/>
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
    [
        new DamageVar(8, ValueProp.Move).WithUpgrade(4)
    ];

    /// <inheritdoc />
    public Func<HangingCardToken, bool> TokenIsAble => it => it is AutoHangingCardTokenWithConfig
    {
        HangingCardConfig.HangingType: HangingType.Follow
    };

    /// <inheritdoc />
    public Func<HangingCardToken, HangingTriggerContext, HangingTriggerResult?, HangingTriggerResult?>
        SimpleChangeTriggerResult => (_, _, originResult) =>
    {
        if (originResult is null)
            return new HangingTriggerResult(HangGlowType.Preview, null);
        if (originResult.Value.GlowType == HangGlowType.None) 
            return originResult;
        return originResult.Value with { GlowType = HangGlowType.Preview };
    };

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.AutoAttack(this, cardPlay).Execute(choiceContext);

        var configs = HangingCardManager.GetHangingCardTokens(this).OfType<AutoHangingCardTokenWithConfig>();

        foreach (var config in configs)
        {
            await config.HangingAction(choiceContext, cardPlay);
        }
    }
}