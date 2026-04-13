using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using Superstitio.Main.Patches.LocPatch;
using static Superstitio.Main.Utils.SuperstitioLocStringFactory;

namespace Superstitio.Main.Base;

/// <summary>
/// 一个临时增益/减益效果的基础类，会在回合结束时移除并应用相应的内部能力效果。
/// </summary>
/// <typeparam name="TPower">内部应用的能力类型。</typeparam>
public abstract class TemporaryPower<TPower>() : SuperstitioBasePower(new PowerInitMessage
{
    Type = ModelDb.Power<TPower>().Type,
    InitStackType = PowerInitMessage.StackStyle.Normal
}), ITemporaryPower, IAddDumbVariablesToPowerDescription where TPower : PowerModel, new()
{
    /// <inheritdoc cref="ShouldIgnoreNextInstance"/>
    public void IgnoreNextInstance() => this.ShouldIgnoreNextInstance = true;

    /// <summary>
    /// 这个机制仅因<see cref="Misery"/>卡牌而存在
    /// 用法请参阅 <see cref="Misery.DoHackyThingsForSpecificPowers"/>
    /// </summary>
    private bool ShouldIgnoreNextInstance { get; set; }

    /// <summary>
    /// 获取产生此临时效果的来源模型（卡牌、药水或遗物）。
    /// </summary>
    public abstract AbstractModel OriginModel { get; }

    /// <summary>
    /// 获取内部应用的能力实例。
    /// </summary>
    public PowerModel InternallyAppliedPower { get; } = ModelDb.Power<TPower>();

    /// <summary>
    /// 获取一个值，指示此临时效果是否为正面效果（增益）。
    /// </summary>
    protected virtual bool IsPositive => this.Type is PowerType.Buff;

    /// <summary>
    /// 获取用于计算内部能力变化量的符号（1 表示增益，-1 表示减益）。
    /// </summary>
    private int Sign => !this.IsPositive ? -1 : 1;


    /// <inheritdoc />
    public override LocString Title =>
        this.OriginModel switch
        {
            CardModel cardModel => cardModel.TitleLocString,
            PotionModel potionModel => potionModel.Title,
            RelicModel relicModel => relicModel.Title,
            PowerModel powerModel => powerModel.Title,
            OrbModel orbModel => orbModel.Title,
            CharacterModel characterModel => characterModel.Title,
            MonsterModel monsterModel => monsterModel.Title,
            _ => throw new InvalidOperationException()
        };

    /// <inheritdoc />
    public override LocString Description =>
        CreateLocString(locTable, nameof(TemporaryPower<>), this.IsPositive ? "power" : "down", "description");

    /// <inheritdoc />
    protected override string SmartDescriptionLocKey =>
        CreateLocString(locTable, nameof(TemporaryPower<>), this.IsPositive ? "power" : "down", "smartDescription").LocEntryKey;

    /// <inheritdoc />
    public void AddDumbVariablesToPowerDescription(LocString description)
    {
        description.Add("PowerName", this.InternallyAppliedPower.Title);
    }

    /// <inheritdoc />
    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            var items = new List<IHoverTip>();
            var collection = this.OriginModel switch
            {
                CardModel card => [HoverTipFactory.FromCard(card)],
                PotionModel model => [HoverTipFactory.FromPotion(model)],
                RelicModel relic => HoverTipFactory.FromRelic(relic),
                PowerModel power => [HoverTipFactory.FromPower(power)],
                _ => throw new InvalidOperationException()
            };

            items.AddRange(collection);
            items.Add(HoverTipFactory.FromPower<TPower>());
            return items;
        }
    }

    /// <inheritdoc />
    public override async Task BeforeApplied(
        Creature target,
        decimal amount,
        Creature? applier,
        CardModel? cardSource)
    {
        if (this.ShouldIgnoreNextInstance)
        {
            this.ShouldIgnoreNextInstance = false;
            return;
        }

        await PowerCmd.Apply<TPower>(target, this.Sign * amount, applier, cardSource, true);
    }

    /// <inheritdoc />
    public override async Task AfterPowerAmountChanged(
        PowerModel power,
        decimal amount,
        Creature? applier,
        CardModel? cardSource)
    {
        var temporaryPower = this;
        if (amount == temporaryPower.Amount || power != temporaryPower)
            return;
        if (temporaryPower.ShouldIgnoreNextInstance)
        {
            temporaryPower.ShouldIgnoreNextInstance = false;
            return;
        }

        await PowerCmd.Apply<TPower>(temporaryPower.Owner, temporaryPower.Sign * amount, applier, cardSource, true);
    }

    /// <inheritdoc />
    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        var power = this;
        if (side != power.Owner.Side)
            return;
        power.Flash();
        await PowerCmd.Remove(power);
        await PowerCmd.Apply<TPower>(power.Owner, -power.Sign * power.Amount, power.Owner, null);
    }
}