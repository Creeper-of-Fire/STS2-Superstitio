using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Superstitio.Api.BaseLib.HangingCard;
using Superstitio.Api.Card;
using Superstitio.Api.DynamicVars;
using Superstitio.Api.Extensions;
using Superstitio.Api.HangingCard;
using Superstitio.Api.HangingCard.UI;
using Superstitio.Main.Features.Corruptus;
using Superstitio.Main.Maso.Base;
using SuperstitioBaseCard = Superstitio.Main.Base.SuperstitioBaseCard;

namespace Superstitio.Main.Maso.Cards.CotiKoki;

/**
 * Title = "性交-插入：耳奸"
 *
 * Description = """
 * 对敌人造成{Damage:diff()}点伤害{Repeat:diff()}次。
 * 移除{Block:diff()}点[sine][red]腐朽[/red][/sine]。
 * 获得{FrailPower:diff()}层[gold]脆弱[/gold]。
 * 将[gold]{CotiEye:diff()}[/gold]放入手中。
 * """
 *
 * Flavor = "是脑交！要是脑子有触觉就好了，哧溜。"
 *
 * Sfw.Title = "龙形拳"
 *
 * Sfw.Flavor = "龙影变幻莫测，连自己的心神也为之震颤。"
 */
[Pool(typeof(TokenCardPool))]
public class CotiEar() : SuperstitioBaseCard(new CardInitMessage
{
    BaseCost = 0,
    Type = CardType.Attack,
    Rarity = CardRarity.Token, // 衍生卡
    Target = TargetType.AnyEnemy,
})
{
    /// <inheritdoc />
    public override IEnumerable<CardKeywordSpec> InitCardKeywords =>
    [
        CardKeyword.Exhaust
    ];

    /// <inheritdoc />
    public override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
    [
        new DamageVar(2, ValueProp.Move).WithUpgrade(1),
        new BlockVar(4, ValueProp.Move).WithUpgrade(1), // 这里借用BlockVar来表示移除腐朽的数值
        new PowerVar<FrailPower>(1).AddToolTips(),
        new RepeatVar(2),
        new CardTitleVar<CotiEye>().AddToolTips().WithUpgrade(1),
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
 * Title = "性交-插入：眼交"
 *
 * Description = """
 * 对[gold]自身和敌人[/gold]各造成{Damage:diff()}点伤害{Repeat:diff()}次。
 * 获得{WeakPower:diff()}层[gold]虚弱[/gold]。
 * {CardHangingDescription}
 * """
 *
 * HangingEffect = "将[gold]{CotiEar:diff()}[/gold]放入手中"
 *
 * Flavor = "也许是脑交。总的来说还是取决于肉棒的主人敢不敢用力吧。"
 *
 * Sfw.Title = "蛇形拳"
 *
 * Sfw.Flavor = "灵动如蛇，虽伤敌自损，却也让敌方无所适从。"
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
    public override IEnumerable<CardKeywordSpec> InitCardKeywords =>
    [
        CardKeyword.Exhaust
    ];

    /// <inheritdoc />
    public override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
    [
        new DamageVar(4, ValueProp.Move).WithUpgrade(1),
        new TriggerCountVar(3),
        new RepeatVar(2),
        new PowerVar<WeakPower>(1).AddToolTips(),
        new CardTitleVar<CotiEar>().AddToolTips().WithUpgrade(1),
    ];

    /// <inheritdoc />
    public HangingCardConfig HangingCardConfig => new(
        Card: this,
        HangingType: HangingType.Delay,
        CardTypeFilter: CardType.None,
        CardVisualEffect: new CardVisualEffect
        {
            HangGlowType = HangGlowType.Good,
            TargetType = TargetType.Self
        }
    );

    /// <inheritdoc />
    public bool HangingSelfAfterPlay => true;

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 对敌人造成伤害
        await DamageCmd.AutoAttack(this, cardPlay, hitCount: this.DynamicVars.Repeat.IntValue).Execute(choiceContext);

        // 对自身造成伤害
        await DamageCmd.AutoAttack(this, this.Owner.Creature, hitCount: this.DynamicVars.Repeat.IntValue).Execute(choiceContext);

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