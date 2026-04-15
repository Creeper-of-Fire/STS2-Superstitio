using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Superstitio.Main.Base;
using Superstitio.Main.DynamicVars;
using Superstitio.Main.Extensions;
using Superstitio.Main.Features.PowerCardPower;
using Superstitio.Main.Maso.Base;

namespace Superstitio.Main.Maso.Cards.Self;

/**
 * Title = "饮鸩止渴"
 *
 * Description = """
 * 移除一半的[sine][red]腐朽[/red][/sine]。
 * 获得移除量一半的[gold]中毒[/gold]。
 * （预计移除：{DecayRemovalMultiplier:diff()}）
 * """
 *
 * Flavor = "甜丝丝的，谁能想到呢？我可不是觉得有趣才喝的哦。"
 *
 * Sfw.Title = "饮鸩止渴"
 *
 * Sfw.Flavor = "未入肠胃，已绝咽喉，岂可为哉！"
 */
public class DrinkPoison() : MasoBaseCard(new CardInitMessage
{
    BaseCost = 1,
    Type = CardType.Skill,
    Rarity = CardRarity.Uncommon,
    Target = TargetType.Self
})
{
    private const int DecayRemovalMultiplier = 2;
    private const int DecayRemovalMultiplierUpgrade = 0;

    /// <inheritdoc />
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Retain,
        CardKeyword.Exhaust,
    ];

    /// <inheritdoc />
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
    [
        new DynamicVar("DecayRemovalMultiplier", DecayRemovalMultiplier).WithUpgrade(DecayRemovalMultiplierUpgrade),
    ];

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 1. 移除一半的腐朽
        // 2. 获得移除量一半的中毒
        // 3. 显示预计移除量
    }

    /// <inheritdoc />
    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        this.RemoveKeyword(CardKeyword.Exhaust);
    }
}

/**
 * Title = "割腕"
 *
 * Description = """
 * 失去{HpLoss:diff()}点生命。
 * 移除{DecayRemoval:diff()}点[sine][red]腐朽[/red][/sine]。
 * """
 *
 * Flavor = "充满了魔性的魅力。"
 *
 * Sfw.Title = "放血疗法"
 *
 * Sfw.Flavor = "你不是失血过多。相反，你血液过剩了。来，让我划上一刀。"
 */
public class CutWrist() : MasoBaseCard(new CardInitMessage
{
    BaseCost = 1,
    Type = CardType.Skill,
    Rarity = CardRarity.Common,
    Target = TargetType.Self
})
{
    private const int HpLoss = 8;
    private const int HpLossUpgrade = 0;
    private const int DecayRemoval = 18;
    private const int DecayRemovalUpgrade = 4;

    /// <inheritdoc />
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
    [
        new HpLossVar(HpLoss).WithUpgrade(HpLossUpgrade),
        new DynamicVar("DecayRemoval", DecayRemoval).WithUpgrade(DecayRemovalUpgrade),
    ];

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 1. 失去 !M! 生命
        // 2. 移除 !B! 点 腐朽
    }
}

/**
 * Title = "拔指甲"
 *
 * Description = """
 * 失去{HpLoss:diff()}点生命，抽{CardsDrawn:diff()}张牌。
 * 往抽牌堆中添加{CardsAdded:diff()}张[gold]拔指甲[/gold]。
 * 本回合只能打出{MaxPlayable:diff()}张。
 * （已使用 !M! 张）
 * """
 *
 * UpgradeDescription = """
 * 对自身造成{HpLoss:diff()}点伤害。
 * 抽{CardsDrawn:diff()}张牌。
 * 往抽牌堆中添加{CardsAdded:diff()}张[gold]拔指甲[/gold]。
 * 本回合只能打出{MaxPlayable:diff()}张。
 * （已使用 !M! 张）
 * """
 *
 * Flavor = "拔下来马上又会马上长回去，想往里面插根针都来不及。"
 *
 * Sfw.Title = "以牙还牙"
 *
 * Sfw.Flavor = "以眼还眼？"
 */
public class NailExtraction() : MasoBaseCard(new CardInitMessage
{
    BaseCost = 0,
    Type = CardType.Skill,
    Rarity = CardRarity.Uncommon,
    Target = TargetType.Self
})
{
    private const int HpLoss = 3;
    private const int HpLossUpgrade = 0;
    private const int CardsDrawn = 2;
    private const int CardsDrawnUpgrade = 0;
    private const int CardsAdded = 2;
    private const int CardsAddedUpgrade = 0;
    private const int MaxPlayable = 20;
    private const int MaxPlayableUpgrade = 0;

    /// <inheritdoc />
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
    [
        new DamageSelfVar(HpLoss, ValueProp.Move).WithUpgrade(HpLossUpgrade),
        new DrawCardsVar(CardsDrawn).WithUpgrade(CardsDrawnUpgrade),
        new DynamicVar("CardsAdded", CardsAdded).WithUpgrade(CardsAddedUpgrade),
        new DynamicVar("MaxPlayable", MaxPlayable).WithUpgrade(MaxPlayableUpgrade),
    ];

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 1. 失去 %d 生命
        // 2. 抽 %d 张牌
        // 3. 往抽牌堆中添加 %d 张 *以牙还牙
        // 4. 本回合只能打出 %d 张
        // 5. 跟踪已使用张数
    }
}

/**
 * Title = "慕残之心"
 *
 * Description = """
 * 抽到[gold]无法打出[/gold]的牌时，将其消耗。
 * 在手牌中添加一张[gold]感受幻肢[/gold]。
 * """
 *
 * Flavor = "真希望我一开始就没有这些部位，那样一定很无助~❤"
 *
 * Sfw.Title = "加装义体"
 *
 * Sfw.Flavor = ""
 */
public class OrgasmWithLostBody() : MasoBaseCard(new CardInitMessage
{
    BaseCost = 1,
    Type = CardType.Power,
    Rarity = CardRarity.Rare,
    Target = TargetType.Self
})
{
    /// <inheritdoc />
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
    [
        new PowerVar<OrgasmWithLostBodyPower>(1).AddToolTips(),
    ];

    /// <inheritdoc cref="OrgasmWithLostBody"/>
    public class OrgasmWithLostBodyPower() : SimpleCardPower<OrgasmWithLostBody>(new PowerInitMessage
    {
        Type = PowerType.Buff,
        InitStackType = PowerInitMessage.StackStyle.Normal
    })
    {
        // TODO: Implement logic
        // 1. 当抽到无法打出的牌时，将其消耗
        // 2. 在手牌中添加一张 *义体
    }

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.ApplyByCard<OrgasmWithLostBodyPower>(this, this.Owner.Creature);
    }
}

/**
 * Title = "身体欠损"
 *
 * Description = """
 * 若还有可选择的部位：移除{DecayRemoval:diff()}点[sine][red]腐朽[/red][/sine]。
 * 攻击力提升{AttackIncreasePercent:diff()}%。
 * 选择一个部位，其对应的[gold]性交牌[/gold]无法打出。
 * """
 *
 * Flavor = "用刀砍，还是用手拽下来呢？这是个问题。"
 *
 * Sfw.Title = "义体升级"
 *
 * Sfw.Flavor = ""
 */
public class BodyModification_CutOff() : MasoBaseCard(new CardInitMessage
{
    BaseCost = 1,
    Type = CardType.Power,
    Rarity = CardRarity.Uncommon,
    Target = TargetType.Self
})
{
    private const int DecayRemoval = 14;
    private const int DecayRemovalUpgrade = 4;
    private const int AttackIncreasePercent = 15;
    private const int AttackIncreasePercentUpgrade = 20;

    /// <inheritdoc />
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
    [
        new DynamicVar("DecayRemoval", DecayRemoval).WithUpgrade(DecayRemovalUpgrade),
        new DynamicVar("AttackIncreasePercent", AttackIncreasePercent).WithUpgrade(AttackIncreasePercentUpgrade),
    ];

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 1. 若还有可选择的部位：移除 !B! 腐朽
        // 2. 攻击力提升 !M! %
        // 3. 选择一个部位，其对应的武学牌无法打出
    }
}

/**
 * Title = "欠损部位选择"
 *
 * Description = """
 * 这张牌用于选择，它的名称和描述都会在选择界面改变。
 * """
 *
 * Flavor = "好难选啊，能手快全选吗？"
 *
 * Sfw.Title = "部位选择"
 *
 * Sfw.Flavor = "好难选啊，能手快全选吗？"
 */
public class BodyModification_CutOff_Chose() : MasoBaseCard(new CardInitMessage
{
    BaseCost = 0,
    Type = CardType.Skill,
    Rarity = CardRarity.Token,
    Target = TargetType.Self
})
{
    /// <inheritdoc />
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
    [
        // No vars needed for this card
    ];

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 1. 显示部位选择界面
        // 2. 根据选择更新卡牌效果
    }
}

/**
 * Title = "超巨乳化"
 *
 * Description = """
 * 受到攻击伤害而失去生命时，获得{MercyGain:diff()}点[gold]仁慈[/gold]。
 * """
 *
 * Flavor = "大大大大大！"
 *
 * Sfw.Title = "仁慈之心"
 *
 * Sfw.Flavor = "你得知道LV和EXP到底是什么的缩写。"
 */
public class BodyModification_SuperHugeBreast() : MasoBaseCard(new CardInitMessage
{
    BaseCost = 2,
    Type = CardType.Power,
    Rarity = CardRarity.Uncommon,
    Target = TargetType.Self
})
{
    private const int MercyGain = 4;
    private const int MercyGainUpgrade = 2;

    /// <inheritdoc />
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
    [
        new PowerVar<BodyModification_SuperHugeBreastPower>(MercyGain).WithUpgrade(MercyGainUpgrade)
            .AddToolTips<BodyModification_SuperHugeBreastPower>(),
    ];

    /// <inheritdoc cref="BodyModification_SuperHugeBreast"/>
    public class BodyModification_SuperHugeBreastPower() : SimpleCardPower<BodyModification_SuperHugeBreast>(new PowerInitMessage
    {
        Type = PowerType.Buff,
        InitStackType = PowerInitMessage.StackStyle.Normal
    })
    {
        // TODO: Implement logic
        // 1. 受到攻击伤害而失去生命时，获得 !M! 仁慈
    }

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.ApplyByCard<BodyModification_SuperHugeBreastPower>(this, this.Owner.Creature);
    }
}

/**
 * Title = "纹身穿刺"
 *
 * Description = """
 * 每当有一张牌被[gold]消耗[/gold]或被[gold]丢弃[/gold]时，获得{FelixGain:diff()}点[gold]快感[/gold]。
 * """
 *
 * Flavor = ""
 *
 * Sfw.Title = "强者战纹"
 *
 * Sfw.Flavor = "好刺激，好刺激，难得能亲眼看到强者之战，这回就算死也值回票价呀！"
 */
public class BodyModification_TattooAndPiercing() : MasoBaseCard(new CardInitMessage
{
    BaseCost = 1,
    Type = CardType.Power,
    Rarity = CardRarity.Uncommon,
    Target = TargetType.Self
})
{
    private const int FelixGain = 2;
    private const int FelixGainUpgrade = 1;

    /// <inheritdoc />
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
    [
        new PowerVar<BodyModification_TattooAndPiercingPower>(FelixGain).WithUpgrade(FelixGainUpgrade)
            .AddToolTips<BodyModification_TattooAndPiercingPower>(),
    ];

    /// <inheritdoc cref="BodyModification_TattooAndPiercing"/>
    public class BodyModification_TattooAndPiercingPower() : SimpleCardPower<BodyModification_TattooAndPiercing>(new PowerInitMessage
    {
        Type = PowerType.Buff,
        InitStackType = PowerInitMessage.StackStyle.Normal
    })
    {
        // TODO: Implement logic
        // 1. 每当有一张牌被消耗或被丢弃时，获得 !M! 快感
        // 2. 显示当前战纹及其效果
    }

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.ApplyByCard<BodyModification_TattooAndPiercingPower>(this, this.Owner.Creature);
    }
}

/**
 * Title = "脱垂"
 *
 * Description = """
 * 受到攻击伤害时，[gold]丢弃[/gold]一张随机手牌，此次伤害减为{DamageReductionDivisor:diff()}分之一。
 * 若没有手牌，则尝试[gold]丢弃[/gold]一张[gold]助兴序列[/gold]中的随机牌。
 * """
 *
 * Flavor = ""
 *
 * Sfw.Title = "反应装甲"
 *
 * Sfw.Flavor = "什么都贴爆反只会害了你。"
 */
public class BodyModification_Prolapse() : MasoBaseCard(new CardInitMessage
{
    BaseCost = 1,
    Type = CardType.Power,
    Rarity = CardRarity.Uncommon,
    Target = TargetType.Self
})
{
    private const int DamageReductionDivisor = 2;
    private const int DamageReductionDivisorUpgrade = 1;

    /// <inheritdoc />
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
    [
        new PowerVar<BodyModification_ProlapsePower>(DamageReductionDivisor).WithUpgrade(DamageReductionDivisorUpgrade)
            .AddToolTips<BodyModification_ProlapsePower>(),
    ];

    /// <inheritdoc cref="BodyModification_Prolapse"/>
    public class BodyModification_ProlapsePower() : SimpleCardPower<BodyModification_Prolapse>(new PowerInitMessage
    {
        Type = PowerType.Buff,
        InitStackType = PowerInitMessage.StackStyle.Normal
    })
    {
        // TODO: Implement logic
        // 1. 受到攻击伤害时，丢弃一张随机手牌
        // 2. 此次伤害减为 {Amount} 分之一
        // 3. 若没有手牌，则尝试丢弃一张合力序列中的随机牌
        // 4. 显示当前反应装甲及其效果
    }

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.ApplyByCard<BodyModification_ProlapsePower>(this, this.Owner.Creature);
    }
}

/**
 * Title = "凌迟"
 *
 * Description = """
 * [gold]并行[/gold]。
 * [gold]助兴序列[/gold]中每有一张卡牌，失去{HpLossPerCard:diff()}点生命，移除{DecayRemovalPerCard:diff()}点[sine][red]腐朽[/red][/sine]。
 * """
 *
 * Flavor = ""
 *
 * Sfw.Title = "鲜血护体"
 *
 * Sfw.Flavor = ""
 */
public class CruelTorture_Dismember() : MasoBaseCard(new CardInitMessage
{
    BaseCost = 1,
    Type = CardType.Skill,
    Rarity = CardRarity.Common,
    Target = TargetType.Self
})
{
    private const int HpLossPerCard = 1;
    private const int HpLossPerCardUpgrade = 0;
    private const int DecayRemovalPerCard = 3;
    private const int DecayRemovalPerCardUpgrade = 1;

    /// <inheritdoc />
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
    [
        new DynamicVar("HpLossPerCard", HpLossPerCard).WithUpgrade(HpLossPerCardUpgrade),
        new DynamicVar("DecayRemovalPerCard", DecayRemovalPerCard).WithUpgrade(DecayRemovalPerCardUpgrade),
    ];

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 1. 极速
        // 2. 合力序列中每有一张卡牌，失去 !M! 生命
        // 3. 合力序列中每有一张卡牌，移除 !B! 腐朽
    }
}

/**
 * Title = "炮烙"
 *
 * Description = """
 * 将{BurnCardsAdded:diff()}张[gold]灼伤[/gold]放入你的弃牌堆中。
 * 获得{TemporaryDexterity:diff()}点[gold]临时敏捷[/gold]。
 * [gold]缠绵[/gold]回合开始：
 * 获得{TemporaryDexterity:diff()}点[gold]临时敏捷[/gold]。
 * """
 *
 * Flavor = ""
 *
 * Sfw.Title = "超载"
 *
 * Sfw.Flavor = ""
 */
public class CruelTorture_HotPillar() : MasoBaseCard(new CardInitMessage
{
    BaseCost = 1,
    Type = CardType.Skill,
    Rarity = CardRarity.Uncommon,
    Target = TargetType.Self
})
{
    private const int BurnCardsAdded = 1;
    private const int BurnCardsAddedUpgrade = 0;
    private const int TemporaryDexterity = 3;
    private const int TemporaryDexterityUpgrade = 1;

    /// <inheritdoc />
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
    [
        new DynamicVar("BurnCardsAdded", BurnCardsAdded).WithUpgrade(BurnCardsAddedUpgrade),
        new DynamicVar("TemporaryDexterity", TemporaryDexterity).WithUpgrade(TemporaryDexterityUpgrade),
    ];

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 1. 将 %d 张 *灼伤 放入你的弃牌堆中
        // 2. 获得 !M! 点 临时敏捷
        // 3. 伴随 回合开始 ：获得 !M! 点 临时敏捷
    }
}

/**
 * Title = "尖桩贯穿"
 *
 * Description = """
 * 每有{DecayThreshold:diff()}点[sine][red]腐朽[/red][/sine]，抽一张牌。
 * （预计抽牌：%d）
 * 失去{HpLoss:diff()}点生命，抽一张牌。
 * """
 *
 * Flavor = ""
 *
 * Sfw.Title = "祭祀"
 *
 * Sfw.Flavor = "以撒和他的父母住在山上的一所小房子里。"
 */
public class CruelTorture_Impale() : MasoBaseCard(new CardInitMessage
{
    BaseCost = 1,
    Type = CardType.Skill,
    Rarity = CardRarity.Uncommon,
    Target = TargetType.Self
})
{
    private const int DecayThreshold = 5;
    private const int DecayThresholdUpgrade = -2;
    private const int HpLoss = 5;
    private const int HpLossUpgrade = -2;

    /// <inheritdoc />
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
    [
        new DynamicVar("DecayThreshold", DecayThreshold).WithUpgrade(DecayThresholdUpgrade),
        new DamageSelfVar(HpLoss, ValueProp.Move).WithUpgrade(HpLossUpgrade),
    ];

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 1. 每有 !M! 点 腐朽，抽一张牌
        // 2. 显示预计抽牌数
        // 3. 失去 !M! 生命，抽一张牌
    }
}

/**
 * Title = "恋污"
 *
 * Description = """
 * 如果这张牌从你的手牌中被[gold]丢弃[/gold]或[gold]消耗[/gold]，获得{BloodlustGain:diff()}点[gold]回味情爱[/gold]。
 * """
 *
 * Flavor = ""
 *
 * Sfw.Title = "光辉飞盾"
 *
 * Sfw.Flavor = "你好，程序！"
 */
public class LoveDirty() : MasoBaseCard(new CardInitMessage
{
    BaseCost = 1,
    Type = CardType.Skill,
    Rarity = CardRarity.Common,
    Target = TargetType.Self
})
{
    private const int BloodlustGain = 12;
    private const int BloodlustGainUpgrade = 4;

    /// <inheritdoc />
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
    [
        new DynamicVar("BloodlustGain", BloodlustGain).WithUpgrade(BloodlustGainUpgrade),
    ];

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 1. 如果这张牌从你的手牌中被丢弃或消耗，获得 !B! 回味热血
    }
}

/**
 * Title = "沉重镣铐"
 *
 * Description = """
 * 获得{ShackleBlock:diff()}点[gold]镣铐格挡[/gold]。
 * 若从本卡获得的格挡失去，则抽{CardsDrawn:diff()}张牌。
 * """
 *
 * Flavor = ""
 *
 * Sfw.Title = "沉重镣铐"
 *
 * Sfw.Flavor = ""
 */
public class HeavyCuffs() : MasoBaseCard(new CardInitMessage
{
    BaseCost = 1,
    Type = CardType.Skill,
    Rarity = CardRarity.Common,
    Target = TargetType.Self
})
{
    private const int ShackleBlock = 8;
    private const int ShackleBlockUpgrade = 3;
    private const int CardsDrawn = 2;
    private const int CardsDrawnUpgrade = 1;

    /// <inheritdoc />
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
    [
        new DynamicVar("ShackleBlock", ShackleBlock).WithUpgrade(ShackleBlockUpgrade),
        new DrawCardsVar(CardsDrawn).WithUpgrade(CardsDrawnUpgrade),
    ];

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 1. 获得 !B! 镣铐格挡
        // 2. 若从本卡获得的格挡失去，则抽 !M! 张牌
    }
}

/**
 * Title = "感度3000"
 *
 * Description = """
 * [gold]高潮[/gold]时，获得{EnergyGain:diff()}点[E]。
 * """
 *
 * Flavor = ""
 *
 * Sfw.Title = "炽热之魂"
 *
 * Sfw.Flavor = ""
 */
public class Sensitive3000() : MasoBaseCard(new CardInitMessage
{
    BaseCost = 2,
    Type = CardType.Power,
    Rarity = CardRarity.Rare,
    Target = TargetType.Self
})
{
    private const int EnergyGain = 1;
    private const int EnergyGainUpgrade = 0;

    /// <inheritdoc />
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
    [
        new PowerVar<Sensitive3000Power>(EnergyGain).WithUpgrade(EnergyGainUpgrade)
            .AddToolTips<Sensitive3000Power>(),
    ];

    /// <inheritdoc cref="Sensitive3000"/>
    public class Sensitive3000Power() : SimpleCardPower<Sensitive3000>(new PowerInitMessage
    {
        Type = PowerType.Buff,
        InitStackType = PowerInitMessage.StackStyle.Normal
    })
    {
        // TODO: Implement logic
        // 1. 狂暴时，获得 !M! 的 [E]
    }

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.ApplyByCard<Sensitive3000Power>(this, this.Owner.Creature);
    }
}

/**
 * Title = "施虐形态"
 *
 * Description = """
 * 每当造成攻击伤害或失去生命后，下回合获得{StrengthGain:diff()}点[gold]力量[/gold]。
 * """
 *
 * Flavor = ""
 *
 * Sfw.Title = "复仇形态"
 *
 * Sfw.Flavor = ""
 */
public class SadismForm() : MasoBaseCard(new CardInitMessage
{
    BaseCost = 3,
    Type = CardType.Power,
    Rarity = CardRarity.Rare,
    Target = TargetType.Self
})
{
    private const int StrengthGain = 2;
    private const int StrengthGainUpgrade = 1;

    /// <inheritdoc />
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
    [
        new PowerVar<SadismFormPower>(StrengthGain).WithUpgrade(StrengthGainUpgrade)
            .AddToolTips<SadismFormPower>(),
    ];

    /// <inheritdoc cref="SadismForm"/>
    public class SadismFormPower() : SimpleCardPower<SadismForm>(new PowerInitMessage
    {
        Type = PowerType.Buff,
        InitStackType = PowerInitMessage.StackStyle.Normal
    })
    {
        // TODO: Implement logic
        // 1. 每当造成攻击伤害或失去生命后，下回合获得 !M! 力量
    }

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.ApplyByCard<SadismFormPower>(this, this.Owner.Creature);
    }
}

/**
 * Title = "性病轮盘赌"
 *
 * Description = """
 * 获得{BloodlustGain:diff()}点[gold]回味情爱[/gold]。
 * 本回合，每当你失去生命时，给予全体敌人{FearGain:diff()}层[gold]爱欲[/gold]。
 * """
 *
 * Flavor = ""
 *
 * Sfw.Title = "轮盘赌"
 *
 * Sfw.Flavor = ""
 */
public class StdRoulette() : MasoBaseCard(new CardInitMessage
{
    BaseCost = 2,
    Type = CardType.Skill,
    Rarity = CardRarity.Uncommon,
    Target = TargetType.Self
})
{
    private const int BloodlustGain = 16;
    private const int BloodlustGainUpgrade = 0;
    private const int FearGain = 2;
    private const int FearGainUpgrade = 2;

    /// <inheritdoc />
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
    [
        new DynamicVar("BloodlustGain", BloodlustGain).WithUpgrade(BloodlustGainUpgrade),
        new DynamicVar("FearGain", FearGain).WithUpgrade(FearGainUpgrade),
    ];

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 1. 获得 !B! 回味热血
        // 2. 本回合，每当你失去生命时，给予全体敌人 !M! 层 恐惧
    }
}

/**
 * Title = "人体蜈蚣"
 *
 * Description = """
 * 给予目标{ChainStacks:diff()}层[gold]人体蜈蚣[/gold]。
 * """
 *
 * Flavor = ""
 *
 * Sfw.Title = "铁索连环"
 *
 * Sfw.Flavor = ""
 */
public class HumanCentipede() : MasoBaseCard(new CardInitMessage
{
    BaseCost = 1,
    Type = CardType.Skill,
    Rarity = CardRarity.Uncommon,
    Target = TargetType.AnyPlayer // TODO 实际上应该是SelfOrEnemy，但是目前没有办法实现
})
{
    private const int ChainStacks = 8;
    private const int ChainStacksUpgrade = 3;
    private const int InternalDamage = 1;
    private const int InternalDamageUpgrade = 1;

    /// <inheritdoc />
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
    [
        new DynamicVar("ChainStacks", ChainStacks).WithUpgrade(ChainStacksUpgrade),
        new DynamicVar("InternalDamage", InternalDamage).WithUpgrade(InternalDamageUpgrade),
    ];

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 给予目标 !M! 层 人体蜈蚣 : 受到攻击伤害时，对敌我双方造成 %d 点 内劲伤害
    }
}

/**
 * Title = "断头台"
 *
 * Description = """
 * 本回合，[gold]高潮[/gold]所需的[gold]快感[/gold]提高{FelixRequirementIncrease:diff()}。
 * 攻击造成的伤害提高{DamageIncreasePercent:diff()}%。
 * [gold]欲求[/gold][gold]高潮[/gold]{HangingTime:diff()}次：
 * [gold]立即死亡？[/gold]
 * [gold]自动释放[/gold]。
 * """
 *
 * Flavor = ""
 *
 * Sfw.Title = "命悬一线"
 *
 * Sfw.Flavor = ""
 */
public class Guillotine() : MasoBaseCard(new CardInitMessage
{
    BaseCost = 0,
    Type = CardType.Skill,
    Rarity = CardRarity.Rare,
    Target = TargetType.Self
})
{
    private const int FelixRequirementIncrease = 10;
    private const int FelixRequirementIncreaseUpgrade = 0;
    private const int DamageIncreasePercent = 50;
    private const int DamageIncreasePercentUpgrade = 0;
    private const int HangingTime = 2;
    private const int HangingTimeUpgrade = 1;

    /// <inheritdoc />
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
    [
        new DynamicVar("FelixRequirementIncrease", FelixRequirementIncrease).WithUpgrade(FelixRequirementIncreaseUpgrade),
        new DynamicVar("DamageIncreasePercent", DamageIncreasePercent).WithUpgrade(DamageIncreasePercentUpgrade),
        new DynamicVar("HangingTime", HangingTime).WithUpgrade(HangingTimeUpgrade),
    ];

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 1. 本回合，狂暴所需的快感提高 %d
        // 2. 攻击造成的伤害提高 %d %%
        // 3. 延缓 狂暴 !M! 次：立即死亡？
        // 4. 自动释放
    }
}