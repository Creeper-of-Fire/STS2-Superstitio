using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Superstitio.Main.Features.Corruptus;
using Superstitio.Main.Features.Rage;
using Superstitio.Main.Maso.Pools;

namespace Superstitio.Main.Maso.Relics;

/// <summary>
/// Maso 的初始遗物
/// </summary>
[Pool(typeof(MasoRelicPool))]
public class MasoStartRelic : CustomRelicModel, ICorruptusBuffer, IAfterRageThresholdReached
{
    /// <inheritdoc />
    public override RelicRarity Rarity => RelicRarity.Starter;

    /// <inheritdoc />
    public CorruptusBufferComponent CorruptusBufferComponent => field ??= new(this);

    /// <inheritdoc />
    public Creature OwnerCreature => this.Owner.Creature;

    /// <inheritdoc />
    public override decimal ModifyHpLostAfterOstyLate(Creature target,
        decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource) =>
        this.CorruptusBufferComponent.ModifyHpLostAfterOstyLate(target, amount, props, dealer, cardSource);

    /// <inheritdoc />
    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side != this.OwnerCreature.Side)
            return;
        await this.CorruptusBufferComponent.TriggerCorruptusDamage(choiceContext);
    }

    /// <inheritdoc />
    public async Task AfterRageThresholdReached(Creature powerOwner, RagePower ragePower, Creature? applier, CardModel? cardSource)
    {
        if (powerOwner.Player is null || powerOwner != this.OwnerCreature)
            return;

        await PowerCmd.Apply<RageDiscountPower>(powerOwner, 1, applier, cardSource);
    }

    private const int RageGetPerEnergy = 2;

    /// <inheritdoc />
    public override async Task AfterEnergySpent(CardModel card, int amount)
    {
        await PowerCmd.Apply<RagePower>(this.OwnerCreature, amount * RageGetPerEnergy, null, card);
    }
}