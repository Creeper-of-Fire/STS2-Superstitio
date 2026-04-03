using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using Superstitio.Main.Base;
using Superstitio.Main.Extensions;
using Superstitio.Main.Lupa.Base;
using Superstitio.Main.Lupa.Cards.Base;
using Superstitio.Main.Lupa.Relics;
using Superstitio.Main.Maso.Cards.Kongfu;

namespace Superstitio.Main.Lupa;

/// <summary>
/// 
/// </summary>
public class LupaCharacter : SuperstitioCharacter
{
    /// <inheritdoc />
    protected override CharacterColorAssets ColorsConfig => field ??= new CharacterColorAssets
    {
        NameColor = new Color(0.5f, 0.5f, 1f)
    };

    /// <inheritdoc />
    protected override CharacterStatsAssets StatsConfig => field ??= new CharacterStatsAssets
    {
        StartingHp = 70,
        Gender = CharacterGender.Feminine,
    };

    /// <inheritdoc />
    protected override CharacterLoadoutAssets LoadoutConfig => field ??= new CharacterLoadoutAssets
    {
        CardPool = ModelDb.CardPool<LupaCardPool>(),
        RelicPool = ModelDb.RelicPool<LupaRelicPool>(),
        PotionPool = ModelDb.PotionPool<LupaPotionPool>(),
        StartingDeck =
        [
            ..StrikeLupa.Card().Repeat(4),
            ..DefendLupa.Card().Repeat(4),
            Masturbate.Card(),
            KokiFoot.Card(),
        ],
        StartingRelics =
        [
            LupaStartRelic.Relic(),
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