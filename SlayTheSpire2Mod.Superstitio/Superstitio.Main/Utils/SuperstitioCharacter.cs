using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;

namespace Superstitio.Main.Utils;

/// <summary>
/// 角色全套色彩配置。
/// </summary>
public record struct CharacterColorAssets
{
    /// <summary>
    /// 角色名称颜色。
    /// </summary>
    public Color NameColor { get; init; }

    /// <summary>
    /// 能量图标/表盘的轮廓颜色。
    /// </summary>
    public Color? EnergyLabelOutlineColor { get; init; }

    /// <summary>
    /// 剧情/事件对话文本颜色。
    /// </summary>
    public Color? DialogueColor { get; init; }

    /// <summary>
    /// 地图路线绘制颜色。
    /// </summary>
    public Color? MapDrawingColor { get; init; }

    /// <summary>
    /// 多人模式远程定位线颜色。
    /// </summary>
    public Color? RemoteTargetingLineColor { get; init; }

    /// <summary>
    /// 多人模式远程定位线轮廓颜色。
    /// </summary>
    public Color? RemoteTargetingLineOutline { get; init; }
}

/// <summary>
/// 角色的基础数值与身份属性。
/// </summary>
public record struct CharacterStatsAssets
{
    /// <summary>
    /// 人物性别。
    /// </summary>
    public CharacterGender Gender { get; init; }

    /// <summary>
    /// 初始血量。
    /// </summary>
    public int StartingHp { get; init; }

    /// <summary>
    /// 初始金钱。
    /// </summary>
    public int? StartingGold { get; init; }

    /// <summary>
    /// 能量上限。
    /// </summary>
    public int? MaxEnergy { get; init; }

    /// <summary>
    /// 基础充能球槽位数量。
    /// </summary>
    public int? BaseOrbSlotCount { get; init; }

    /// <summary>
    /// 是否始终显示储君的辉星计数器。
    /// </summary>
    public bool? ShouldAlwaysShowStarCounter { get; init; }
}

/// <summary>
/// 角色战斗资源与初始配置。
/// </summary>
public record struct CharacterLoadoutAssets
{
    /// <summary>
    /// 卡牌池。
    /// </summary>
    public CardPoolModel CardPool { get; init; }

    /// <summary>
    /// 遗物池。
    /// </summary>
    public RelicPoolModel RelicPool { get; init; }

    /// <summary>
    /// 药水池。
    /// </summary>
    public PotionPoolModel PotionPool { get; init; }

    /// <summary>
    /// 初始卡组。
    /// </summary>
    public IEnumerable<CardModel> StartingDeck { get; init; }

    /// <summary>
    /// 初始遗物。
    /// </summary>
    public IReadOnlyList<RelicModel> StartingRelics { get; init; }

    /// <summary>
    /// 初始药水。
    /// </summary>
    public IReadOnlyList<PotionModel>? StartingPotions { get; init; }
}

/// <summary>
/// 角色视觉与动画资源配置
/// </summary>
public record struct CharacterVisualAssets
{
    /// <summary>
    /// 攻击动画延迟。
    /// </summary>
    public float? AttackAnimDelay { get; init; }

    /// <summary>
    /// 施法动画延迟。
    /// </summary>
    public float? CastAnimDelay { get; init; }

    /// <summary>
    /// 人物模型 tscn 路径。
    /// </summary>
    public string? VisualPath { get; init; }

    /// <summary>
    /// 卡牌拖尾路径。
    /// </summary>
    public string? TrailPath { get; init; }

    /// <summary>
    /// 能量表盘 tscn 路径。
    /// </summary>
    public string? EnergyCounterPath { get; init; }

    /// <summary>
    /// 篝火休息动画。
    /// </summary>
    public string? RestSiteAnimPath { get; init; }

    /// <summary>
    /// 商店人物动画。
    /// </summary>
    public string? MerchantAnimPath { get; init; }

    /// <summary>
    /// 人物选择过渡动画。
    /// </summary>
    public string? CharacterSelectTransitionPath { get; init; }

    /// <summary>
    /// 攻击建筑师的攻击特效列表
    /// </summary>
    public List<string>? ArchitectAttackVfx { get; init; }
}

/// <summary>
/// 角色 UI 与图标资源配置
/// </summary>
public record struct CharacterUiAssets
{
    /// <summary>
    /// 人物头像路径。
    /// </summary>
    public string? IconTexturePath { get; init; }

    /// <summary>
    /// 人物头像 2 号。
    /// </summary>
    public string? IconPath { get; init; }

    /// <summary>
    /// 人物选择背景。
    /// </summary>
    public string? CharacterSelectBg { get; init; }

    /// <summary>
    /// 人物选择图标。
    /// </summary>
    public string? CharacterSelectIconPath { get; init; }

    /// <summary>
    /// 人物选择图标 - 锁定状态。
    /// </summary>
    public string? CharacterSelectLockedIconPath { get; init; }

    /// <summary>
    /// 地图上的角色标记图标、表情轮盘上的角色头像
    /// </summary>
    public string? MapMarkerPath { get; init; }
}

/// <summary>
/// 角色音效资源配置
/// </summary>
public record struct CharacterSfxAssets
{
    /// <summary>
    /// 攻击音效
    /// </summary>
    public string? AttackSfx { get; init; }

    /// <summary>
    /// 施法音效
    /// </summary>
    public string? CastSfx { get; init; }

    /// <summary>
    /// 死亡音效
    /// </summary>
    public string? DeathSfx { get; init; }

    /// <summary>
    /// 角色选择音效
    /// </summary>
    public string? CharacterSelectSfx { get; init; }

    /// <summary>
    /// 过渡音效
    /// </summary>
    public string? CharacterTransitionSfx { get; init; }
}

/// <summary>
/// 角色多人模式资源配置
/// </summary>
public record struct CharacterMultiplayerAssets
{
    /// <summary>
    /// 多人模式 - 手指。
    /// </summary>
    public string? ArmPointingTexturePath { get; init; }

    /// <summary>
    /// 多人模式剪刀石头布 - 石头。
    /// </summary>
    public string? ArmRockTexturePath { get; init; }

    /// <summary>
    /// 多人模式剪刀石头布 - 布。
    /// </summary>
    public string? ArmPaperTexturePath { get; init; }

    /// <summary>
    /// 多人模式剪刀石头布 - 剪刀。
    /// </summary>
    public string? ArmScissorsTexturePath { get; init; }
}

/// <summary>
/// 自定义角色基类。
/// 继承此类需要提供各个资源结构体的具体实现。
/// </summary>
public abstract class SuperstitioCharacter : RealPlaceholderCharacterModel
{
    #region 强制子类实现的结构体

    /// <summary>
    /// 获取角色的全套色彩配置。
    /// </summary>
    protected abstract CharacterColorAssets ColorsConfig { get; }

    /// <summary>
    /// 获取角色的基础数值与身份属性。
    /// </summary>
    protected abstract CharacterStatsAssets StatsConfig { get; }

    /// <summary>
    /// 获取角色的战斗资源与初始配置。
    /// </summary>
    protected abstract CharacterLoadoutAssets LoadoutConfig { get; }

    /// <summary>
    /// 获取角色的视觉与动画资源配置。
    /// </summary>
    protected abstract CharacterVisualAssets? VisualAssets { get; }

    /// <summary>
    /// 获取角色的 UI 与图标资源配置。
    /// </summary>
    protected abstract CharacterUiAssets? UiAssets { get; }

    /// <summary>
    /// 获取角色的多人模式相关资源配置。
    /// </summary>
    protected abstract CharacterMultiplayerAssets? MultiplayerAssets { get; }

    /// <summary>
    /// 获取角色的音效资源配置。
    /// </summary>
    protected abstract CharacterSfxAssets? SfxAssets { get; }

    #endregion

    #region 从结构体读取资源 (密封覆盖)

    // --- 色彩 ---

    /// <inheritdoc cref="CharacterColorAssets.NameColor"/>
    public sealed override Color NameColor => this.ColorsConfig.NameColor;

    /// <inheritdoc cref="CharacterColorAssets.EnergyLabelOutlineColor"/>
    public sealed override Color EnergyLabelOutlineColor => this.ColorsConfig.EnergyLabelOutlineColor ?? base.EnergyLabelOutlineColor;

    /// <inheritdoc cref="CharacterColorAssets.DialogueColor"/>
    public sealed override Color DialogueColor => this.ColorsConfig.DialogueColor ?? base.DialogueColor;

    /// <inheritdoc cref="CharacterColorAssets.MapDrawingColor"/>
    public sealed override Color MapDrawingColor => this.ColorsConfig.MapDrawingColor ?? base.MapDrawingColor;

    /// <inheritdoc cref="CharacterColorAssets.RemoteTargetingLineColor"/>
    public sealed override Color RemoteTargetingLineColor => this.ColorsConfig.RemoteTargetingLineColor ?? base.RemoteTargetingLineColor;

    /// <inheritdoc cref="CharacterColorAssets.RemoteTargetingLineOutline"/>
    public sealed override Color RemoteTargetingLineOutline =>
        this.ColorsConfig.RemoteTargetingLineOutline ?? base.RemoteTargetingLineOutline;

    // --- 数值与身份 ---

    /// <inheritdoc cref="CharacterStatsAssets.Gender"/>
    public sealed override CharacterGender Gender => this.StatsConfig.Gender;

    /// <inheritdoc cref="CharacterStatsAssets.StartingHp"/>
    public sealed override int StartingHp => this.StatsConfig.StartingHp;

    /// <inheritdoc cref="CharacterStatsAssets.StartingGold"/>
    public sealed override int StartingGold => this.StatsConfig.StartingGold ?? base.StartingGold;

    /// <inheritdoc cref="CharacterStatsAssets.MaxEnergy"/>
    public sealed override int MaxEnergy => this.StatsConfig.MaxEnergy ?? base.MaxEnergy;

    /// <inheritdoc cref="CharacterStatsAssets.BaseOrbSlotCount"/>
    public sealed override int BaseOrbSlotCount => this.StatsConfig.BaseOrbSlotCount ?? base.BaseOrbSlotCount;

    /// <inheritdoc cref="CharacterStatsAssets.ShouldAlwaysShowStarCounter"/>
    public sealed override bool ShouldAlwaysShowStarCounter =>
        this.StatsConfig.ShouldAlwaysShowStarCounter ?? base.ShouldAlwaysShowStarCounter;

    // --- 加载项与池 ---

    /// <inheritdoc cref="CharacterLoadoutAssets.CardPool"/>
    public sealed override CardPoolModel CardPool => this.LoadoutConfig.CardPool;

    /// <inheritdoc cref="CharacterLoadoutAssets.RelicPool"/>
    public sealed override RelicPoolModel RelicPool => this.LoadoutConfig.RelicPool;

    /// <inheritdoc cref="CharacterLoadoutAssets.PotionPool"/>
    public sealed override PotionPoolModel PotionPool => this.LoadoutConfig.PotionPool;

    /// <inheritdoc cref="CharacterLoadoutAssets.StartingDeck"/>
    public sealed override IEnumerable<CardModel> StartingDeck => this.LoadoutConfig.StartingDeck;

    /// <inheritdoc cref="CharacterLoadoutAssets.StartingRelics"/>
    public sealed override IReadOnlyList<RelicModel> StartingRelics => this.LoadoutConfig.StartingRelics;

    /// <inheritdoc cref="CharacterLoadoutAssets.StartingPotions"/>
    public sealed override IReadOnlyList<PotionModel> StartingPotions => this.LoadoutConfig.StartingPotions ?? base.StartingPotions;

    // --- 视觉与动画 ---

    /// <inheritdoc cref="CharacterVisualAssets.AttackAnimDelay"/>
    public sealed override float AttackAnimDelay => this.VisualAssets?.AttackAnimDelay ?? base.AttackAnimDelay;

    /// <inheritdoc cref="CharacterVisualAssets.CastAnimDelay"/>
    public sealed override float CastAnimDelay => this.VisualAssets?.CastAnimDelay ?? base.CastAnimDelay;

    /// <inheritdoc cref="CharacterVisualAssets.VisualPath"/>
    public sealed override string CustomVisualPath => this.VisualAssets?.VisualPath ?? base.CustomVisualPath;

    /// <inheritdoc cref="CharacterVisualAssets.TrailPath"/>
    public sealed override string CustomTrailPath => this.VisualAssets?.TrailPath ?? base.CustomTrailPath;

    /// <inheritdoc cref="CharacterVisualAssets.EnergyCounterPath"/>
    public sealed override string CustomEnergyCounterPath => this.VisualAssets?.EnergyCounterPath ?? base.CustomEnergyCounterPath;

    /// <inheritdoc cref="CharacterVisualAssets.RestSiteAnimPath"/>
    public sealed override string CustomRestSiteAnimPath => this.VisualAssets?.RestSiteAnimPath ?? base.CustomRestSiteAnimPath;

    /// <inheritdoc cref="CharacterVisualAssets.MerchantAnimPath"/>
    public sealed override string CustomMerchantAnimPath => this.VisualAssets?.MerchantAnimPath ?? base.CustomMerchantAnimPath;

    /// <inheritdoc cref="CharacterVisualAssets.CharacterSelectTransitionPath"/>
    public sealed override string CustomCharacterSelectTransitionPath =>
        this.VisualAssets?.CharacterSelectTransitionPath ?? base.CustomCharacterSelectTransitionPath;

    /// <inheritdoc cref="CharacterVisualAssets.ArchitectAttackVfx"/>
    public sealed override List<string> GetArchitectAttackVfx() => this.VisualAssets?.ArchitectAttackVfx ?? base.GetArchitectAttackVfx();

    // --- UI 与图标 ---

    /// <inheritdoc cref="CharacterUiAssets.IconTexturePath"/>
    public sealed override string? CustomIconTexturePath => this.UiAssets?.IconTexturePath ?? base.CustomIconTexturePath;

    /// <inheritdoc cref="CharacterUiAssets.IconPath"/>
    public sealed override string CustomIconPath => this.UiAssets?.IconPath ?? base.CustomIconPath;

    /// <inheritdoc cref="CharacterUiAssets.CharacterSelectBg"/>
    public sealed override string CustomCharacterSelectBg => this.UiAssets?.CharacterSelectBg ?? base.CustomCharacterSelectBg;

    /// <inheritdoc cref="CharacterUiAssets.CharacterSelectIconPath"/>
    public sealed override string? CustomCharacterSelectIconPath =>
        this.UiAssets?.CharacterSelectIconPath ?? base.CustomCharacterSelectIconPath;

    /// <inheritdoc cref="CharacterUiAssets.CharacterSelectLockedIconPath"/>
    public sealed override string? CustomCharacterSelectLockedIconPath =>
        this.UiAssets?.CharacterSelectLockedIconPath ?? base.CustomCharacterSelectLockedIconPath;

    /// <inheritdoc cref="CharacterUiAssets.MapMarkerPath"/>
    public sealed override string? CustomMapMarkerPath => this.UiAssets?.MapMarkerPath ?? base.CustomMapMarkerPath;

    // --- 多人模式 ---

    /// <inheritdoc cref="CharacterMultiplayerAssets.ArmPointingTexturePath"/>
    public sealed override string CustomArmPointingTexturePath =>
        this.MultiplayerAssets?.ArmPointingTexturePath ?? base.CustomArmPointingTexturePath;

    /// <inheritdoc cref="CharacterMultiplayerAssets.ArmRockTexturePath"/>
    public sealed override string CustomArmRockTexturePath => this.MultiplayerAssets?.ArmRockTexturePath ?? base.CustomArmRockTexturePath;

    /// <inheritdoc cref="CharacterMultiplayerAssets.ArmPaperTexturePath"/>
    public sealed override string CustomArmPaperTexturePath =>
        this.MultiplayerAssets?.ArmPaperTexturePath ?? base.CustomArmPaperTexturePath;

    /// <inheritdoc cref="CharacterMultiplayerAssets.ArmScissorsTexturePath"/>
    public sealed override string CustomArmScissorsTexturePath =>
        this.MultiplayerAssets?.ArmScissorsTexturePath ?? base.CustomArmScissorsTexturePath;

    // --- 音效 ---

    /// <inheritdoc cref="CharacterSfxAssets.AttackSfx"/>
    public sealed override string CustomAttackSfx => this.SfxAssets?.AttackSfx ?? base.CustomAttackSfx;

    /// <inheritdoc cref="CharacterSfxAssets.CastSfx"/>
    public sealed override string CustomCastSfx => this.SfxAssets?.CastSfx ?? base.CustomCastSfx;

    /// <inheritdoc cref="CharacterSfxAssets.DeathSfx"/>
    public sealed override string CustomDeathSfx => this.SfxAssets?.DeathSfx ?? base.CustomDeathSfx;

    /// <inheritdoc cref="CharacterSfxAssets.CharacterSelectSfx"/>
    public sealed override string CharacterSelectSfx => this.SfxAssets?.CharacterSelectSfx ?? base.CharacterSelectSfx;

    /// <inheritdoc cref="CharacterSfxAssets.CharacterTransitionSfx"/>
    public sealed override string CharacterTransitionSfx => this.SfxAssets?.CharacterTransitionSfx ?? base.CharacterTransitionSfx;

    #endregion
}