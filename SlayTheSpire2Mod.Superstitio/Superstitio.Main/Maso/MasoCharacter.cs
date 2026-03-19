using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using Superstitio.Main.Utils;

namespace Superstitio.Main.Maso;

/// <summary>
/// 
/// </summary>
public class MasoCharacter : SuperstitioCharacter
{
    /// <inheritdoc />
    protected override CharacterColorAssets ColorsConfig { get; } = new()
    {
        NameColor = new Color(0.5f, 0.5f, 1f)
    };

    /// <inheritdoc />
    protected override CharacterStatsAssets StatsConfig { get; } = new()
    {
        StartingHp = 70,
        Gender = CharacterGender.Feminine,
    };

    /// <inheritdoc />
    protected override CharacterLoadoutAssets LoadoutConfig { get; } = new()
    {
        CardPool = ModelDb.CardPool<MonkCardPool>(),
        RelicPool = ModelDb.RelicPool<MonkRelicPool>(),
        PotionPool = ModelDb.PotionPool<MonkPotionPool>(),
        StartingDeck = [
            ModelDb.Card<MonkCard>(),
            ModelDb.Card<MonkCard>(),
            ModelDb.Card<MonkCard>(),
        ],
        StartingRelics = [
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

