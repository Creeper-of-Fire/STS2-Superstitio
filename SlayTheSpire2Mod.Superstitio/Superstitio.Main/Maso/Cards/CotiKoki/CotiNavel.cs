using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Superstitio.Main.Base;
using Superstitio.Main.Extensions;
using Superstitio.Main.Maso.Base;

namespace Superstitio.Main.Maso.Cards.CotiKoki;

/**
 * Title = "性交-插入：肚脐"
 *
 * Description = "对敌人造成{Damage:diff()}点伤害，对自身造成{Damage:diff()}点伤害。获得{VulnerablePower:diff()}层易伤。"
 *
 * Flavor = "洞必试，试必捅。射进腹腔里会宫外孕吗？"
 *
 * Sfw.Title = "熊形拳"
 *
 * Sfw.Flavor = "笨重但致命的一击，如狂熊拍击般不顾一切。"
 */
public class CotiNavel() : MasoBaseCard(new CardInitMessage
{
    BaseCost = 2,
    Type = CardType.Attack,
    Rarity = CardRarity.Uncommon,
    Target = TargetType.AllEnemies,
})
{
    /// <inheritdoc />
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain];

    /// <inheritdoc />
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
    [
        new PowerVar<VulnerablePower>(1).AddToolTips(),
        new DamageVar(22,ValueProp.Move).WithUpgrade(7),
    ];

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 对目标造成伤害
        await DamageCmd.AutoAttack(this, cardPlay).Execute(choiceContext);
        // 对自身造成伤害
        await DamageCmd.AutoAttack(this, this.Owner.Creature).Execute(choiceContext);

        // 自身获得易伤
        await PowerCmd.ApplyByCard<VulnerablePower>(this, this.Owner.Creature);
    }
}