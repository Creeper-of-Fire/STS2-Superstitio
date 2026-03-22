using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using Superstitio.Main.Extensions;
using Superstitio.Main.Maso.Cards.Base;
using Superstitio.Main.Maso.Cards.Kongfu;
using Superstitio.Main.Maso.Pools;
using Superstitio.Main.Maso.Relics;
using Superstitio.Main.SubPool;
using Superstitio.Main.Utils;

namespace Superstitio.Main.Maso;

/// <summary>
/// 
/// </summary>
public class MasoCharacter : SuperstitioCharacter
{
    /// <inheritdoc />
    protected override CharacterColorAssets ColorsConfig => field ??= new()
    {
        NameColor = new Color(0.5f, 0.5f, 1f)
    };

    /// <inheritdoc />
    protected override CharacterStatsAssets StatsConfig => field ??= new()
    {
        StartingHp = 70,
        Gender = CharacterGender.Feminine,
    };

    /// <inheritdoc />
    protected override CharacterLoadoutAssets LoadoutConfig => field ??= new()
    {
        CardPool = ModelDb.CardPool<MasoCardPool>(),
        RelicPool = ModelDb.RelicPool<MasoRelicPool>(),
        PotionPool = ModelDb.PotionPool<MasoPotionPool>(),
        StartingDeck =
        [
            ..StrikeMaso.Card().Repeat(4),
            ..DefendMaso.Card().Repeat(4),
            RageCharge.Card(),
            SuspendingStrike.Card(),
        ],
        StartingRelics =
        [
            MasoStartRelic.Relic(),
            CardPoolSelectionRelic.Relic(),
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