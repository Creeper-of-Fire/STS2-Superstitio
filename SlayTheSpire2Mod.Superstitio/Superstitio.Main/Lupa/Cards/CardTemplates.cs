using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using Superstitio.Main.Base;
using Superstitio.Main.Lupa.Base;
using Superstitio.Main.Maso.Base;

namespace superstitio.main.lupa.cards;

/**
 * Title = ""
 *
 * Description = """
 * 把1张带有**消耗**的随机**性交牌**加入手牌。这张牌在本回合耗能变为0。
 * """
 *
 * Flavor = ""
 */
public class ChooseCoitalPosture() : LupaBaseCard(new CardInitMessage
{
    BaseCost = 1,
    Type = CardType.Skill,
    Rarity = CardRarity.Uncommon,
    Target = TargetType.Self
})
{
    /// <inheritdoc />
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
        [];

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // TODO: 实现卡牌效果
        await Task.CompletedTask;
    }
}

/**
 * Title = ""
 *
 * Description = """
 * 移除 **9 (12)** 点**腐朽**。到下回合开始前，受到攻击伤害时，获得其数值一半的**体表精液**。
 * """
 *
 * Flavor = ""
 */
public class ExposeSelf() : LupaBaseCard(new CardInitMessage
{
    BaseCost = 1,
    Type = CardType.Skill,
    Rarity = CardRarity.Uncommon,
    Target = TargetType.Self
})
{
    /// <inheritdoc />
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
        [];

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // TODO: 实现卡牌效果
        await Task.CompletedTask;
    }
}

/**
 * Title = ""
 *
 * Description = """
 * 失去所有**地板精液**和**体表精液**，获得等价值**快感**和等量**体内精液**。
 * """
 *
 * Flavor = ""
 */
public class PutEveryCumInBody() : LupaBaseCard(new CardInitMessage
{
    BaseCost = 2,
    Type = CardType.Skill,
    Rarity = CardRarity.Uncommon,
    Target = TargetType.Self
})
{
    /// <inheritdoc />
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
        [];

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // TODO: 实现卡牌效果
        await Task.CompletedTask;
    }
}

/**
 * Title = ""
 *
 * Description = """
 * 从弃牌堆中选择最多 **3 (4)** 张牌，将它们升级并放入抽牌堆。
 * """
 *
 * Flavor = ""
 */
public class NakedToSchool() : LupaBaseCard(new CardInitMessage
{
    BaseCost = 1,
    Type = CardType.Skill,
    Rarity = CardRarity.Uncommon,
    Target = TargetType.Self
})
{
    /// <inheritdoc />
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
        [];

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // TODO: 实现卡牌效果
        await Task.CompletedTask;
    }
}

/**
 * Title = ""
 *
 * Description = """
 * 获得 **10 (16)** 点**引诱格挡**。获得 **1** 回合**激情**。消耗。
 * """
 *
 * Flavor = ""
 */
public class NudeLive() : LupaBaseCard(new CardInitMessage
{
    BaseCost = 2,
    Type = CardType.Skill,
    Rarity = CardRarity.Uncommon,
    Target = TargetType.Self
})
{
    /// <inheritdoc />
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
        [];

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // TODO: 实现卡牌效果
        await Task.CompletedTask;
    }
}

/**
 * Title = ""
 *
 * Description = """
 * 失去所有的格挡，获得 1/**3 (1/2)** 的**多层护甲**。
 * """
 *
 * Flavor = ""
 */
public class Philter() : LupaBaseCard(new CardInitMessage
{
    BaseCost = 1,
    Type = CardType.Skill,
    Rarity = CardRarity.Uncommon,
    Target = TargetType.Self
})
{
    /// <inheritdoc />
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
        [];

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // TODO: 实现卡牌效果
        await Task.CompletedTask;
    }
}

/**
 * Title = ""
 *
 * Description = """
 * 获得 **4 (8)** 点**引诱格挡**。本回合获得 **2 (3)** 次**激情**。
 * """
 *
 * Flavor = ""
 */
public class PrivatePhoto() : LupaBaseCard(new CardInitMessage
{
    BaseCost = 1,
    Type = CardType.Skill,
    Rarity = CardRarity.Common,
    Target = TargetType.Self
})
{
    /// <inheritdoc />
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
        [];

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // TODO: 实现卡牌效果
        await Task.CompletedTask;
    }
}

/**
 * Title = ""
 *
 * Description = """
 * 失去所有的**精液**，每失去1点价值的**精液**，移除 **?** 的**腐朽**。
 * """
 *
 * Flavor = ""
 */
public class SemenBath() : LupaBaseCard(new CardInitMessage
{
    BaseCost = 3,
    Type = CardType.Skill,
    Rarity = CardRarity.Rare,
    Target = TargetType.Self
})
{
    /// <inheritdoc />
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
        [];

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // TODO: 实现卡牌效果
        await Task.CompletedTask;
    }
}

/**
 * Title = ""
 *
 * Description = """
 * 当你打出**性交牌**时，获得对应的印记，集齐5个后，自动打出一张**轮奸派对**。使得除了印记本身的数值外，每多一种印记，**轮奸派对**提高 **15% (20%)** 的数值。
 * """
 *
 * Flavor = ""
 */
public class GangBangPrepare() : LupaBaseCard(new CardInitMessage
{
    BaseCost = 2,
    Type = CardType.Power,
    Rarity = CardRarity.Rare,
    Target = TargetType.Self
})
{
    /// <inheritdoc />
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
        [];

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // TODO: 实现卡牌效果
        await Task.CompletedTask;
    }
}

/**
 * Title = ""
 *
 * Description = """
 * 受到攻击伤害而失去生命时，获得1张随机**状态**牌。所有**消耗**牌会送入**弃牌堆**。
 * """
 *
 * Flavor = ""
 */
public class Ku_Koro() : LupaBaseCard(new CardInitMessage
{
    BaseCost = 1,
    Type = CardType.Power,
    Rarity = CardRarity.Rare,
    Target = TargetType.Self
})
{
    /// <inheritdoc />
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
        [];

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // TODO: 实现卡牌效果
        await Task.CompletedTask;
    }
}

/**
 * Title = ""
 *
 * Description = """
 * 获得**精液**时，同时获得等量的**佳酿**。当收集满 **10 (15)** **佳酿**或战斗结束时，回复**佳酿**数量的生命。消耗。
 * """
 *
 * Flavor = ""
 */
public class DrinkSemenBeer() : LupaBaseCard(new CardInitMessage
{
    BaseCost = 0,
    Type = CardType.Power,
    Rarity = CardRarity.Uncommon,
    Target = TargetType.Self
})
{
    /// <inheritdoc />
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
        [];

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // TODO: 实现卡牌效果
        await Task.CompletedTask;
    }
}

/**
 * Title = ""
 *
 * Description = """
 * 当你获得**精液**时，获得其价值的 **1 (2)** 点**引诱格挡**。
 * """
 *
 * Flavor = ""
 */
public class SemenTattoo() : LupaBaseCard(new CardInitMessage
{
    BaseCost = 2,
    Type = CardType.Power,
    Rarity = CardRarity.Uncommon,
    Target = TargetType.Self
})
{
    /// <inheritdoc />
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
        [];

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // TODO: 实现卡牌效果
        await Task.CompletedTask;
    }
}

/**
 * Title = ""
 *
 * Description = """
 * **精欲** **3**：获得 **13 (18)** 点**精液格挡**。
 * """
 *
 * Flavor = ""
 */
public class DrySemen() : LupaBaseCard(new CardInitMessage
{
    BaseCost = 1,
    Type = CardType.Skill,
    Rarity = CardRarity.Basic,
    Target = TargetType.Self
})
{
    /// <inheritdoc />
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
        [];

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // TODO: 实现卡牌效果
        await Task.CompletedTask;
    }
}

/**
 * Title = ""
 *
 * Description = """
 * 造成 **12 (16)** 点**伤害**。**连绵**：**精欲** **3**：从抽牌堆中打出一张非**精液润滑**的**攻击牌**。
 * """
 *
 * Flavor = ""
 */
public class SemenLubricate() : LupaBaseCard(new CardInitMessage
{
    BaseCost = 3,
    Type = CardType.Attack,
    Rarity = CardRarity.Rare,
    Target = TargetType.AnyEnemy
})
{
    /// <inheritdoc />
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
        [];

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // TODO: 实现卡牌效果
        await Task.CompletedTask;
    }
}

/**
 * Title = ""
 *
 * Description = """
 * **精欲** **4**：造成 **18 (24)** 点**伤害**。
 * """
 *
 * Flavor = ""
 */
public class SemenMagic() : LupaBaseCard(new CardInitMessage
{
    BaseCost = 1,
    Type = CardType.Attack,
    Rarity = CardRarity.Uncommon,
    Target = TargetType.AnyEnemy
})
{
    /// <inheritdoc />
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
        [];

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // TODO: 实现卡牌效果
        await Task.CompletedTask;
    }
}

/**
 * Title = ""
 *
 * Description = """
 * 目标失去所有的**通用负面效果**和**人工制品**。在弃牌堆中添加一张**分娩**。获得**妊娠** **8 (11)** ：流产时，自身获得目标失去效果的两倍。
 * """
 *
 * Flavor = ""
 */
public class UnBirth() : MasoBaseCard(new CardInitMessage
{
    BaseCost = 1,
    Type = CardType.Skill,
    Rarity = CardRarity.Uncommon,
    Target = TargetType.AnyEnemy // TODO: 实际上应该是SelfOrEnemy，目前没有实现
})
{
    /// <inheritdoc />
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
        [];

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // TODO: 实现卡牌效果
        await Task.CompletedTask;
    }
}

/**
 * Title = ""
 *
 * Description = """
 * 在弃牌堆中添加一张**分娩**。获得 **10 (13)** **妊娠**：顺产：生成和目标对应的**宝宝**。
 * """
 *
 * Flavor = ""
 */
public class HaveBirthWith() : SuperstitioBaseCard(new CardInitMessage
{
    BaseCost = 1,
    Type = CardType.Skill,
    Rarity = CardRarity.Common,
    Target = TargetType.AnyEnemy // TODO: 实际上应该是SelfOrEnemy，目前没有实现
})
{
    /// <inheritdoc />
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
        [];

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // TODO: 实现卡牌效果
        await Task.CompletedTask;
    }
}

/**
 * Title = ""
 *
 * Description = """
 * 目标敌人获得**脆弱**和**虚弱**，失去上限的 **15% (20%)** 生命，失去的生命转化为**格挡**。给予**壁垒**。消耗。
 * """
 *
 * Flavor = ""
 */
public class Tease() : SuperstitioBaseCard(new CardInitMessage
{
    BaseCost = 0,
    Type = CardType.Skill,
    Rarity = CardRarity.Uncommon,
    Target = TargetType.AnyEnemy
})
{
    /// <inheritdoc />
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
        [];

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // TODO: 实现卡牌效果
        await Task.CompletedTask;
    }
}

/**
 * Title = ""
 *
 * Description = """
 * **并行**。移除 **7 (10)** 点**腐朽**。所有**助兴序列**中卡牌的计数增加 **3 (4)** 。
 * """
 *
 * Flavor = ""
 */
public class OneMoreHour() : SuperstitioBaseCard(new CardInitMessage
{
    BaseCost = 1,
    Type = CardType.Skill,
    Rarity = CardRarity.Common,
    Target = TargetType.Self
})
{
    /// <inheritdoc />
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
        [];

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // TODO: 实现卡牌效果
        await Task.CompletedTask;
    }
}

/**
 * Title = ""
 *
 * Description = """
 * **并行**。移除 **14 (18)** 点**腐朽**。**强制满足** **2 (3)** 次。
 * """
 *
 * Flavor = ""
 */
public class PassiveGangBang() : SuperstitioBaseCard(new CardInitMessage
{
    BaseCost = 2,
    Type = CardType.Skill,
    Rarity = CardRarity.Rare,
    Target = TargetType.Self
})
{
    /// <inheritdoc />
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
        [];

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // TODO: 实现卡牌效果
        await Task.CompletedTask;
    }
}

/**
 * Title = ""
 *
 * Description = """
 * 选择一张手牌，将其**送入助兴序列**并**欲求**打出任意牌 **8 (6)** 次：打出复制后被**丢弃**。
 * """
 *
 * Flavor = ""
 */
public class ReadyToSex() : SuperstitioBaseCard(new CardInitMessage
{
    BaseCost = 0,
    Type = CardType.Skill,
    Rarity = CardRarity.Uncommon,
    Target = TargetType.Self
})
{
    /// <inheritdoc />
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
        [];

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // TODO: 实现卡牌效果
        await Task.CompletedTask;
    }
}

/**
 * Title = ""
 *
 * Description = """
 * 选择最多 **2** 张手牌，抽对应数量的牌，将选中手牌**送入助兴序列**并**欲求**打出任意牌 **5 (3)** 次：送入手牌，本回合耗能减少 **1** 。自动释放。
 * """
 *
 * Flavor = ""
 */
public class RideDildoBike() : SuperstitioBaseCard(new CardInitMessage
{
    BaseCost = 1,
    Type = CardType.Skill,
    Rarity = CardRarity.Uncommon,
    Target = TargetType.Self
})
{
    /// <inheritdoc />
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
        [];

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // TODO: 实现卡牌效果
        await Task.CompletedTask;
    }
}

/**
 * Title = ""
 *
 * Description = """
 * 添加 **2** 张**情趣玩具**到手牌中。消耗。
 * """
 *
 * Flavor = ""
 */
public class AddSexToy() : SuperstitioBaseCard(new CardInitMessage
{
    BaseCost = 2,
    Type = CardType.Skill,
    Rarity = CardRarity.Uncommon,
    Target = TargetType.Self
})
{
    /// <inheritdoc />
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
        [];

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // TODO: 实现卡牌效果
        await Task.CompletedTask;
    }
}

/**
 * Title = ""
 *
 * Description = """
 * 获得 **X * 15 (20)** 点**快感**。下回合开始时，消耗 **1** 张牌。重复X次。消耗。
 * """
 *
 * Flavor = ""
 */
public class ForceOrgasm() : SuperstitioBaseCard(new CardInitMessage
{
    BaseCost = -1,
    Type = CardType.Skill,
    Rarity = CardRarity.Uncommon,
    Target = TargetType.Self
})
{
    /// <inheritdoc />
    protected override bool HasEnergyCostX => true;

    /// <inheritdoc />
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
        [];

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // TODO: 实现卡牌效果
        await Task.CompletedTask;
    }
}

/**
 * Title = ""
 *
 * Description = """
 * 佩戴 **1 (2)** **情趣玩具**，失去 **1** 生命，触发所有**情趣玩具**的效果。
 * """
 *
 * Flavor = ""
 */
public class ForcePutSexToy() : SuperstitioBaseCard(new CardInitMessage
{
    BaseCost = 1,
    Type = CardType.Skill,
    Rarity = CardRarity.Common,
    Target = TargetType.Self
})
{
    /// <inheritdoc />
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
        [];

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // TODO: 实现卡牌效果
        await Task.CompletedTask;
    }
}

/**
 * Title = ""
 *
 * Description = """
 * 持续 **1** 回合。**快感**不改变，且不再抽卡。在**回合结束**或**你的回合结束**时，你的**负面效果和强化效果**均不触发。回合开始时，释放期间产生的**快感**的2倍。
 * """
 *
 * Flavor = ""
 */
public class TimeStop() : SuperstitioBaseCard(new CardInitMessage
{
    BaseCost = 2,
    Type = CardType.Skill,
    Rarity = CardRarity.Rare,
    Target = TargetType.Self
})
{
    /// <inheritdoc />
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
        [];

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // TODO: 实现卡牌效果
        await Task.CompletedTask;
    }
}

/**
 * Title = ""
 *
 * Description = """
 * 获得 **5** 点**引诱格挡**。下回合获得 **10 (15)** **快感**。抽 **2** 张牌。
 * """
 *
 * Flavor = ""
 */
public class Endure() : SuperstitioBaseCard(new CardInitMessage
{
    BaseCost = 1,
    Type = CardType.Skill,
    Rarity = CardRarity.Uncommon,
    Target = TargetType.Self
})
{
    /// <inheritdoc />
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
        [];

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // TODO: 实现卡牌效果
        await Task.CompletedTask;
    }
}

/**
 * Title = ""
 *
 * Description = """
 * 获得当前负面状态 **?** 倍的**快感**。从手牌中**丢弃**所有**状态**和**诅咒**，每**丢弃**一张，获得能量，抽一张牌。消耗。（升级后移除消耗）
 * """
 *
 * Flavor = ""
 */
public class EscapeConjuring() : SuperstitioBaseCard(new CardInitMessage
{
    BaseCost = 0,
    Type = CardType.Skill,
    Rarity = CardRarity.Uncommon,
    Target = TargetType.Self
})
{
    /// <inheritdoc />
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
        [];

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // TODO: 实现卡牌效果
        await Task.CompletedTask;
    }
}

/**
 * Title = ""
 *
 * Description = """
 * 抽 **3 (4)** 张牌。下回合开始时，消耗 **1** 张牌。
 * """
 *
 * Flavor = ""
 */
public class Overdraft() : SuperstitioBaseCard(new CardInitMessage
{
    BaseCost = 1,
    Type = CardType.Skill,
    Rarity = CardRarity.Common,
    Target = TargetType.Self
})
{
    /// <inheritdoc />
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
        [];

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // TODO: 实现卡牌效果
        await Task.CompletedTask;
    }
}

/**
 * Title = ""
 *
 * Description = """
 * 从抽牌堆中选择最多 **4 (6)** 张牌并**消耗**。消耗。
 * """
 *
 * Flavor = ""
 */
public class BecomeTrash() : SuperstitioBaseCard(new CardInitMessage
{
    BaseCost = 2,
    Type = CardType.Skill,
    Rarity = CardRarity.Uncommon,
    Target = TargetType.Self
})
{
    /// <inheritdoc />
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
        [];

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // TODO: 实现卡牌效果
        await Task.CompletedTask;
    }
}

/**
 * Title = ""
 *
 * Description = """
 * 从卡组中任选1张牌，将其复制加入手牌。消耗。（升级后获得保留）
 * """
 *
 * Flavor = ""
 */
public class GloryHole() : SuperstitioBaseCard(new CardInitMessage
{
    BaseCost = 2,
    Type = CardType.Skill,
    Rarity = CardRarity.Rare,
    Target = TargetType.Self
})
{
    /// <inheritdoc />
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
        [];

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // TODO: 实现卡牌效果
        await Task.CompletedTask;
    }
}

/**
 * Title = ""
 *
 * Description = """
 * 从3种魔物娘形态中选择1种获得。
 * """
 *
 * Flavor = ""
 */
public class MonsterGirlMode() : SuperstitioBaseCard(new CardInitMessage
{
    BaseCost = 2,
    Type = CardType.Power,
    Rarity = CardRarity.Rare,
    Target = TargetType.Self
})
{
    /// <inheritdoc />
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
        [];

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // TODO: 实现卡牌效果
        await Task.CompletedTask;
    }
}

/**
 * Title = ""
 *
 * Description = """
 * 获得 **?** 层**窒息**。
 * """
 *
 * Flavor = ""
 */
public class ChokeChoker() : SuperstitioBaseCard(new CardInitMessage
{
    BaseCost = 1,
    Type = CardType.Power,
    Rarity = CardRarity.Uncommon,
    Target = TargetType.Self
})
{
    /// <inheritdoc />
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
        [];

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // TODO: 实现卡牌效果
        await Task.CompletedTask;
    }
}

/**
 * Title = ""
 *
 * Description = """
 * 在你的回合结束时，**保留**最多 **1** 张牌，并让其下次打出时不消耗能量。
 * """
 *
 * Flavor = ""
 */
public class HideInPussy() : SuperstitioBaseCard(new CardInitMessage
{
    BaseCost = 2,
    Type = CardType.Power,
    Rarity = CardRarity.Rare,
    Target = TargetType.Self
})
{
    /// <inheritdoc />
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
        [];

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // TODO: 实现卡牌效果
        await Task.CompletedTask;
    }
}

/**
 * Title = ""
 *
 * Description = """
 * **缠绵**打出攻击牌：对敌方全体造成 **2** 点伤害。
 * """
 *
 * Flavor = ""
 */
public class Ahegao() : SuperstitioBaseCard(new CardInitMessage
{
    BaseCost = 2,
    Type = CardType.Attack,
    Rarity = CardRarity.Uncommon,
    Target = TargetType.AllEnemies
})
{
    /// <inheritdoc />
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
        [];

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // TODO: 实现卡牌效果
        await Task.CompletedTask;
    }
}

/**
 * Title = ""
 *
 * Description = """
 * 失去 **5** 点生命。造成等同于当前**腐朽**总量的伤害。（预计为 !D! 点）
 * """
 *
 * Flavor = ""
 */
public class EroSion() : SuperstitioBaseCard(new CardInitMessage
{
    BaseCost = 1,
    Type = CardType.Attack,
    Rarity = CardRarity.Common,
    Target = TargetType.AnyEnemy
})
{
    /// <inheritdoc />
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
        [];

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // TODO: 实现卡牌效果
        await Task.CompletedTask;
    }
}

/**
 * Title = ""
 *
 * Description = """
 * 抽1张牌，使随机敌人流失 **2 (3)** 点生命。若回合结束时在手牌中，**消耗**并把原本的卡牌加入弃牌堆。
 * """
 *
 * Flavor = ""
 */
public class FeelPhantomBody() : SuperstitioBaseCard(new CardInitMessage
{
    BaseCost = 0,
    Type = CardType.Status,
    Rarity = CardRarity.Token,
    Target = TargetType.Self
})
{
    /// <inheritdoc />
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
        [];

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // TODO: 实现卡牌效果
        await Task.CompletedTask;
    }
}

/**
 * Title = ""
 *
 * Description = """
 * **自动打出**。**缠绵**回合结束 **1** 次：对所有敌人造成 !D! 点伤害。获得 !B! **干涸精痕**。
 * """
 *
 * Flavor = ""
 */
public class GangBang() : SuperstitioBaseCard(new CardInitMessage
{
    BaseCost = 0,
    Type = CardType.Status,
    Rarity = CardRarity.Rare,
    Target = TargetType.AllEnemies
})
{
    /// <inheritdoc />
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
        [];

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // TODO: 实现卡牌效果
        await Task.CompletedTask;
    }
}

/**
 * Title = ""
 *
 * Description = """
 * 获得 **5 (8)** 临时生命。移除场上所有**妊娠状态**，每移除一个，再获得一次格挡。消耗。
 * """
 *
 * Flavor = ""
 */
public class GiveBirth() : SuperstitioBaseCard(new CardInitMessage
{
    BaseCost = 0,
    Type = CardType.Skill,
    Rarity = CardRarity.Token,
    Target = TargetType.Self
})
{
    /// <inheritdoc />
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
        [];

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // TODO: 实现卡牌效果
        await Task.CompletedTask;
    }
}

/**
 * Title = ""
 *
 * Description = """
 * **时空递归**。移除 **5 (8)** 点**腐朽**。自动打出，被消耗时，将一张复制加入手牌。
 * """
 *
 * Flavor = ""
 */
public class SelfReference() : SuperstitioBaseCard(new CardInitMessage
{
    BaseCost = -1,
    Type = CardType.Curse,
    Rarity = CardRarity.Event,
    Target = TargetType.Self,
    ShowInCardLibrary = false
})
{
    /// <inheritdoc />
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Unplayable,
    ];

    /// <inheritdoc />
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
        [];

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // TODO: 实现卡牌效果
        await Task.CompletedTask;
    }
}

/**
 * Title = ""
 *
 * Description = """
 * 给目标装备 **2 (3)** 个随机**情趣玩具**。
 * """
 *
 * Flavor = ""
 */
public class SexToy() : SuperstitioBaseCard(new CardInitMessage
{
    BaseCost = 0,
    Type = CardType.Skill,
    Rarity = CardRarity.Token,
    Target = TargetType.AnyEnemy // TODO: 实际上应该是SelfOrEnemy，目前没有实现
})
{
    /// <inheritdoc />
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
        [];

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // TODO: 实现卡牌效果
        await Task.CompletedTask;
    }
}

/**
 * Title = ""
 *
 * Description = """
 * 给予**敌我双方** **1 (2)** 层易伤。消耗，保留。
 * """
 *
 * Flavor = ""
 */
public class VulnerableTogether() : SuperstitioBaseCard(new CardInitMessage
{
    BaseCost = 0,
    Type = CardType.Skill,
    Rarity = CardRarity.Token,
    Target = TargetType.AllEnemies // 实际上也包括自身
})
{
    /// <inheritdoc />
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
        [];

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // TODO: 实现卡牌效果
        await Task.CompletedTask;
    }
}