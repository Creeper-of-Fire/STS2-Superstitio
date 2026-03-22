using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves.Runs;
using Superstitio.Main.Maso.Pools;
using Superstitio.Main.SubPool.UI;

namespace Superstitio.Main.SubPool;

#pragma warning disable CS1591
/// <summary>
/// 卡池选择遗物
/// </summary>
[Pool(typeof(MasoRelicPool))]
public class CardPoolSelectionRelic : RelicModel, IHoldCardPoolSelection
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    public override bool ShouldReceiveCombatHooks => false;

    [SavedProperty]
    public List<string> SelectedSubPoolIds
    {
        get;
        set
        {
            this.AssertMutable();
            field = value;
        }
    } = [];

    // 在遗物被添加到玩家时，如果是新游戏（不是读档），就从 SubPoolManager 读取配置
    public override async Task AfterObtained()
    {
        await base.AfterObtained();

        if (this.SelectedSubPoolIds.Count == 0)
        {
            var enabledSubPools = SubPoolManager.GetEnabledSubPools(this.Owner.Character.CardPool.GetType());
            this.SelectedSubPoolIds = enabledSubPools
                .Select(pool => pool.Id)
                .ToList();

            Log.Info($"[MasoMod] 遗物 {this.Id.Entry} 初始化，读取了 {this.SelectedSubPoolIds.Count} 个子池选择");
        }
    }

    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            if (this.SelectedSubPoolIds.Count == 0) yield break;

            var poolList = string.Join("\n", this.SelectedSubPoolIds.Select(id => $"• {GetSubPoolName(id)}"));
            yield return new HoverTip(
                this.Title,
                poolList
            );
        }
    }

    private static string GetSubPoolName(string poolId)
    {
        return poolId;
        // var loc = new LocString("superstitio", $"subpool_{poolId}.name");
        // return loc.Exists() ? loc.GetFormattedText() : poolId;
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