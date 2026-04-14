using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;

namespace Superstitio.Main.Base;

/// <summary>
/// 角色全套色彩配置。
/// </summary>
public record CharacterColorAssets
{
    /// <summary>
    /// 角色名称颜色。
    /// </summary>
    public required Color NameColor { get; init; }

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
public record CharacterStatsAssets
{
    /// <summary>
    /// 人物性别。
    /// </summary>
    public required CharacterGender Gender { get; init; }

    /// <summary>
    /// 初始血量。
    /// </summary>
    public required int StartingHp { get; init; }

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
public record CharacterLoadoutAssets
{
    /// <summary>
    /// 卡牌池。
    /// </summary>
    public required CardPoolModel CardPool { get; init; }

    /// <summary>
    /// 遗物池。
    /// </summary>
    public required RelicPoolModel RelicPool { get; init; }

    /// <summary>
    /// 药水池。
    /// </summary>
    public required PotionPoolModel PotionPool { get; init; }

    /// <summary>
    /// 初始卡组。
    /// </summary>
    public required IEnumerable<CardModel> StartingDeck { get; init; }

    /// <summary>
    /// 初始遗物。
    /// </summary>
    public required IReadOnlyList<RelicModel> StartingRelics { get; init; }

    /// <summary>
    /// 初始药水。
    /// </summary>
    public IReadOnlyList<PotionModel>? StartingPotions { get; init; }
}

/// <summary>
/// 角色视觉与动画资源配置
/// </summary>
public record CharacterVisualAssets
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
public record CharacterUiAssets
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
public record CharacterSfxAssets
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
public record CharacterMultiplayerAssets
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
public abstract class SuperstitioCharacter : PlaceholderCharacterModel
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
    public override Color NameColor => this.ColorsConfig.NameColor;

    /// <inheritdoc cref="CharacterColorAssets.EnergyLabelOutlineColor"/>
    public override Color EnergyLabelOutlineColor => this.ColorsConfig.EnergyLabelOutlineColor ?? base.EnergyLabelOutlineColor;

    /// <inheritdoc cref="CharacterColorAssets.DialogueColor"/>
    public override Color DialogueColor => this.ColorsConfig.DialogueColor ?? base.DialogueColor;

    /// <inheritdoc cref="CharacterColorAssets.MapDrawingColor"/>
    public override Color MapDrawingColor => this.ColorsConfig.MapDrawingColor ?? base.MapDrawingColor;

    /// <inheritdoc cref="CharacterColorAssets.RemoteTargetingLineColor"/>
    public override Color RemoteTargetingLineColor => this.ColorsConfig.RemoteTargetingLineColor ?? base.RemoteTargetingLineColor;

    /// <inheritdoc cref="CharacterColorAssets.RemoteTargetingLineOutline"/>
    public override Color RemoteTargetingLineOutline =>
        this.ColorsConfig.RemoteTargetingLineOutline ?? base.RemoteTargetingLineOutline;

    // --- 数值与身份 ---

    /// <inheritdoc cref="CharacterStatsAssets.Gender"/>
    public override CharacterGender Gender => this.StatsConfig.Gender;

    /// <inheritdoc cref="CharacterStatsAssets.StartingHp"/>
    public override int StartingHp => this.StatsConfig.StartingHp;

    /// <inheritdoc cref="CharacterStatsAssets.StartingGold"/>
    public override int StartingGold => this.StatsConfig.StartingGold ?? base.StartingGold;

    /// <inheritdoc cref="CharacterStatsAssets.MaxEnergy"/>
    public override int MaxEnergy => this.StatsConfig.MaxEnergy ?? base.MaxEnergy;

    /// <inheritdoc cref="CharacterStatsAssets.BaseOrbSlotCount"/>
    public override int BaseOrbSlotCount => this.StatsConfig.BaseOrbSlotCount ?? base.BaseOrbSlotCount;

    /// <inheritdoc cref="CharacterStatsAssets.ShouldAlwaysShowStarCounter"/>
    public override bool ShouldAlwaysShowStarCounter =>
        this.StatsConfig.ShouldAlwaysShowStarCounter ?? base.ShouldAlwaysShowStarCounter;

    // --- 加载项与池 ---

    /// <inheritdoc cref="CharacterLoadoutAssets.CardPool"/>
    public override CardPoolModel CardPool => this.LoadoutConfig.CardPool;

    /// <inheritdoc cref="CharacterLoadoutAssets.RelicPool"/>
    public override RelicPoolModel RelicPool => this.LoadoutConfig.RelicPool;

    /// <inheritdoc cref="CharacterLoadoutAssets.PotionPool"/>
    public override PotionPoolModel PotionPool => this.LoadoutConfig.PotionPool;

    /// <inheritdoc cref="CharacterLoadoutAssets.StartingDeck"/>
    public override IEnumerable<CardModel> StartingDeck => this.LoadoutConfig.StartingDeck;

    /// <inheritdoc cref="CharacterLoadoutAssets.StartingRelics"/>
    public override IReadOnlyList<RelicModel> StartingRelics => this.LoadoutConfig.StartingRelics;

    /// <inheritdoc cref="CharacterLoadoutAssets.StartingPotions"/>
    public override IReadOnlyList<PotionModel> StartingPotions => this.LoadoutConfig.StartingPotions ?? base.StartingPotions;

    // --- 视觉与动画 ---

    /// <inheritdoc cref="CharacterVisualAssets.AttackAnimDelay"/>
    public override float AttackAnimDelay => this.VisualAssets?.AttackAnimDelay ?? base.AttackAnimDelay;

    /// <inheritdoc cref="CharacterVisualAssets.CastAnimDelay"/>
    public override float CastAnimDelay => this.VisualAssets?.CastAnimDelay ?? base.CastAnimDelay;

    /// <inheritdoc cref="CharacterVisualAssets.VisualPath"/>
    public override string CustomVisualPath => this.VisualAssets?.VisualPath ?? base.CustomVisualPath;

    /// <inheritdoc cref="CharacterVisualAssets.TrailPath"/>
    public override string CustomTrailPath => this.VisualAssets?.TrailPath ?? base.CustomTrailPath;

    /// <inheritdoc cref="CharacterVisualAssets.EnergyCounterPath"/>
    public override string CustomEnergyCounterPath => this.VisualAssets?.EnergyCounterPath ?? base.CustomEnergyCounterPath;

    /// <inheritdoc cref="CharacterVisualAssets.RestSiteAnimPath"/>
    public override string CustomRestSiteAnimPath => this.VisualAssets?.RestSiteAnimPath ?? base.CustomRestSiteAnimPath;

    /// <inheritdoc cref="CharacterVisualAssets.MerchantAnimPath"/>
    public override string CustomMerchantAnimPath => this.VisualAssets?.MerchantAnimPath ?? base.CustomMerchantAnimPath;

    /// <inheritdoc cref="CharacterVisualAssets.CharacterSelectTransitionPath"/>
    public override string CustomCharacterSelectTransitionPath =>
        this.VisualAssets?.CharacterSelectTransitionPath ?? base.CustomCharacterSelectTransitionPath;

    /// <inheritdoc cref="CharacterVisualAssets.ArchitectAttackVfx"/>
    public override List<string> GetArchitectAttackVfx() => this.VisualAssets?.ArchitectAttackVfx ?? base.GetArchitectAttackVfx();

    // --- UI 与图标 ---

    /// <inheritdoc cref="CharacterUiAssets.IconTexturePath"/>
    public override string? CustomIconTexturePath => this.UiAssets?.IconTexturePath ?? base.CustomIconTexturePath;

    /// <inheritdoc cref="CharacterUiAssets.IconPath"/>
    public override string CustomIconPath => this.UiAssets?.IconPath ?? base.CustomIconPath;

    /// <inheritdoc cref="CharacterUiAssets.CharacterSelectBg"/>
    public override string CustomCharacterSelectBg => this.UiAssets?.CharacterSelectBg ?? base.CustomCharacterSelectBg;

    /// <inheritdoc cref="CharacterUiAssets.CharacterSelectIconPath"/>
    public override string? CustomCharacterSelectIconPath =>
        this.UiAssets?.CharacterSelectIconPath ?? base.CustomCharacterSelectIconPath;

    /// <inheritdoc cref="CharacterUiAssets.CharacterSelectLockedIconPath"/>
    public override string? CustomCharacterSelectLockedIconPath =>
        this.UiAssets?.CharacterSelectLockedIconPath ?? base.CustomCharacterSelectLockedIconPath;

    /// <inheritdoc cref="CharacterUiAssets.MapMarkerPath"/>
    public override string? CustomMapMarkerPath => this.UiAssets?.MapMarkerPath ?? base.CustomMapMarkerPath;

    // --- 多人模式 ---

    /// <inheritdoc cref="CharacterMultiplayerAssets.ArmPointingTexturePath"/>
    public override string CustomArmPointingTexturePath =>
        this.MultiplayerAssets?.ArmPointingTexturePath ?? base.CustomArmPointingTexturePath;

    /// <inheritdoc cref="CharacterMultiplayerAssets.ArmRockTexturePath"/>
    public override string CustomArmRockTexturePath => this.MultiplayerAssets?.ArmRockTexturePath ?? base.CustomArmRockTexturePath;

    /// <inheritdoc cref="CharacterMultiplayerAssets.ArmPaperTexturePath"/>
    public override string CustomArmPaperTexturePath =>
        this.MultiplayerAssets?.ArmPaperTexturePath ?? base.CustomArmPaperTexturePath;

    /// <inheritdoc cref="CharacterMultiplayerAssets.ArmScissorsTexturePath"/>
    public override string CustomArmScissorsTexturePath =>
        this.MultiplayerAssets?.ArmScissorsTexturePath ?? base.CustomArmScissorsTexturePath;

    // --- 音效 ---

    /// <inheritdoc cref="CharacterSfxAssets.AttackSfx"/>
    public override string CustomAttackSfx => this.SfxAssets?.AttackSfx ?? base.CustomAttackSfx;

    /// <inheritdoc cref="CharacterSfxAssets.CastSfx"/>
    public override string CustomCastSfx => this.SfxAssets?.CastSfx ?? base.CustomCastSfx;

    /// <inheritdoc cref="CharacterSfxAssets.DeathSfx"/>
    public override string CustomDeathSfx => this.SfxAssets?.DeathSfx ?? base.CustomDeathSfx;

    /// <inheritdoc cref="CharacterSfxAssets.CharacterSelectSfx"/>
    public override string CharacterSelectSfx => this.SfxAssets?.CharacterSelectSfx ?? base.CharacterSelectSfx;

    /// <inheritdoc cref="CharacterSfxAssets.CharacterTransitionSfx"/>
    public override string CharacterTransitionSfx => this.SfxAssets?.CharacterTransitionSfx ?? PlaceholderCharacterTransitionSfx;

    /// <summary>
    /// 获取角色的过渡音效
    /// 从角色配置中读取，若未配置则使用默认值
    /// </summary>
    private const string PlaceholderCharacterTransitionSfx = "event:/sfx/ui/wipe_ironclad";

    #endregion
}