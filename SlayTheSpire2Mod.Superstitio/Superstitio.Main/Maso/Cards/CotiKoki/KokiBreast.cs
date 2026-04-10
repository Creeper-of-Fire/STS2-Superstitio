using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Superstitio.Main.Base;
using Superstitio.Main.DynamicVars.Extensions;
using Superstitio.Main.Extensions;
using Superstitio.Main.Features.Milk;
using Superstitio.Main.Maso.Base;

namespace Superstitio.Main.Maso.Cards.CotiKoki;

/// <summary>
/// 造成伤害。给予仁慈。
/// </summary>
public class KokiBreast() : MasoBaseCard(new CardInitMessage
{
    BaseCost = 2,
    Type = CardType.Attack,
    Rarity = CardRarity.Common,
    Target = TargetType.Self,
})
{
    private const int Damage = 15;

    private const int DamageUpgrade = 6;

    private const int Milk = 5;

    private const int MilkUpgrade = 3;

    /// <inheritdoc />
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
    [
        new DamageVar(Damage, ValueProp.Move).WithUpgrade(DamageUpgrade),
        new PowerVar<MilkPower>(Milk).WithUpgrade(MilkUpgrade)
    ];

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.AutoAttack(this, cardPlay).Execute(choiceContext);
        await PowerCmd.ApplyByCard<MilkPower>(this, cardPlay.GetTargetOrThrow());
    }
}