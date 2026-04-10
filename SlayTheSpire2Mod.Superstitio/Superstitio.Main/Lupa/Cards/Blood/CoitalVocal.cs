using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Superstitio.Main.Base;
using Superstitio.Main.Lupa.Base;

namespace Superstitio.Main.Lupa.Cards.Blood;

/// <summary>
/// 战吼 - 获得 在场敌人数量+1次的 !B! 点 *格挡
/// </summary>
public class CoitalVocal() : LupaBaseCard(new CardInitMessage
{
    BaseCost = 1,
    Type = CardType.Skill,
    Rarity = CardRarity.Common,
    Target = TargetType.Self
})
{
    private const int Block = 3;
    private const int BlockUpgrade = 1;

    /// <inheritdoc />
    protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
    [
        new BlockVar(Block, ValueProp.Move).WithUpgrade(BlockUpgrade)
    ];

    /// <inheritdoc />
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        // 获得在场敌人数量+1次的格挡
        int enemyCount = this.CombatState?.HittableEnemies.Count ?? 0;
        for (int i = 0; i < enemyCount + 1; i++)
        {
            await CommonActions.CardBlock(this, cardPlay);
        }
    }
}