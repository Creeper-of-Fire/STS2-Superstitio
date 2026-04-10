using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Superstitio.Main.Features.Corruptus;
using Superstitio.Main.Features.Felix;
using Superstitio.Main.Maso.Base;
using Superstitio.Main.SubPool;
using Superstitio.Main.Utils;

namespace Superstitio.Main.Maso.Relics;

/// <summary>
/// Maso 的初始遗物
/// 单次获得的腐朽每有3点获得2点快感。达到顶峰时，移除5腐朽 。
/// </summary>
[Pool(typeof(MasoRelicPool))]
[CustomImgName("DevaBody")]
public class MasoStartRelic : CardPoolSelectionRelic, ICorruptusBuffer, IAfterClimaxReached
{
    private const int CorruptusReduceWhenClimax = 5;

    /// <inheritdoc />
    public override RelicRarity Rarity => RelicRarity.Starter;

    /// <inheritdoc />
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        ..base.ExtraHoverTips,
        HoverTipFactory.FromPower<CorruptusPower>(),
        HoverTipFactory.FromPower<FelixPower>()
    ];
    

    /// <inheritdoc />
    public Creature OwnerCreature => this.Owner.Creature;

    /// <inheritdoc />
    public CorruptusBufferComponent CorruptusBufferComponent => field ??= new CorruptusBufferComponent(this);

    /// <inheritdoc />
    public override decimal ModifyHpLostAfterOstyLate(
        Creature target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource
    ) => this.CorruptusBufferComponent.ModifyHpLostAfterOstyLate(target, amount, props, dealer, cardSource);

    /// <inheritdoc />
    public override Task AfterModifyingHpLostAfterOsty() =>
        this.CorruptusBufferComponent.AfterModifyingHpLostAfterOsty();

    /// <inheritdoc />
    public override async Task AfterPowerAmountChanged(PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power.Owner != this.OwnerCreature)
            return;
        if (power is not CorruptusPower)
            return;
        // 仅在腐朽增加时触发（amount >= 0）
        if (amount < 0)
            return;

        // 根据增加的腐朽量，按比例给予快感（每 3 点腐朽获得 2 点快感）
        decimal felixGain = Math.Floor(amount / 3m) * 2m;

        if (felixGain > 0)
        {
            await PowerCmd.Apply<FelixPower>(this.OwnerCreature, felixGain, applier, cardSource);
        }
    }

    /// <inheritdoc />
    public async Task AfterClimaxReached(Creature powerOwner, FelixPower felixPower, Creature? applier, CardModel? cardSource)
    {
        if (powerOwner.Player is null || powerOwner != this.OwnerCreature)
            return;

        await CorruptusManager.DecreaseCorruptus(powerOwner, CorruptusReduceWhenClimax, applier, cardSource);
    }
}