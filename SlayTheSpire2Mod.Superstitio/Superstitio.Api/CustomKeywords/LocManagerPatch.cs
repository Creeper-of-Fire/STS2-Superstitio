using HarmonyLib;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Superstitio.Analyzer;

// ReSharper disable InconsistentNaming
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释

namespace Superstitio.Api.CustomKeywords;

// [HarmonyPatch(typeof(LocManager), "LoadLocFormatters")]
// public static class LocManager_LoadLocFormatters_Patch
// {
//     static void Postfix()
//     {
//
//         // 直接通过 Smart.Default 获取 SmartFormatter 实例
//         var smartFormatter = Smart.Default;
//
//         // 创建你自己的 formatter
//         var myCustomFormatter = new MyCustomFormatter();
//
//         // 直接使用 InsertExtension 插入到最前面（位置 0）
//         smartFormatter.InsertExtension(0, myCustomFormatter);
//     }
// }

[HarmonyPatch(typeof(DynamicVar), nameof(DynamicVar.ToHighlightedString))]
public static class DynamicVar_ToHighlightedString_Patch
{
    static bool Prefix(DynamicVar __instance, bool inverse, ref string __result)
    {
        // 检查是否实现了 IHighlightableDynamicVar 接口
        if (__instance is IHighlightableDynamicVar highlightable)
        {
            __result = highlightable.OverrideToHighlightedString(inverse);
            return false; // 跳过原方法
        }
        
        return true; // 继续执行原方法
    }
}

[AttachedTo(typeof(DynamicVar))]
public interface IHighlightableDynamicVar
{
    string OverrideToHighlightedString(bool inverse);
}