using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Superstitio.Main.Features.Corruptus;
using Superstitio.Main.Maso.Pools;

namespace Superstitio.Main.Maso.Relics;

/// <summary>
/// Maso 的初始遗物
/// </summary>
[Pool(typeof(MasoRelicPool))]
public class MasoStartRelic : CustomRelicModel, ICorruptusBuffer
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
}