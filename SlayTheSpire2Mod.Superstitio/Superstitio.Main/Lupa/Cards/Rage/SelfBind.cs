using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using Superstitio.Main.Base;
using Superstitio.Main.Extensions;
using Superstitio.Main.Lupa.Base;

namespace Superstitio.Main.Lupa.Cards.Rage;

/**
 * Title = "结下束缚"
 *
 * Description = """
 * 获得{Power:diff()}层力量。
 * 失去1层敏捷。
 * """
 *
 * Flavor = "束缚自己，获得更强大的力量。"
 */
public class SelfBind() : LupaBaseCard(new CardInitMessage
{
    BaseCost = 1,
    Type = CardType.Power,
    Rarity = CardRarity.Common,
    Target = TargetType.Self
})
{
    private const int Strength = 3;
    private const int StrengthUpgrade = 1;
    private const int Dexterity = -1;

    /// <inheritdoc />
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
    [
        new PowerVar<StrengthPower>(Strength).WithUpgrade(StrengthUpgrade),
        new PowerVar<DexterityPower>(Dexterity),
    ];

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.ApplyByCard<StrengthPower>(this, this.Owner.Creature);
        await PowerCmd.ApplyByCard<DexterityPower>(this, this.Owner.Creature);
    }
}