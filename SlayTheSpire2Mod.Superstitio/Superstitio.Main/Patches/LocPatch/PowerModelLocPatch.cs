using HarmonyLib;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using Superstitio.Analyzer;

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
// ReSharper disable InconsistentNaming

namespace Superstitio.Main.Patches.LocPatch;

/// <summary>
/// 为 Power的描述添加一些额外变量
/// </summary>
[AttachedTo(typeof(PowerModel))]
public interface IAddDumbVariablesToPowerDescription
{
    /// <inheritdoc cref="IAddDumbVariablesToPowerDescription"/>
    /// <param name="description"></param>
    public void AddDumbVariablesToPowerDescription(LocString description);
}

[HarmonyPatch(typeof(PowerModel))]
public class PowerModelLocPatch
{
    [HarmonyPatch("AddDumbVariablesToDescription")]
    [HarmonyPostfix]
    public static void Postfix(PowerModel __instance, LocString description)
    {
        if (__instance is not IAddDumbVariablesToPowerDescription power)
            return;
        power.AddDumbVariablesToPowerDescription(description);
    }
}