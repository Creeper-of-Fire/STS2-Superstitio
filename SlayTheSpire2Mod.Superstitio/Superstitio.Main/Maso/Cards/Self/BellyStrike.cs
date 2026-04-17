using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Superstitio.Main.Base;
using Superstitio.Main.Extensions;
using Superstitio.Main.Maso.Base;

namespace Superstitio.Main.Maso.Cards.Self;

/**
 * Title = "腹部打击"
 *
 * Description = """
 * 对目标造成{CalculatedDamage:diff()}伤害，本回合给予{StrengthPower:diff()}力量。
 * 鞭策 (对自己/队友使用时触发的额外效果。)：只造成一半的伤害，翻倍其力量。
 * 目前，敌我双方任意角色可选的逻辑还没有做出来。所以这张卡会在之后才实现选择己方的功能
 * """
 *
 * Flavor = "对着这里来！"
 *
 * Sfw.Title = "力量模式"
 *
 * Sfw.Flavor = "我是劳伦斯巴恩斯，他们叫我“先知”。"
 */
public class BellyStrike() : MasoBaseCard(new CardInitMessage
{
    BaseCost = 2,
    Type = CardType.Attack,
    Rarity = CardRarity.Uncommon,
    Target = TargetType.None // TODO 目前，敌我双方任意角色可选的逻辑还没有做出来。所以这张卡会在之后才实现
})
{
    private const int Damage = 22;
    private const int DamageUpgrade = 8;
    private const int TemporaryStrength = 4;

    /// <inheritdoc />
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
    [
        new PowerVar<StrengthPower>(TemporaryStrength)
            .AddToolTips(),
        new CalculationBaseVar(Damage).WithUpgrade(DamageUpgrade),
        new ExtraDamageVar(0),
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier((_, target) =>
        {
            if (target is null || !target.IsPlayer) return 1m;
            return 0.5m;
        }),
    ];

    /// <inheritdoc />
    public class BellyStrikePower : TemporaryPower<StrengthPower>
    {
        /// <inheritdoc />
        public override AbstractModel OriginModel => ModelDb.Card<SetupStrike>();
    }

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.AutoAttack(this, cardPlay.Target).Execute(choiceContext);
        if (cardPlay.Target is null)
            return;
        await PowerCmd.ApplyByCard<BellyStrikePower>(this, cardPlay.Target);

        if (cardPlay.Target.IsPlayer)
        {
            int num = cardPlay.Target.IsAlive ? cardPlay.Target.GetPowerAmount<StrengthPower>() : 0;
            if (num > 0)
            {
                await PowerCmd.Apply<StrengthPower>(cardPlay.Target, num, this.Owner.Creature, this);
            }
        }
    }
}