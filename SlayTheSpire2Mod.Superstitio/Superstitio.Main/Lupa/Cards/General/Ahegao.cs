using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Superstitio.Main.Base;
using Superstitio.Main.DynamicVars;
using Superstitio.Main.Extensions;
using Superstitio.Main.Features.HangingCard;
using Superstitio.Main.Features.HangingCard.UI;
using Superstitio.Main.Lupa.Base;

namespace Superstitio.Main.Lupa.Cards.General;

/**
 * Title = "阿嘿颜"
 *
 * Description = """
 * {CardHangingDescription}
 * """
 *
 * HangingEffect = "对敌方全体造成{Repeat:diff()}次{Damage:diff()}点伤害。"
 * 
 * Flavor = "想要保持专业的微笑，但是控制不住。"
 *
 * Sfw.Title = "连砍带顺劈"
 *
 * Sfw.Flavor = "把普通攻击变成全体二连击。"
 */
public class Ahegao() : LupaBaseCard(new CardInitMessage
{
    BaseCost = 2,
    Type = CardType.Attack,
    Rarity = CardRarity.Uncommon,
    Target = TargetType.AllEnemies
}), IWithHangingConfigCard
{
    /// <inheritdoc />
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
    [
        new DamageVar(2, ValueProp.Move).WithUpgrade(3),
        new RepeatVar(2),
        new TriggerCountVar(99),
    ];

    /// <inheritdoc />
    public HangingCardConfig HangingCardConfig => new(
        Card: this,
        HangingType: HangingType.Follow,
        CardTypeFilter: CardType.Attack,
        CardVisualEffect: new CardVisualEffect
        {
            HangGlowType = HangGlowType.Bad,
            TargetType = TargetType.AllEnemies
        }
    );

    /// <inheritdoc />
    public bool HangingSelfAfterPlay => true;

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var token = this.CreateHangingToken(async (context, play) =>
        {
            await DamageCmd.AutoAttack(this, play).Execute(context);
        });

        await HangingCardManager.HangCard(token, this);
    }
}