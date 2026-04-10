using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Superstitio.Main.Base;
using Superstitio.Main.DynamicVars;
using Superstitio.Main.Extensions;
using Superstitio.Main.Features.Corruptus;
using Superstitio.Main.Features.HangingCard;
using Superstitio.Main.Maso.Base;

namespace Superstitio.Main.Maso.Cards.CotiKoki;

/**
 * 消耗。移除 !B! 点 *腐朽 。 NL 造成 !D! 点 伤害 两次。 NL 把 CotiEye 加入手牌。 NL 获得 1 层 脆弱 。
        private const val COST = 0
        private const val DAMAGE = 2
        private const val UPGRADE_DAMAGE = 1
        private const val BLOCK = 4
        private const val UPGRADE_BLOCK = 1
 *
 */
public class CotiEar() : MasoBaseCard(new CardInitMessage
{
    BaseCost = 0,
    Type = CardType.Attack,
    Rarity = CardRarity.Token, // 衍生卡
    Target = TargetType.AnyEnemy,
})
{
    /// <inheritdoc />
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    /// <inheritdoc />
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
    [
        new DamageVar(2, ValueProp.Move).WithUpgrade(1),
        new BlockVar(4, ValueProp.Move).WithUpgrade(1), // 这里借用BlockVar来表示移除腐朽的数值
        new PowerVar<VulnerablePower>(1)
    ];

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 造成两次伤害
        await DamageCmd.AutoAttack(this, cardPlay).Execute(choiceContext);
        await DamageCmd.AutoAttack(this, cardPlay).Execute(choiceContext);

        // 移除腐朽 (假设腐朽是 CorruptusPower)
        await CorruptusManager.DecreaseCorruptus(this.Owner.Creature, this.DynamicVars.Block.BaseValue, this.Owner.Creature, this);

        // 自身获得脆弱
        await PowerCmd.ApplyByCard<VulnerablePower>(this, this.Owner.Creature);

        // 把 CotiEye 加入手牌
        var cotiEye = this.CombatState?.CreateCard<CotiEye>(this.Owner);
        if (cotiEye is not null)
            await CardPileCmd.AddGeneratedCardToCombat(cotiEye, PileType.Hand, addedByPlayer: true);
    }
}

/// <summary>
/// 消耗。对自身和敌人造成伤害两次，获得虚弱。挂起：打出3张牌后获得 CotiEar。
/// </summary>
public class CotiEye() : MasoBaseCard(new CardInitMessage
{
    BaseCost = 1,
    Type = CardType.Attack,
    Rarity = CardRarity.Uncommon,
    Target = TargetType.AnyEnemy,
}), IWithHangingConfigCard
{
    /// <inheritdoc />
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    /// <inheritdoc />
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
    [
        new DamageVar(4, ValueProp.Move).WithUpgrade(1),
        new TriggerCountVar(3),
        new PowerVar<WeakPower>(1)
    ];

    /// <inheritdoc />
    public HangingCardConfig HangingCardConfig => new(this, HangingType.Delay, CardType.None);

    /// <inheritdoc />
    public bool HangingSelfAfterPlay => true;

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 对敌人造成两次伤害
        await DamageCmd.AutoAttack(this, cardPlay, hitCount: 2).Execute(choiceContext);

        // 对自身造成伤害
        await DamageCmd.AutoAttack(this, this.Owner.Creature).Execute(choiceContext);

        // 获得虚弱
        await PowerCmd.ApplyByCard<WeakPower>(this, this.Owner.Creature);

        // 挂起：打出3次牌后加入 CotiEar
        var token = this.CreateHangingToken(async (_, _) =>
        {
            var cotiEar = this.CombatState?.CreateCard<CotiEar>(this.Owner);
            await CardPileCmd.AddGeneratedCardsToCombat(cotiEar is not null ? [cotiEar] : [], PileType.Hand, addedByPlayer: true);
        });
        await HangingCardManager.HangCard(token, this);
    }
}