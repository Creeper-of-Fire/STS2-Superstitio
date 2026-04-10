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
 * Title = "龙形拳"
 *
 * Description = """
 * 对敌人造成{Damage:diff()}点伤害，连续攻击{Repeat:diff()}次。对自身造成{Damage:diff()}点伤害。获得{WeakPower:diff()}层虚弱。
 * {CardHangingDescription}
 * """
 *
 * HangingEffect = "将[gold]蛇形拳[/gold]放入手中"
 *
 * Flavor = "龙影变幻莫测，连自己的心神也为之震颤。"
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
        new PowerVar<FrailPower>(1),
        new RepeatVar(2)
    ];

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 造成伤害
        await DamageCmd.AutoAttack(this, cardPlay, hitCount: this.DynamicVars.Repeat.IntValue).Execute(choiceContext);

        // 移除腐朽
        await CorruptusManager.DecreaseCorruptus(this.Owner.Creature, this.DynamicVars.Block.BaseValue, this.Owner.Creature, this);

        // 自身获得脆弱
        await PowerCmd.ApplyByCard<FrailPower>(this, this.Owner.Creature);

        // 把 CotiEye 加入手牌
        var cotiEye = this.CombatState?.CreateCard<CotiEye>(this.Owner);
        if (cotiEye is not null)
            await CardPileCmd.AddGeneratedCardToCombat(cotiEye, PileType.Hand, addedByPlayer: true);
    }
}

/**
 * Title = "蛇形拳"
 *
 * Description = """
 * 对敌人造成{Damage:diff()}点伤害，连续攻击{Repeat:diff()}次。对自身造成{Damage:diff()}点伤害。获得{WeakPower:diff()}层虚弱。
 * {CardHangingDescription}
 * """
 *
 * HangingEffect = "将[gold]龙形拳[/gold]放入手中"
 *
 * Flavor = "灵动如蛇，虽伤敌自损，却也让敌方无所适从。"
 */
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
        new PowerVar<WeakPower>(1),
        new RepeatVar(2)
    ];

    /// <inheritdoc />
    public HangingCardConfig HangingCardConfig => new(this, HangingType.Delay, CardType.None);

    /// <inheritdoc />
    public bool HangingSelfAfterPlay => true;

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 对敌人造成伤害
        await DamageCmd.AutoAttack(this, cardPlay, hitCount: this.DynamicVars.Repeat.IntValue).Execute(choiceContext);

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