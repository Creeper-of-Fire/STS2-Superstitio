using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using Superstitio.Main.Features.Felix;
using Superstitio.Main.Lupa.Base;
using Superstitio.Main.SubPool;

namespace Superstitio.Main.Lupa.Relics;

/// <summary>
/// Lupa 的初始遗物，消耗能量时获得快感，她靠卡牌来把伤害转换为腐朽，而非在这里处理。腐朽转为全局效果。
/// </summary>
[Pool(typeof(LupaRelicPool))]
public class LupaStartRelic : CardPoolSelectionRelic, IAfterClimaxReached
{
    /// <inheritdoc />
    public override RelicRarity Rarity => RelicRarity.Starter;

    private const int FelixGetPerEnergy = 2;

    /// <inheritdoc />
    public async Task AfterClimaxReached(Creature powerOwner, FelixPower felixPower, Creature? applier, CardModel? cardSource)
    {
        if (powerOwner.Player is null || powerOwner != this.Owner.Creature)
            return;

        await PowerCmd.Apply<FelixDiscountPower>(powerOwner, 1, applier, cardSource);
    }

    /// <inheritdoc />
    public override async Task AfterEnergySpent(CardModel card, int amount)
    {
        if (amount <= 0 || this.Owner.Creature.Player != card.Owner)
            return;

        await PowerCmd.Apply<FelixPower>(this.Owner.Creature, amount * FelixGetPerEnergy, null, card);
    }
}