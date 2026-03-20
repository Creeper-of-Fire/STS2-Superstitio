using System.Diagnostics.CodeAnalysis;
using BaseLib.Abstracts;
using Godot;
using JetBrains.Annotations;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using Superstitio.Main.Maso.Cards.Base;
using Superstitio.Main.Maso.Cards.Kongfu;
using Superstitio.Main.Utils;

namespace Superstitio.Main.Maso;

/// <summary>
/// 
/// </summary>
public class MasoCharacter : SuperstitioCharacter
{
    /// <inheritdoc />
    [field: AllowNull, CanBeNull]
    protected override CharacterColorAssets ColorsConfig => field ??= new()
    {
        NameColor = new Color(0.5f, 0.5f, 1f)
    };

    /// <inheritdoc />
    [field: AllowNull, CanBeNull]
    protected override CharacterStatsAssets StatsConfig => field ??= new()
    {
        StartingHp = 70,
        Gender = CharacterGender.Feminine,
    };

    /// <inheritdoc />
    [field: AllowNull, CanBeNull]
    protected override CharacterLoadoutAssets LoadoutConfig => field ??= new()
    {
        CardPool = ModelDb.CardPool<MasoCardPool>(),
        RelicPool = ModelDb.RelicPool<MonkRelicPool>(),
        PotionPool = ModelDb.PotionPool<MonkPotionPool>(),
        StartingDeck =
        [
            ModelDb.Card<StrikeMaso>(),
            ModelDb.Card<StrikeMaso>(),
            ModelDb.Card<StrikeMaso>(),
            ModelDb.Card<StrikeMaso>(),
            // ModelDb.Card<DefendMaso>(),
            // ModelDb.Card<DefendMaso>(),
            // ModelDb.Card<DefendMaso>(),
            // ModelDb.Card<DefendMaso>(),
            ModelDb.Card<SuspendingStrike>(),
        ],
        StartingRelics =
        [
            ModelDb.Relic<MonkRelic>()
        ],
    };

    /// <inheritdoc />
    protected override CharacterVisualAssets? VisualAssets => null;

    /// <inheritdoc />
    protected override CharacterUiAssets? UiAssets => null;

    /// <inheritdoc />
    protected override CharacterMultiplayerAssets? MultiplayerAssets => null;

    /// <inheritdoc />
    protected override CharacterSfxAssets? SfxAssets => null;
}

/// <summary>
/// 职业的药水池模型。
/// </summary>
public class MasoPotionPool : CustomPotionPoolModel
{
    /// <summary>
    /// 能量图标。
    /// </summary>
    public override string EnergyColorName => "ironclad";
}

/// <summary>
/// 职业的遗物池模型。
/// </summary>
public class MasoRelicPool : CustomRelicPoolModel
{
    /// <summary>
    /// 能量图标。
    /// </summary>
    public override string EnergyColorName => "ironclad";
}