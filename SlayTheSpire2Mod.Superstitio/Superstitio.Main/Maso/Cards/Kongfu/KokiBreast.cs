using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Superstitio.Main.Base;
using Superstitio.Main.DynamicVars.Extensions;
using Superstitio.Main.Extensions;
using Superstitio.Main.Maso.Base;

namespace Superstitio.Main.Maso.Cards.Kongfu;

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
        new DynamicVar(nameof(Milk), Milk).WithUpgrade(MilkUpgrade)
    ];

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.AutoAttack(this, cardPlay).Execute(choiceContext);
        await PowerCmd.ApplyByCard<MilkPower>(this, cardPlay.GetTargetOrThrow(), this.DynamicVars.GetVarOrThrow(nameof(Milk)).BaseValue);
    }
}

/// <summary>
/// 在下一次攻击后，给予玩家（施加者） %d 点 #y临时生命 。
/// 或者也许召唤一个召唤物？
/// TODO 临时生命还没有做好，目前没有效果。
/// </summary>
public class MilkPower : CustomPowerModel
{
    /// <inheritdoc />
    public override PowerType Type => PowerType.Buff;

    /// <inheritdoc />
    public override PowerStackType StackType => PowerStackType.Counter;

    /// <inheritdoc />
    public override bool IsInstanced => false;
}