using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Saves.Runs;
using Superstitio.Main.Base;
using Superstitio.Main.SubPool.UI;
using Superstitio.Main.Utils;

namespace Superstitio.Main.SubPool;

/// <summary>
/// 卡池选择遗物
/// </summary>
public abstract class CardPoolSelectionRelic : SuperstitioBaseRelic, IHoldCardPoolSelection
{
    /// <inheritdoc />
    public override RelicRarity Rarity => RelicRarity.Starter;

    /// <summary>
    /// 
    /// </summary>
    [SavedProperty]
    protected virtual string SelectedSubPoolIdsRaw
    {
        get;
        set
        {
            this.AssertMutable();
            field = value;
        }
    } = string.Empty;


    /// <summary>
    /// 一个内部标记，用于判断是否已经初始化
    /// </summary>
    [SavedProperty]
    protected virtual bool IsInitialized { get; set; } = false;

    /// <inheritdoc />
    public List<string> SelectedSubPoolIds
    {
        get => string.IsNullOrEmpty(this.SelectedSubPoolIdsRaw)
            ? []
            : this.SelectedSubPoolIdsRaw.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
        set =>
            // 当修改列表时，自动将其拍扁成字符串存入 Raw 属性
            this.SelectedSubPoolIdsRaw = (value.Count == 0) ? string.Empty : string.Join(",", value);
    }

    /// <inheritdoc />
    /// 在遗物被添加到玩家时，如果是新游戏（不是读档），就从 SubPoolManager 读取配置
    public override async Task AfterObtained()
    {
        await base.AfterObtained();

        if (!this.IsInitialized)
        {
            var enabledSubPools = SubPoolManager.GetEnabledSubPools(this.Owner.Character.CardPool.GetType());
            this.SelectedSubPoolIds = enabledSubPools
                .Select(pool => pool.Id)
                .ToList();

            // 3. 初始化完成后，立刻设为 true
            this.IsInitialized = true;

            Log.Info($"[MasoMod] 遗物 {this.Id.Entry} 首次获得，完成初始化。");
        }
        else
        {
            Log.Info($"[MasoMod] 遗物 {this.Id.Entry} 从存档加载，跳过初始化。数量: {this.SelectedSubPoolIds.Count}");
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            if (this.SelectedSubPoolIds.Count == 0)
                return base.ExtraHoverTips;

            string poolList = string.Join("\n", this.SelectedSubPoolIds.Select(id => $"• {GetSubPoolName(id)}"));
            return
            [
                new HoverTip(
                    SuperstitioLocStringFactory.ExtendLocString("CARD_POOL", "tip", "subpoolListTip", "title"),
                    poolList
                )
            ];
        }
    }

    private static string GetSubPoolName(string poolId)
    {
        var loc = SuperstitioLocStringFactory.ExtendLocString($"SUBPOOL_{poolId}", "name");
        return loc.Exists() ? loc.GetFormattedText() : poolId;
    }
}

/// <summary>
/// 卡池选择遗物接口
/// </summary>
public interface IHoldCardPoolSelection
{
    /// <summary>
    /// 已选择的子卡池 ID 列表
    /// </summary>
    List<string> SelectedSubPoolIds { get; set; }
}